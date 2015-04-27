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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace Prosoft.FXMGR.Metadata
{
    public class MetadataProvider
    {
        private class RawMetadata
        {
            public IList<string> includes = new List<string>();
            public IList<string> metadataEntries = new List<string>();
        }

        private static Regex multilineComments = new Regex("/\\*.*?\\*/", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex singlelineComments = new Regex("//.*?\n", RegexOptions.Compiled);
		private static Regex metadataRegex = new Regex("FX_METADATA\\s*\\(\\s*\\((.+?)\\)\\s*\\)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex includeRegex = new Regex("#\\s*include\\s+FX_INTERFACE\\(\\s*(\\w+)\\s*\\)", RegexOptions.Compiled);

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Extracts metadata and import information from given source or header file.
        /// </summary>
		/// <exception cref="System.ArgumentException">Throws when target file is incorrect or non-readable.</exception>
		/// <exception cref="System.ObjectDisposedException">Trying to read the file which is disposed.</exception>
		/// <exception cref="System.ArgumentNullException">Null reference when metadata reading or transformations.</exception>
		/// <exception cref="System.UnauthorizedAccessException">Specified file cannot be read.</exception>
		/// <exception cref="System.IOException">Specified file cannot be read.</exception>
		/// <exception cref="System.NotSupportedException">Operation is not supported.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Specified file is too large and cannot be read.</exception>
		/// <exception cref="System.OutOfMemoryException">Specified file is too large and cannot be read.</exception>
		/// 
		private static RawMetadata GetRawMetadataEntries(string path)
        {
            RawMetadata rawMetadata = new RawMetadata();

			string fileContent = File.ReadAllText(path);

			//
			// Remove comments from source or header.
			//
			fileContent = multilineComments.Replace(fileContent, "");
			fileContent = singlelineComments.Replace(fileContent, "");

			MatchCollection metadata = metadataRegex.Matches(fileContent);
			MatchCollection includes = includeRegex.Matches(fileContent);

			foreach (Match pragmaEntry in metadata)
			{
				string metadataWithRemovedNewlines = pragmaEntry.Groups[1].Value.Replace("\r\n", "");
				rawMetadata.metadataEntries.Add(metadataWithRemovedNewlines);
			}

			foreach (Match includeEntry in includes)
			{
				rawMetadata.includes.Add(includeEntry.Groups[1].Value);
			}

            return rawMetadata;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Parse metadata entries in file specified and return associated YAML
        /// stream or empty stream if no metadata is found.
        /// </summary>
		/// <exception cref="System.ArgumentNullException">Throws when invalid argument (i.e. null) passed as metadata entries.</exception>
		/// <exception cref="System.OverflowException">Too many metadata entries.</exception>
		/// <exception cref="System.YamlException">Throws when input metadata entries cannot be parsed due to syntax errors.</exception>
		/// 
        private static YamlStream ParseRawMetadataEntries(IEnumerable<string> entries)
        {
            YamlStream metadataStream = new YamlStream();

			foreach (var entry in entries)
			{
				using (StringReader input = new StringReader(entry))
				{
					if (metadataStream.Documents.Count() == 0)
					{
						metadataStream.Load(input);
					}
					else
					{
						//
						// Metadata entries are represented as sub-documents in stream.
						//
						YamlStream tempStream = new YamlStream();
						tempStream.Load(input);
						metadataStream.Add(tempStream.Documents[0]);
					}
				}
			}

            return metadataStream;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves metadata from given file, parse it and optionally attach dependency
        /// information to the metadata (is extracted from includes).
        /// "dependencies" key is reserved at top-level metadata entry.
        /// </summary>
		/// <exception cref="System.ArgumentException">Throws when target file is incorrect or non-readable.</exception>
		/// <exception cref="System.ObjectDisposedException">Trying to read the file which is disposed.</exception>
		/// <exception cref="System.UnauthorizedAccessException">Specified file cannot be read.</exception>
		/// <exception cref="System.IOException">Specified file cannot be read.</exception>
		/// <exception cref="System.NotSupportedException">Operation is not supported.</exception>
		/// <exception cref="System.OutOfMemoryException">Specified file is too large and cannot be read.</exception>
		/// <exception cref="System.OverflowException">Too many metadata entries.</exception>
		/// <exception cref="System.YamlException">Throws when input metadata entries cannot be parsed due to syntax errors.</exception>
		/// 
        public static YamlStream GetMetadata(string path)
        {
            RawMetadata rawMetadata = GetRawMetadataEntries(path);
            YamlStream metadata = ParseRawMetadataEntries(rawMetadata.metadataEntries);

            //
            // Imports are always linked to module, if the file does not contain any
            // module-related info, then it cannot be a module, so, no need to extract imports,
            // even when they are exist in the file.
            //
            if (metadata.Documents.Count > 0)
            {
                YamlSequenceNode imports = new YamlSequenceNode();

                foreach (string importModule in rawMetadata.includes)
                {
                    imports.Add(new YamlScalarNode(importModule));
                }

                YamlMappingNode metadataRoot = (YamlMappingNode)metadata.Documents[0].RootNode;
                metadataRoot.Children.Add(new YamlScalarNode("dependencies"), imports);
            }

			return metadata;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Query inner node with the specified key in given metadata entry.
        /// <exception cref="System.ArgumentException">Thrown when specified metadata node is null</exception>
        /// </summary>
        public static YamlNode QueryMetadata(YamlMappingNode metadataNode, string key)
        {
            YamlNode resultNode = null;
            metadataNode.Children.TryGetValue(new YamlScalarNode(key), out resultNode);
            return resultNode;
        }
    }
}
