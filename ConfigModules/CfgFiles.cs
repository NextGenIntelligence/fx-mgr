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
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace Prosoft.FXMGR.ConfigModules
{
    using FxInterface = KeyValuePair<string, string>;

    public class CfgFiles
    {
        private readonly IEnumerable<KeyValuePair<string, YamlStream>> metadata;

        public CfgFiles(IEnumerable<KeyValuePair<string, YamlStream>> metadata)
        {
			if (metadata == null)
				throw new System.ArgumentNullException();

            this.metadata = metadata;
        }

		/// <summary>
		/// Getting list of files associated with the modules. 
		/// </summary>
		/// <exception cref="System.FormatException">Throws when interface describing metadata is in wrong format.</exception>
		/// 
        public IEnumerable<KeyValuePair<string, string>> GetAssociatedFiles(IEnumerable<FxInterface> includedModules)
        {
            HashSet<FxInterface> requiredModules = new HashSet<FxInterface>(includedModules);
            List<KeyValuePair<string, string>> files = new List<KeyValuePair<string, string>>();

            foreach (KeyValuePair<string, YamlStream> metadataItem in metadata)
            {
                FxInterface moduleName = DependencyManager.GetAssociatedInterface(metadataItem);

                if (requiredModules.Contains(moduleName))
                {
                    files.Add(new KeyValuePair<string, string>(metadataItem.Key, moduleName.Key));
                }
            }

            return files;
        }
    }
}
