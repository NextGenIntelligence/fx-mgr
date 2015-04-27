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

namespace Prosoft.FXMGR.ConfigFramework
{
    //
    // Model controller is used to include/exclude modules or change implementations for
    // already included modules.
    //
    public interface IModelController
    {
        bool IsExcludeAllowed(IModelItem node);
        IEnumerable<IModelItem> ExcludeModule(IModelItem node);
        IEnumerable<IModelItem> IncludeModule(IModelItem node);
        IEnumerable<IModelItem> ChangeImplementation(IModelItem parent, IModelItem to);

        IModelItem GetModelItemByName(string module, string impl = null);

        IEnumerable<IModelItem> Model { get; }
        IEnumerable<string> MarkedModules { get; }
        IEnumerable<KeyValuePair<string, string>> IncludedModules { get; }
    }

    //
    // Abstract item of the model. It does not neccessarily correspond to some module (i.e. it may be a category).
    //
    public interface IModelItem
    {
        string name { get; }
        string description { get; }
        object Tag { get; set; }
    }

    //
    // Model item container is used to represent "expandable" noeds in hierrarchy: for example categories
    // have set of modules, modules have set of possible interfaces and so on.
    //
    public interface IModelItemContainer : IModelItem
    {
        IEnumerable<IModelItem> children { get; }
    }

    //
    // Module interface. Module may be included due to either user selection or references from another modules.
    //
    public interface IModule : IModelItem
    {
        bool selected { get; }
        bool enabled { get; }
    }
}
