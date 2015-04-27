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
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Prosoft.FXMGR.ConfigFramework
{
    using System.Text;
    using FxInterface = KeyValuePair<string, string>;

    public class ModelController : IModelController
    {
        //--------------------------------------------------------------------------------

        private abstract class AbstractNode : IModelItem
        {
            public string name { get; private set; }
            public string description { get; private set; }

            public AbstractNode(string name, string description)
            {
				if (name == null) throw new ArgumentNullException();
                this.name = name;
                this.description = description;
            }

            public object Tag { get; set; }
        }

        //--------------------------------------------------------------------------------

        private abstract class ComponentNode : AbstractNode, IModule
        {
            public FxInterface intrface { get; private set; }
            public int references { get; set; }

            public ComponentNode(string name, string description, FxInterface intrface)
                : base(name, description)
            {
				if (intrface.Key == null) throw new ArgumentNullException();
                this.intrface = intrface;
            }

            public bool selected
            {
                get
                {
                    return references != 0;
                }
            }

            public abstract bool enabled { get; }
        }

        //--------------------------------------------------------------------------------

        private class InterfaceNode : ComponentNode, IModelItemContainer
        {
            public bool included;
            public IList<ImplementationNode> childNodes = new List<ImplementationNode>();
            public ImplementationNode selectedImplementation;

            public InterfaceNode(string name, string description, FxInterface intrface)
                : base(name, description, intrface)
            {
				if (intrface.Value != null) throw new ArgumentException();
            }

            public override bool enabled
            {
                get
                {
                    return (included && references == 1) || (references == 0 && selectedImplementation == null);
                }
            }

            public IEnumerable<IModelItem> children
            {
                get
                {
                    return childNodes;
                }
            }
        }

        //--------------------------------------------------------------------------------

        private class ImplementationNode : ComponentNode
        {
            public InterfaceNode parent { get; private set; }

            public ImplementationNode(string name, string description, FxInterface intrface, InterfaceNode parent)
                : base(name, description, intrface)
            {
				if (intrface.Value == null) throw new ArgumentNullException();
                this.parent = parent;
            }

            public override bool enabled
            {
                get
                {
                    return (parent.references > 0);
                }
            }
        }

        //--------------------------------------------------------------------------------

        private class CategoryNode : AbstractNode, IModelItemContainer
        {
            public List<InterfaceNode> childNodes = new List<InterfaceNode>();

            public CategoryNode(string name, string description)
                : base(name, description)
            {

            }

            public IEnumerable<IModelItem> children
            {
                get
                {
                    return childNodes;
                }
            }
        }

        //--------------------------------------------------------------------------------

        private readonly DependencyManager dependencyManager;
        private readonly IEnumerable<KeyValuePair<string, YamlStream>> metadata;
        private readonly InterfaceTranslator interfaceMap;

        private HashSet<string> markedModules;
        private ICollection<CategoryNode> model;
        private IDictionary<FxInterface, ComponentNode> interfaceToModelItem;

        public ModelController(IEnumerable<KeyValuePair<string, YamlStream>> metadata, DependencyManager dm, InterfaceTranslator im)
        {
			if(metadata == null || dm == null || im == null)
			{
				throw new ArgumentNullException();
			}

            this.metadata = metadata;
            this.dependencyManager = dm;
            this.interfaceMap = im;

            BuildModel();
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Recursively builds component model from categories, to modules and their implementations.
        /// </summary>
        private void BuildModel()
        {
            this.interfaceToModelItem = new Dictionary<FxInterface, ComponentNode>();
            this.model = new List<CategoryNode>();
            this.markedModules = new HashSet<string>();

            //
            // Multiple categories is not supported now.
            //
            var root = new CategoryNode("Uncategorized", "");
            model.Add(root);

            BuildModules(root);
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Adds modules to given category node.
        /// </summary>
        private void BuildModules(CategoryNode root)
        {
            foreach (string abstractInterfaceName in dependencyManager.GetAbstractInterfaces())
            {
                InterfaceNode treeItem = new InterfaceNode(abstractInterfaceName, "", new FxInterface(abstractInterfaceName, null));
                root.childNodes.Add(treeItem);

                interfaceToModelItem[treeItem.intrface] = treeItem;

                BuildImplementations(treeItem, abstractInterfaceName);
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Adds implementations to given module node.
        /// </summary>
        private void BuildImplementations(InterfaceNode root, string abstractInterface)
        {
            foreach (string implementationName in dependencyManager.GetAvailImplementations(abstractInterface))
            {
                ImplementationNode treeItem = new ImplementationNode(implementationName, "", new FxInterface(abstractInterface, implementationName), root);
                root.childNodes.Add(treeItem);

                interfaceToModelItem[treeItem.intrface] = treeItem;
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Recursively increment reference counter for specified modules and all their dependencies.
        /// Note: Be sure that dependency graph does not contain cycles.
        /// </summary>
		/// <exception cref="System.ArgumentException">Unknown imports during reference processing.</exception>
		/// <exception cref="System.InvalidCastException">Model inconsistemcy has been detected.</exception>
		/// 
        private List<ComponentNode> ReferenceModule(string targetModule, int delta)
        {
            var nextTargets = new List<string>();
            nextTargets.Add(targetModule);
            List<string> tempTargets = new List<string>();
            List<ComponentNode> affectedModelItems = new List<ComponentNode>();

            do
            {
                tempTargets.Clear();
                tempTargets.AddRange(nextTargets);
                nextTargets.Clear();

                foreach (string target in tempTargets)
                {
                    FxInterface implementation = this.interfaceMap.TranslateAbstractInterface(target);
					
					ComponentNode modelItem = interfaceToModelItem[new FxInterface(target, null)];
					InterfaceNode interfaceModelItem = (InterfaceNode)modelItem;
					interfaceModelItem.selectedImplementation = (ImplementationNode)interfaceToModelItem[implementation];

					modelItem.references += delta;
                    interfaceModelItem.selectedImplementation.references += delta;
                    IEnumerable<string> dependencies = dependencyManager.GetInterfaceDependencies(implementation);

                    nextTargets.AddRange(dependencies);

                    affectedModelItems.Add(interfaceModelItem);
                    affectedModelItems.Add(interfaceModelItem.selectedImplementation);
                }
            }
            while (nextTargets.Count > 0);

            return affectedModelItems;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Recursively decrement reference counter for specified modules and all their dependencies.
        /// </summary>
		/// <exception cref="System.ArgumentException">Unknown imports during dereference processing.</exception>
		/// <exception cref="System.InvalidCastException">Model inconsistemcy has been detected.</exception>
		/// <exception cref="System.InvalidOperationException">Model inconsistemcy has been detected.</exception>
		/// 
        private List<ComponentNode> DereferenceModule(string targetModule, int delta)
        {
            List<string> nextTargets = new List<string>();
            nextTargets.Add(targetModule);
            List<string> tempTargets = new List<string>();
            List<ComponentNode> affectedNodes = new List<ComponentNode>();

            do
            {
                tempTargets.Clear();
                tempTargets.AddRange(nextTargets);
                nextTargets.Clear();

                foreach (string target in tempTargets)
                {
					FxInterface implementation = this.interfaceMap.TranslateAbstractInterface(target);

                    ComponentNode modelItem = interfaceToModelItem[new FxInterface(target, null)];
					InterfaceNode interfaceModelItem = (InterfaceNode)modelItem;

                    modelItem.references -= delta;

                    if (modelItem.references < 0)
                    {
                        throw new InvalidOperationException(
                            "Model inconsistency has been detected on dereferencing " + modelItem.name);
                    }
                    
                    affectedNodes.Add(interfaceModelItem.selectedImplementation);
                    affectedNodes.Add(interfaceModelItem);

                    interfaceModelItem.selectedImplementation.references -= delta;

					if (interfaceModelItem.selectedImplementation.references < 0)
					{
                        throw new InvalidOperationException(
                            "Model inconsistency has been detected on dereferencing " + modelItem.name);
					}

                    if (modelItem.references == 0)
                    {
                        this.interfaceMap.RemoveInterfaceFromMapping(modelItem.intrface.Key);
                        interfaceModelItem.selectedImplementation = null;
                    }

                    IEnumerable<string> deps = dependencyManager.GetInterfaceDependencies(implementation);
                    nextTargets.AddRange(deps);
                }
            }
            while(nextTargets.Count > 0);

            return affectedNodes;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Checks dependency graph for cycles.
        /// </summary>
		/// <exception cref="System.ArgumentException">Unknown imports during reference processing.</exception>
		/// <exception cref="System.InvalidOperationException">Dependency graph is not acyclic.</exception>
		/// 
        private void CheckForCycles(FxInterface target, Stack<FxInterface> importStack)
        {
            ICollection<FxInterface> implementations = new List<FxInterface>();
            IEnumerable<string> dependencies = dependencyManager.GetInterfaceDependencies(target);

            foreach (string abstractInterface in dependencies)
            {
                implementations.Add(this.interfaceMap.TranslateAbstractInterface(abstractInterface));
            }

            foreach (FxInterface implementation in implementations)
            {
                if (importStack.Contains(implementation)) 
                {
                    StringBuilder sb = new StringBuilder("Cyclic dependency is detected: ");

					foreach (var module in importStack)
					{
						sb.Append(module.Key + ":" + module.Value + "; ");
					}

					throw new InvalidOperationException(sb.ToString());
                }

                importStack.Push(implementation);
                CheckForCycles(implementation, importStack);
                importStack.Pop();
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Include module associated with specified model item.
        /// </summary>
		/// <exception cref="System.ArgumentException">Unknown imports during reference processing.</exception>
		/// <exception cref="System.InvalidCastException">Model inconsistency has been detected.</exception>
		/// <exception cref="System.InvalidOperationException">Dependency graph contains cycles.</exception>
		/// 
        public IEnumerable<IModelItem> IncludeModule(IModelItem modelItem)
        {
			if (modelItem == null)
				throw new ArgumentException();

            InterfaceNode interfaceModelItem = (InterfaceNode)modelItem;
            FxInterface target = interfaceModelItem.intrface;
			CheckForCycles(this.interfaceMap.TranslateAbstractInterface(target.Key), new Stack<FxInterface>());
			IEnumerable<ComponentNode> affectedNodes = ReferenceModule(target.Key, 1);
			markedModules.Add(target.Key);
			interfaceModelItem.included = true;
            return affectedNodes;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Exclude module associated with specified model item.
        /// Note: Use IsExcludeAllowed before using of this function.
        /// </summary>
		/// <exception cref="System.ArgumentException">Unknown imports during dereference processing.</exception>
		/// <exception cref="System.InvalidCastException">Model inconsistemcy has been detected.</exception>
		/// <exception cref="System.InvalidOperationException">Model inconsistency has been detected.</exception>
		/// 
        public IEnumerable<IModelItem> ExcludeModule(IModelItem modelItem)
        {
			if (modelItem == null)
				throw new ArgumentException();

            InterfaceNode interfaceModelItem = (InterfaceNode)modelItem;
			IEnumerable<ComponentNode> affectedNodes = DereferenceModule(interfaceModelItem.intrface.Key, 1);
			markedModules.Remove(interfaceModelItem.intrface.Key);
			interfaceModelItem.included = false;
            return affectedNodes;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Change implementation for specified interface (dereference old implementation and then
        /// reference new one.
        /// </summary>
		/// <exception cref="System.ArgumentException">Unknown imports during reference processing.</exception>
		/// <exception cref="System.InvalidCastException">Model inconsistemcy has been detected.</exception>
		/// <exception cref="System.InvalidOperationException">Model inconsistemcy has been detected.</exception>
		/// 
        public IEnumerable<IModelItem> ChangeImplementation(IModelItem interfaceModelItem, IModelItem newImplModelItem)
        {
			if (interfaceModelItem == null || newImplModelItem == null)
				throw new ArgumentException();

            InterfaceNode parent = (InterfaceNode)interfaceModelItem;
            ImplementationNode from = parent.selectedImplementation;
            ImplementationNode to = (ImplementationNode)newImplModelItem;
            FxInterface prev = from.intrface;
            FxInterface next = to.intrface;
            int refs = parent.references;

            List<ComponentNode> affectedByDeref = DereferenceModule(prev.Key, refs);
            this.interfaceMap.ChangeMapping(prev.Key, next.Value);
            parent.selectedImplementation = to;
            CheckForCycles(this.interfaceMap.TranslateAbstractInterface(prev.Key), new Stack<FxInterface>());
            List<ComponentNode> affectedByRef = ReferenceModule(next.Key, refs);
            affectedByDeref.AddRange(affectedByRef);

            return new HashSet<ComponentNode>(affectedByDeref);
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Used for checking whether excluding of the module is allowed. If so, HandleExclude may be called for that module.
        /// Note: This functionality is is taken out from exclude function in order to allow GUI application to set
        /// availability of GUI elements for user.
        /// </summary>
		/// <exception cref="System.InvalidCastException">Model inconsistemcy has been detected.</exception>
		/// <exception cref="System.InvalidOperationException">Model inconsistemcy has been detected.</exception>
		/// 
        public bool IsExcludeAllowed(IModelItem interfaceModelItem)
        {
            InterfaceNode interfaceNode = (InterfaceNode)interfaceModelItem;
            ImplementationNode selectedChild = interfaceNode.selectedImplementation;

            if (!interfaceNode.included)
                throw new InvalidOperationException();

            if (interfaceNode.references > 1)
            {
                return false;
            }

            return true;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return component model.
        /// </summary>        
        public IEnumerable<IModelItem> Model
        {
            get
            {
                return model;
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return all modules included, regardless of user-selected or included by dependency.
        /// </summary> 
        public IEnumerable<KeyValuePair<string, string>> IncludedModules
        {
            get
            {
                //
                // Select all modules corresponding to implementations where reference counter is non-null.
                //
                return interfaceToModelItem.Values.Where(x => x.selected && x.intrface.Value != null).Select(x => x.intrface);
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return all modules explicitly selected by user.
        /// </summary> 
        public IEnumerable<string> MarkedModules
        {
            get
            {
                return markedModules;
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Return model item corresponding to specified module (abstract module by default).
        /// </summary> 
        public IModelItem GetModelItemByName(string module, string impl = null)
        {
			ComponentNode modelNode = null;
            FxInterface fullName = new FxInterface(module, impl);
			interfaceToModelItem.TryGetValue(fullName, out modelNode);
			return modelNode;
        }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Reset model and all reference counters. It should be used in case of exceptions in order to 
		/// prevent the system from inconsistent state (i.e. exception within recursive module reference process).
		/// </summary>
		private IEnumerable<IModelItem> Reset()
		{
			List<IModelItem> affectedNodes = new List<IModelItem>();

			markedModules.Clear();

			foreach(CategoryNode cn in model)
			{
				affectedNodes.AddRange(cn.children);

				foreach(InterfaceNode inode in cn.children)
				{
					inode.selectedImplementation = null;
					inode.references = 0;
					inode.included = false;

					affectedNodes.AddRange(inode.children);

					foreach(ImplementationNode impl in inode.children)
					{
						impl.references = 0;
					}
				}
			}

			return affectedNodes;
		}
    }
}
