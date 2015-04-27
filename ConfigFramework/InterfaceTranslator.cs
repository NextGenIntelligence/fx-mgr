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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Prosoft.FXMGR.ConfigFramework
{
	using FxInterface = KeyValuePair<string, string>;

    public class InterfaceTranslator
    {
        private IDictionary<string, string> currentMapping;
        private IDictionary<string, string> templateMapping;

        private readonly DependencyManager dependencyManager;

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Create empty interface translator.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">Throws when null-ref metadata is specified.</exception>
		///
        public InterfaceTranslator(DependencyManager dm)
        {
			if (dm == null)
				throw new ArgumentNullException();

            this.dependencyManager = dm;
            this.currentMapping = new Dictionary<string, string>();
            this.templateMapping = new Dictionary<string, string>();
        }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Create interface translator with preset template mapping.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">Throws when null-ref metadata is specified.</exception>
		///
        public InterfaceTranslator(DependencyManager dm, IDictionary<string, string> savedMapping)
        {
			if (dm == null || savedMapping == null)
				throw new ArgumentNullException();

            this.dependencyManager = dm;
            this.currentMapping = new Dictionary<string, string>(savedMapping);
            this.templateMapping = new Dictionary<string, string>();
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Adds mapping containing in the template file into template mapping dictionary.
        /// </summary>
		/// <exception cref="System.ArgumentException">Throws when target file is incorrect or non-readable.</exception>
		/// <exception cref="System.ObjectDisposedException">Trying to read the file which is disposed.</exception>
		/// <exception cref="System.UnauthorizedAccessException">Specified file cannot be read.</exception>
		/// <exception cref="System.IOException">Specified file cannot be read.</exception>
		/// <exception cref="System.NotSupportedException">Operation is not supported.</exception>
		/// <exception cref="System.OutOfMemoryException">Specified file is too large and cannot be read.</exception>
		/// <exception cref="System.FormatException">Specified file is in wrong format.</exception>
		/// 
        public void LoadTemplate(string templateFilePath)
        {
            using (TextReader mappingReader = File.OpenText(templateFilePath))
            {
                string mapping;
                Regex mappingRegex = new Regex("(\\w+)\\s*=\\s*(\\w+)");

                while ((mapping = mappingReader.ReadLine()) != null)
                {
                    Match map = mappingRegex.Match(mapping);

					try
					{
						if (map.Success)
						{
							templateMapping[map.Groups[1].Value] = map.Groups[2].Value;
						}
					}
					catch(IndexOutOfRangeException e)
					{
                        throw new FormatException(
                            "Invalid template format in file: " + templateFilePath, e.InnerException);
					}
                }
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Translates interface name into implementation and adds translation into current mapping.
        /// </summary>
		/// <exception cref="System.ArgumentException">Unknown interface is used.</exception>
		/// 
        public FxInterface TranslateAbstractInterface(string interfaceName)
        {
            string implName = null;

            if (!currentMapping.TryGetValue(interfaceName, out implName))
            {
                if (!templateMapping.TryGetValue(interfaceName, out implName))
                {
					IEnumerable<string> impls = dependencyManager.GetAvailImplementations(interfaceName);
					implName = impls.First();

					if (impls.Count() > 1)
					{
						Logger.Log(
                            String.Format("Warning! Template has no implementation for interface {0}. Implementation {1} is used.",
							interfaceName, implName));
					}
                }

                currentMapping[interfaceName] = implName;
            }

            return new FxInterface(interfaceName, implName);
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Removes interface from current mapping.
        /// </summary>
        public void RemoveInterfaceFromMapping(string i)
        {
            currentMapping.Remove(i);
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Updates current mapping and set new implementation for given abstract module.
        /// </summary>
        public void ChangeMapping(string i, string impl)
        {
            currentMapping[i] = impl;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return current mapping.
        /// </summary>
        public IDictionary<string, string> Mapping
        {
            get
            {
                return currentMapping;
            }
        }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Clear current mapping.
		/// </summary>
		public void Reset()
		{
			currentMapping.Clear();
		}
    }
}
