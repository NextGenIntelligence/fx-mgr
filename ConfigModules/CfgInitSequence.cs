/*
 * Copyright (c) 2010-2015, Eremex Ltd.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions 
 * are met:
 * 1. Redistributions of source code must retain the above copyright 
 *    notice, this list of conditions and the following disclaimer.  
 * 2. Redistributions in binary form must reproduce the above copyright 
 *    notice, this list of conditions and the following disclaimer in the 
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the company nor the names of any contributors 
 *    may be used to endorse or promote products derived from this software 
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE EREMEX LTD. AND CONTRIBUTORS "AS IS" AND 
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE EREMEX LTD. OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS 
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using Prosoft.FXMGR.ConfigFramework;
using Prosoft.FXMGR.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Prosoft.FXMGR.ConfigModules
{
    using System.Text;
    using FxInterface = KeyValuePair<string, string>;

    public class CfgInitSequence
    {
        private readonly DependencyManager dependencyManager;
        private readonly IEnumerable<KeyValuePair<string, YamlStream>> metadata;
        private readonly InterfaceTranslator translator;

        public CfgInitSequence(DependencyManager dm, InterfaceTranslator translator, IEnumerable<KeyValuePair<string, YamlStream>> metadata)
        {
            if (dm == null || translator == null || metadata == null)
                throw new ArgumentNullException();

            this.dependencyManager = dm;
            this.metadata = metadata;
            this.translator = translator;
        }

        private class CtorFormat
        {
            //--------------------------------------------------------------------------------
            /// <summary>
            /// Getting constructor info from file metadata.
            /// </summary>
            /// <exception cref="System.FormatException">Throws when metadata is in wrong format.</exception>
            /// 
            public static KeyValuePair<string, bool> GetCtorInfo(KeyValuePair<string, YamlStream> metadataItem)
            {
                YamlNode exportEntry = null;

                try
                {
                    //
                    // Lookup for "ctor" metadata entry, break on first match.
                    //
                    foreach (YamlDocument metadataEntry in metadataItem.Value.Documents)
                    {
                        YamlMappingNode mappingNode = (YamlMappingNode)metadataEntry.RootNode;
                        exportEntry = MetadataProvider.QueryMetadata(mappingNode, "ctor");
                        if (exportEntry != null) break;
                    }

                    if (exportEntry != null)
                    {
                        YamlSequenceNode ctorInfoString = (YamlSequenceNode)exportEntry;
                        YamlScalarNode ctorName = (YamlScalarNode)ctorInfoString.Children[0];
                        YamlScalarNode ctorType = (YamlScalarNode)ctorInfoString.Children[1];
                        return new KeyValuePair<string, bool>(ctorName.Value, ctorType.Value == "on_each_cpu");
                    }
                    else
                    {
                        return new KeyValuePair<string, bool>("", false);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw new FormatException("Wrong ctor metadata entry.");
                }
                catch (InvalidCastException)
                {
                    throw new FormatException("Wrong ctor metadata entry.");
                }
                catch (NullReferenceException)
                {
                    throw new FormatException("Wrong ctor metadata entry.");
                }
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Builds module->ctor_information mapping.
        /// </summary>
        /// <exception cref="System.FormatException">Throws when metadata is in wrong format.</exception>
        /// 
        private IDictionary<string, KeyValuePair<string, bool>> GetCtorMap(IEnumerable<FxInterface> included)
        {
            Dictionary<string, KeyValuePair<string, bool>> ctorMap = new Dictionary<string, KeyValuePair<string, bool>>();
            HashSet<FxInterface> includedModules = new HashSet<FxInterface>(included);

            foreach (KeyValuePair<string, YamlStream> metadataItem in metadata)
            {
                if (Path.GetExtension(metadataItem.Key) == ".h")
                {
                    FxInterface module = DependencyManager.GetAssociatedInterface(metadataItem);

                    if (includedModules.Contains(module))
                    {
                        ctorMap[module.Key] = CtorFormat.GetCtorInfo(metadataItem);
                    }
                }
            }

            return ctorMap;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Init graph item.
        /// </summary>
        private class InitGraphItem
        {
            public List<string> dependencies { get; set; }
            public bool open { get; set; }
            public int orderNum { get; set; }
            public KeyValuePair<string, bool> ctorInfo { get; set; }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Get order number for given init graph element.
        /// </summary>
        private static int GetOrder(IDictionary<string, InitGraphItem> moduleGraph, InitGraphItem target)
        {
            //
            // Items with no dependencies get order number zero.
            //
            if (target.dependencies.Count() == 0) target.orderNum = 0;

            //
            // If order number has already been calculated - just return it.
            //
            if (target.orderNum >= 0) return target.orderNum;

            int order = -1;

            foreach (var module in target.dependencies)
            {
                if (moduleGraph[module].open)
                {
                    StringBuilder sb = new StringBuilder("Initialization graph cycle is detected: ");

                    var cycle = moduleGraph.Where(x => x.Value.open).Select(x => x.Key);

                    foreach (var m in cycle)
                    {
                        sb.Append(m + "; ");
                    }

                    throw new InvalidOperationException(sb.ToString());
                }

                moduleGraph[module].open = true;
                int tempOrder = GetOrder(moduleGraph, moduleGraph[module]) + 1;
                moduleGraph[module].open = false;

                if (tempOrder > order)
                {
                    order = tempOrder;
                }
            }
            return order;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Get full dependencies (including indirect) for given interface name.
        /// </summary>
        /// <exception cref="System.ArgumentException">Unknown interface is used.</exception>
        /// 
        private IEnumerable<string> GetAllInterfaceDependencies(FxInterface fullName)
        {
            HashSet<string> dependencies = new HashSet<string>();
            List<FxInterface> imports = new List<FxInterface>();
            List<FxInterface> next = new List<FxInterface>();

            imports.Add(fullName);

            //
            // Populate full dependencies list recursively.
            //
            do
            {
                foreach (var intrface in imports)
                {
                    var directDependencies = dependencyManager.GetInterfaceDependencies(intrface);

                    foreach (var d in directDependencies)
                    {
                        //
                        // Possible graph cycles is avoided due to rule, that only new 
                        // (not processed before) interfaces are going to next iteration.
                        // So, algorithm hang is impossible, even when target module list
                        // contains cycles in dependency graph.
                        //
                        if (!dependencies.Contains(d))
                        {
                            dependencies.Add(d);
                            next.Add(translator.TranslateAbstractInterface(d));
                        }
                    }
                }

                imports.Clear();
                imports.AddRange(next);
                next.Clear();
            }
            while (imports.Count > 0);

            return dependencies;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Get init sequence as sequence of string representing constructors.
        /// </summary>
        /// <exception cref="System.ArgumentException">Unknown interface is used.</exception>
        /// <exception cref="System.FormatException">Throws when metadata is in wrong format.</exception>
        /// 
        public IEnumerable<KeyValuePair<string, bool>> GetInitSequence(IEnumerable<FxInterface> included, out IEnumerable<string> modules)
        {
            IDictionary<string, InitGraphItem> initGraph = new Dictionary<string, InitGraphItem>();
            IDictionary<string, KeyValuePair<string, bool>> ctorInfo = GetCtorMap(included);
            HashSet<string> modulesWithCtor = new HashSet<string>(ctorInfo.Where(x => x.Value.Key != "").Select(x => x.Key));

            modules = modulesWithCtor;

            //
            // Fill dependency graph.
            //
            foreach (var m in modulesWithCtor)
            {
                var allDependencies = GetAllInterfaceDependencies(translator.TranslateAbstractInterface(m));
                var dependenciesWithCtor = new List<string>(allDependencies.Where(x => modulesWithCtor.Contains(x)).Select(x => x));
                initGraph[m] = new InitGraphItem() { orderNum = -1, open = false, ctorInfo = ctorInfo[m], dependencies = dependenciesWithCtor };
            }

            //
            // Recursively get init order for every module in graph.
            //
            foreach (var module in initGraph.Values)
            {
                module.orderNum = GetOrder(initGraph, module);
            }

            //
            // Sort array by order number.
            //
            var initSequence = initGraph.Values.OrderBy(x => x.orderNum).Select(x => x.ctorInfo);

            return initSequence;
        }
    }
}
