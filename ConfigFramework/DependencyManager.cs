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

using Prosoft.FXMGR.Metadata;
using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace Prosoft.FXMGR.ConfigFramework
{
    using FxInterface = KeyValuePair<string, string>;

    public class DependencyManager
    {
		//
		// Set of static methods converting YAML nodes into semantic values.
		// These methods may throw FormatException in case when given YAML node has incorrect format.
		//
		private class MetadataFormat
		{
			//--------------------------------------------------------------------------------
			/// <summary>
			/// Extracts interface export information from given source or header file.
			/// </summary>
			/// <exception cref="System.FormatException">Throws when interface describing metadata is in wrong format.</exception>
			/// 
			public static FxInterface GetInterfaceExport(string file, YamlDocument doc)
			{
				YamlNode exportEntry = null;

				if (doc.RootNode is YamlMappingNode)
				{
					YamlMappingNode mappingNode = (YamlMappingNode)doc.RootNode;
					YamlNode interfaceEntry = MetadataProvider.QueryMetadata(mappingNode, "interface");
					YamlNode implementationEntry = MetadataProvider.QueryMetadata(mappingNode, "implementation");

					exportEntry = interfaceEntry ?? implementationEntry;
				}

				if (exportEntry != null) 
				{
					try
					{
						YamlSequenceNode interfaceString = (YamlSequenceNode)exportEntry;
						YamlScalarNode abstractName = (YamlScalarNode)interfaceString.Children[0];
						YamlScalarNode implName = (YamlScalarNode)interfaceString.Children[1];
						return new FxInterface(abstractName.Value, implName.Value);
					}
					catch (InvalidCastException e)
					{
                        throw new FormatException(
                            String.Format("Wrong metadata format in file: {0}", file), e.InnerException);
					}
					catch (ArgumentOutOfRangeException e)
					{
                        throw new FormatException(
                            String.Format("Wrong metadata format in file: {0}", file), e.InnerException);
					}
				}
				else
				{
					return new FxInterface();
				}
			}

			//--------------------------------------------------------------------------------
			/// <summary>
			/// Extracts dependency information from given source or header file.
			/// </summary>
			/// <exception cref="System.FormatException">Throws when interface describing metadata is in wrong format.</exception>
			/// 
			public static IEnumerable<string> GetInterfaceDependency(string file, YamlDocument doc)
			{
				YamlNode dependencyMetadataEntry = null;

				if (doc.RootNode is YamlMappingNode)
				{
					YamlMappingNode rootNode = (YamlMappingNode)doc.RootNode;
					dependencyMetadataEntry = MetadataProvider.QueryMetadata(rootNode, "dependencies");
				}

				if (dependencyMetadataEntry != null)
				{
					try
					{
						HashSet<string> dependencies = new HashSet<string>();
						YamlSequenceNode dependenciesSequence = (YamlSequenceNode)dependencyMetadataEntry;

						foreach (YamlNode d in dependenciesSequence.Children)
						{
							YamlScalarNode abstractName = (YamlScalarNode)d;
							dependencies.Add(abstractName.Value);
						}

						return dependencies;
					}
					catch (InvalidCastException)
					{
                        throw new FormatException(
                            String.Format("Wrong metadata format in file: {0}", file));
					}
				}

				return null;
			}
		}

		//--------------------------------------------------------------------------------

        private IDictionary<string, HashSet<string>> abstractToImpl = new Dictionary<string, HashSet<string>>();
        private IDictionary<FxInterface, HashSet<string>> implToDependencies = new Dictionary<FxInterface, HashSet<string>>();

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Create dependency manager and fills internal structures by analyzing metadata.
		/// </summary>
		/// <exception cref="System.FormatException">Throws when unknown interface is specified.</exception>
		/// <exception cref="System.ArgumentNullException">Throws when null-ref metadata is specified.</exception>
		/// 
        public DependencyManager(IEnumerable<KeyValuePair<string, YamlStream>> metadata)
        {
			if (metadata == null)
				throw new ArgumentNullException();

            foreach (KeyValuePair<string, YamlStream> item in metadata)
            {
                FxInterface exportedInterface = GetAssociatedInterface(item);

                if (exportedInterface.Key != null)
                {
                    IEnumerable<string> dependencies = GetInterfaceDependencies(item);

                    AddToDictionary(abstractToImpl, exportedInterface.Key, exportedInterface.Value);

                    foreach (string d in dependencies)
                    {
                        if (exportedInterface.Key != d)
                        {
                            AddToDictionary(implToDependencies, exportedInterface, d);
                        }
                    }
                }
            }
        }

        private void AddToDictionary<K, V>(IDictionary<K, HashSet<V>> dict, K key, V val)
        {
            HashSet<V> temp;

            if (!dict.TryGetValue(key, out temp))
            {
                temp = new HashSet<V>();
                temp.Add(val);
                dict[key] = temp;
            }
            else
            {
                temp.Add(val);
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return interface associated with given metadata item.
        /// </summary>
		/// <exception cref="System.FormatException">Throws when interface describing metadata is in wrong format.</exception>
		/// 
        public static FxInterface GetAssociatedInterface(KeyValuePair<string, YamlStream> metadataItem)
        {
            foreach (YamlDocument metadataEntry in metadataItem.Value.Documents)
            {
				FxInterface exportInterface = MetadataFormat.GetInterfaceExport(metadataItem.Key, metadataEntry);

				if (exportInterface.Key != null)
                {
					return exportInterface;
                }
            }

            return new FxInterface();
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return dependencies for given metadata item.
        /// </summary>
		/// <exception cref="System.FormatException">Throws when interface describing metadata is in wrong format.</exception>
		/// 
        private IEnumerable<string> GetInterfaceDependencies(KeyValuePair<string, YamlStream> metadataItem)
        {
            foreach (YamlDocument document in metadataItem.Value.Documents)
            {
				IEnumerable<string> dependencies = MetadataFormat.GetInterfaceDependency(metadataItem.Key, document);

				if (dependencies != null)
                {
					return dependencies;
                }
            }

            return new HashSet<string>();
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Get available modules (abstract interfaces).
        /// </summary>
        public IEnumerable<string> GetAbstractInterfaces()
        {
            return abstractToImpl.Keys;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Get available implementations for given module.
        /// </summary>
		/// <exception cref="System.ArgumentException">Throws when unknown interface is specified.</exception>
		/// 
        public IEnumerable<string> GetAvailImplementations(string intrface)
        {
			HashSet<string> implementations;

            if(!abstractToImpl.TryGetValue(intrface, out implementations))
			{
                throw new ArgumentException("Using an unknown module " + intrface);
			}

			return implementations;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Get dependencies for given interface name.
        /// </summary>
        public IEnumerable<string> GetInterfaceDependencies(FxInterface fullName)
        {
            HashSet<string> result;

            return implToDependencies.TryGetValue(fullName, out result) ? result : new HashSet<string>();
        }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Checks dependency graph consistency (all dependencies must have corresponding node).
		/// </summary>
		public IEnumerable<string> GetUnknownImports()
		{
			List<string> unknownImports = new List<string>();
			List<string> imports = new List<string>();

			foreach(var m in implToDependencies.Values)
			{
				imports.AddRange(m);
			}

			HashSet<string> uniqueImports = new HashSet<string>(imports);

			foreach(var m in uniqueImports)
			{
				if(!abstractToImpl.ContainsKey(m))
				{
					unknownImports.Add(m);
				}
			}

			return unknownImports;
		}
    }
}
