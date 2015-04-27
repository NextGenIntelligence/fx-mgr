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
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Prosoft.FXMGR.ConfigModules
{
    using FxInterface = KeyValuePair<string, string>;

    public class CfgAspects
    {
        private readonly IEnumerable<KeyValuePair<string, YamlStream>> metadata;

        private class AspectsFormat
        {
            public static IEnumerable<KeyValuePair<string, IEnumerable<string>>> GetModuleAspects(KeyValuePair<string, YamlStream> metadataItem)
            {
                List<KeyValuePair<string, IEnumerable<string>>> aspects = new List<System.Collections.Generic.KeyValuePair<string, IEnumerable<string>>>();

                try
                {
                    //
                    // Lookup for "aspects" metadata entry.
                    //
                    foreach (YamlDocument metadataEntry in metadataItem.Value.Documents)
                    {
                        YamlMappingNode mappingNode = (YamlMappingNode)metadataEntry.RootNode;
                        YamlNode exportEntry = MetadataProvider.QueryMetadata(mappingNode, "aspects");

                        if (exportEntry != null)
                        {
                            YamlSequenceNode aspectInfoString = (YamlSequenceNode)exportEntry;

                            foreach (YamlNode aspectDescriptor in aspectInfoString.Children)
                            {
                                YamlMappingNode aspectDict = (YamlMappingNode)aspectDescriptor;
                                YamlScalarNode aspectKeyNode = (YamlScalarNode)aspectDict.Children.Keys.First();
                                string aspectKey = aspectKeyNode.Value;
                                YamlSequenceNode aspectVal = (YamlSequenceNode)aspectDict.Children.Values.First();
                                string[] aspectVals = new string[aspectVal.Children.Count];

                                for (int i = 0; i < aspectVal.Children.Count; ++i)
                                {
                                    YamlScalarNode aspect = (YamlScalarNode)aspectVal.Children[i];
                                    aspectVals[i] = aspect.Value;
                                }

                                aspects.Add(new KeyValuePair<string, IEnumerable<string>>(aspectKey, aspectVals));
                            }
                        }
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw new FormatException("Wrong aspect metadata entry.");
                }
                catch (InvalidCastException)
                {
                    throw new FormatException("Wrong aspect metadata entry.");
                }
                catch (NullReferenceException)
                {
                    throw new FormatException("Wrong aspect metadata entry.");
                }

                return aspects;
            }
        }

        public CfgAspects(IEnumerable<KeyValuePair<string, YamlStream>> metadata)
        {
            if (metadata == null)
                throw new System.ArgumentNullException();

            this.metadata = metadata;
        }

        /// <summary>
        /// Getting list of aspects contained in metadata. 
        /// </summary>
        /// <exception cref="System.FormatException">Throws when interface describing metadata is in wrong format.</exception>
        /// 
        public IEnumerable<KeyValuePair<string, List<string>>> GetAspects(IEnumerable<FxInterface> includedModules)
        {
            HashSet<FxInterface> requiredModules = new HashSet<FxInterface>(includedModules);
            List<KeyValuePair<string, IEnumerable<string>>> aspects = new List<KeyValuePair<string, IEnumerable<string>>>();

            foreach (KeyValuePair<string, YamlStream> metadataItem in metadata)
            {
                FxInterface moduleName = DependencyManager.GetAssociatedInterface(metadataItem);

                if (requiredModules.Contains(moduleName))
                {
                    aspects.AddRange(AspectsFormat.GetModuleAspects(metadataItem));
                }
            }

            Dictionary<string, List<string>> mergedAspects = new Dictionary<string, List<string>>();

            foreach (var a in aspects)
            {
                List<string> aspectSet;

                if (mergedAspects.TryGetValue(a.Key, out aspectSet))
                {
                    aspectSet.AddRange(a.Value);
                }
                else
                {
                    mergedAspects[a.Key] = new List<string>(a.Value);
                }
            }

            return mergedAspects.ToList();
        }
    }
}
