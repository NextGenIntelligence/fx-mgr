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
using System.Linq;

namespace Prosoft.FXMGR.ConfigModules
{
    public class CfgGraphViz
    {
        private readonly DependencyManager dependencyManager;
        private readonly InterfaceTranslator translator;

        public CfgGraphViz(DependencyManager dm, InterfaceTranslator translator)
        {
			if (dm == null || translator == null)
				throw new System.ArgumentNullException();

            this.dependencyManager = dm;
            this.translator = translator;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return sequence of dependency graph connections in GraphViz format (a -> b;).
        /// First variable is enumeration of all modules, included in build, second - modules
        /// which should be reflected in output graph.
        /// </summary>
		/// <exception cref="System.ArgumentException">Throws when unknown interface is used.</exception>
		/// 
        public string GetDOT(IEnumerable<string> modules, IEnumerable<string> modulesToBeShown)
        {
            string dot = "";
            Dictionary<string, HashSet<string>> dependencyGraph = new Dictionary<string, HashSet<string>>();
            HashSet<string> graphModules = new HashSet<string>(modulesToBeShown);

            //
            // Clone dependency graph from dependency manager and resolve interface->implementation mapping.
            // Each key in dictionary maps to set of dependencies.
            //
            foreach (var module in modules)
            {
                var implementation = translator.TranslateAbstractInterface(module);
                dependencyGraph[module] = new HashSet<string>(dependencyManager.GetInterfaceDependencies(implementation));
            }

            //
            // Next cycle is the heart of the algorithm.
            // Now replace every module in dependency graph with their dependencies (except modules which should
            // be shown in the resulting graph).
            // dict(a) = (x, y) = (x, dependencies(y)), where x - modules should be shown, y - other modules.
            // In every iteration, after replacing, modules y with no dependencies are removed from graph.
            // As a result, we got dependency graph containing only dependencies should be shown in the resulting graph.
            //
            bool graphProcess;
            List<string> temp = new List<string>();
            HashSet<string> itemsToReplace = new HashSet<string>();
            do
            {
                graphProcess = false;

                foreach (var key in dependencyGraph.Keys)
                {
                    temp.Clear();
                    itemsToReplace.Clear();

                    HashSet<string> moduleDependencies = dependencyGraph[key];

                    //
                    // Accumulate modules which should be replaced with their dependencies in itemsToReplace.
                    //
                    foreach(var module in moduleDependencies.Except(graphModules))
                    {
                        itemsToReplace.Add(module);
                    }
                    
                    //
                    // Replace all modules from itemsToReplace with dependencies and store them into temp.
                    //
                    foreach (var module in itemsToReplace)
                    {
                        temp.AddRange(dependencyGraph[module]);
                    }

                    //
                    // Update dependency graph: remove items gathered at first stage.
                    //
                    moduleDependencies.RemoveWhere(x => itemsToReplace.Contains(x));

                    //
                    // Update dependency graph: add dependencies instead of removed items.
                    //
                    foreach (var module in temp)
                    {
                        moduleDependencies.Add(module);
                    }
                }

                //
                // Filter dependency graph: remove all items which should not be shown at resulting graph and
                // have no dependencies.
                //
                foreach (var key in dependencyGraph.Keys)
                {
                    graphProcess = graphProcess | 
                        dependencyGraph[key].RemoveWhere(x => dependencyGraph[x].Count == 0 && !graphModules.Contains(x)) > 0;
                }
            }
            while (graphProcess);

            //
            // Remove modules not shown on the graph.
            //
            foreach (var key in dependencyGraph.Keys.ToList())
            {
                if (!graphModules.Contains(key))
                {
                    dependencyGraph.Remove(key);
                }
            }

            //
            // Generate output.
            //
            foreach (var module in dependencyGraph.Keys)
            {
                foreach (var dependency in dependencyGraph[module])
                {
                    dot += module + " -> " + dependency + ";\r\n";
                }
            }

            return dot;
        }
    }
}
