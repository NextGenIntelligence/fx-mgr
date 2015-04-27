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
using Prosoft.FXMGR.ConfigModules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Prosoft.FXMGR.GuiTool
{
    using FxInterface = KeyValuePair<string, string>;
    
    public enum ControllerState
    {
        Empty,
        ProjectInfoSet,
        Ready
    }

    [Serializable]
    public class ProjectInfo
    {
        public bool templateProject { get; set; }
        public string templateFilePath { get; set; }
        public string projectName { get; set; }
        public string projectPath { get; set; }
        public IList<string> sourceFolders { get; set; }
    }

    public class OutputSettings
    {
        public bool flatListGeneration { get; set; }
        public string flatListPath { get; set; }

        public bool mapHeaderGeneration { get; set; }
        public string mapHeaderPath { get; set; }
        public string mapHeaderPrefix { get; set; }

        public bool gvGeneration { get; set; }
        public string gvPath { get; set; }
        public string[] gvContent { get; set; }

        public bool initGeneration { get; set; }
        public string initPath { get; set; }

        public bool optionsGeneration { get; set; }
        public string optionsPath { get; set; }

        public bool mappingGeneration { get; set; }
        public string mappingPath { get; set; }
    }

    public class ProjectStateChangedEventArgs : EventArgs
    {
        public IEnumerable<IModelItem> model { get; private set; }
        public IEnumerable<BaseOption> options { get; private set; }
        public ControllerState state { get; private set; }

        public ProjectStateChangedEventArgs(ControllerState state, IEnumerable<IModelItem> model = null, IEnumerable<BaseOption> options = null)
        {
            this.state = state;
            this.model = model;
            this.options = options;
        }
    }

    public delegate void ProjectStateChangedEventHandler(object sender, ProjectStateChangedEventArgs e);

    public class ConfigurationChangedEventArgs : EventArgs
    {
        public IEnumerable<IModelItem> affectedNodes { get; private set; }
        public IEnumerable<BaseOption> options { get; private set; }

        public ConfigurationChangedEventArgs(IEnumerable<IModelItem> affectedNodes, IEnumerable<BaseOption> options = null)
        {
            this.affectedNodes = affectedNodes;
            this.options = options;
        }
    }

    public delegate void ConfigurationChangedEventHandler(object sender, ConfigurationChangedEventArgs e);

    public class StateController
    {
        public event ConfigurationChangedEventHandler ConfigurationChanged;
        public event ProjectStateChangedEventHandler ProjectStateChanged;

        public event BeforeAnalyzingHandler BeforeAnalyzing
        {
            add
            {
                this.metadataStorage.BeforeAnalyzing += value;
            }
            remove
            {
                this.metadataStorage.BeforeAnalyzing -= value;
            }
        }

        public event ItemAnalyzedHandler ItemAnalyzed
        {
            add
            {
                this.metadataStorage.ItemAnalyzed += value;
            }
            remove
            {
                this.metadataStorage.ItemAnalyzed -= value;
            }
        }

        public event AfterAnalyzingHandler AfterAnalyzing;

        private ControllerState state = ControllerState.Empty;

        private string optionsFilePath = null;

        private IModelController modelController;
        private MetadataStorage metadataStorage;
        private InterfaceTranslator interfaceTranslator;
        private DependencyManager dependencyManager;
        private ProjectInfo info;
        private CfgOptions optionsManager;

        private bool CheckOptionsPresence(MetadataStorage st)
        {
            bool present = false;

            foreach (var m in metadataStorage.Metadata)
            {
                FxInterface i = DependencyManager.GetAssociatedInterface(m);
                if (i.Key == "CFG_OPTIONS")
                {
                    present = true;
                    break;
                }
            }

            if (!present)
            {
                optionsFilePath = Path.Combine(info.projectPath, info.projectName + "_options.h");
                st.AddGeneratedItem(optionsFilePath, "CFG_OPTIONS", "GENERATED");
            }

            return present;
        }

        public string GetProjectPath() { return projectInfo.projectPath; }
        public string GetProjectName() { return projectInfo.projectName; }
        public bool IsOptionsPresent() { return this.optionsFilePath == null; }

        public void AnalyzeSources(Stream f = null)
        {
            bool success = metadataStorage.AnalyzeSources();

            if (success)
            {
                var metadata = metadataStorage.Metadata;
                CheckOptionsPresence(metadataStorage);
                dependencyManager = new DependencyManager(metadata);
                optionsManager = new CfgOptions(metadata);

                if (f == null)
                {
                    interfaceTranslator = new InterfaceTranslator(dependencyManager);
                    interfaceTranslator.LoadTemplate(info.templateFilePath);
                    modelController = new ModelController(metadata, dependencyManager, interfaceTranslator);
                }
                else
                {
                    BinaryFormatter b = new BinaryFormatter();

                    IDictionary<string, string> savedMapping = (IDictionary<string, string>)b.Deserialize(f);
                    IEnumerable<string> savedCheckedItems = (IEnumerable<string>)b.Deserialize(f);
                    IEnumerable<SavedOption> savedOptions = (IEnumerable<SavedOption>)b.Deserialize(f);

                    interfaceTranslator = new InterfaceTranslator(dependencyManager, savedMapping);
                    interfaceTranslator.LoadTemplate(info.templateFilePath);

                    modelController = new ModelController(metadata, dependencyManager, interfaceTranslator);

                    foreach (string s in savedCheckedItems)
                    {
                        IModelItem tnode = modelController.GetModelItemByName(s);
                        modelController.IncludeModule(tnode);
                    }

                    var ints = modelController.IncludedModules;
                    optionsManager.UpdateOptions(ints);
                    optionsManager.LoadSavedOptions(savedOptions);
                }

                ChangeProjectState(ControllerState.Ready);
            }

            if (AfterAnalyzing != null)
            {
                AfterAnalyzingEventArgs e = new AfterAnalyzingEventArgs(success);
                AfterAnalyzing(this, e);
            }
        }

        public ProjectInfo projectInfo
        {
            get
            {
                return info;
            }
            set
            {
                info = value;
                metadataStorage = new MetadataStorage(info.sourceFolders, new string[] { "*.h", "*.c", "*.s*" });
                ChangeProjectState(ControllerState.ProjectInfoSet);
            }
        }

        private void ChangeProjectState(ControllerState state)
        {
            this.state = state;

            if (ProjectStateChanged != null)
            {
                ProjectStateChangedEventArgs args = (state == ControllerState.Ready) ?
                    new ProjectStateChangedEventArgs(state, this.modelController.Model, this.GetOptions()) :
                    new ProjectStateChangedEventArgs(state);

                ProjectStateChanged(this, args);
            }
        }

        public IEnumerable<string> GetIncluded()
        {
            return modelController.IncludedModules.Select(x => x.Key);
        }

        public void HandleInclude(IModelItem modelItem)
        {
            var affectedNodes = modelController.IncludeModule(modelItem);

            if (ConfigurationChanged != null)
            {
                var sourceChangedEventArgs = new ConfigurationChangedEventArgs(affectedNodes, this.GetOptions());
                ConfigurationChanged(this, sourceChangedEventArgs);
            }
        }

        public void HandleExclude(IModelItem modelItem)
        {
            var affectedNodes = modelController.ExcludeModule(modelItem);

            if (ConfigurationChanged != null)
            {
                var sourceChangedEventArgs = new ConfigurationChangedEventArgs(affectedNodes, this.GetOptions());
                ConfigurationChanged(this, sourceChangedEventArgs);
            }
        }

        public void ChangeImplementation(IModelItem interfaceModelItem, IModelItem newImplModelItem)
        {
            var affectedNodes = modelController.ChangeImplementation(interfaceModelItem, newImplModelItem);

            if (ConfigurationChanged != null)
            {
                var sourceChangedEventArgs = new ConfigurationChangedEventArgs(affectedNodes, this.GetOptions());
                ConfigurationChanged(this, sourceChangedEventArgs);
            }
        }

        public bool IsUncheckAllowed(IModelItem interfaceModelItem)
        {
            return modelController.IsExcludeAllowed(interfaceModelItem);
        }

        private IEnumerable<BaseOption> GetOptions()
        {
            var x = modelController.IncludedModules;
            optionsManager.UpdateOptions(x);
            return optionsManager.Options;
        }

        public void Save(Stream f)
        {
            BinaryFormatter b = new BinaryFormatter();

            b.Serialize(f, state);
            b.Serialize(f, info);

            if (state == ControllerState.Ready)
            {
                IDictionary<string, string> mapping = interfaceTranslator.Mapping;
                IEnumerable<string> selected = modelController.MarkedModules;
                IEnumerable<SavedOption> optionsToSave = optionsManager.GetOptionsToSave();
                b.Serialize(f, mapping);
                b.Serialize(f, selected);
                b.Serialize(f, optionsToSave);
            }
        }

        public bool Load(Stream f)
        {
            BinaryFormatter b = new BinaryFormatter();

            ControllerState savedState = (ControllerState)b.Deserialize(f);
            ProjectInfo savedInfo = (ProjectInfo)b.Deserialize(f);

            projectInfo = savedInfo;

            return savedState == ControllerState.Ready;
        }

        public void GenerateOutput(OutputSettings outputs)
        {
            var metadata = metadataStorage.Metadata;
            CfgFiles fileManager = new CfgFiles(metadata);
            IEnumerable<FxInterface> includedModules = modelController.IncludedModules;
            IEnumerable<KeyValuePair<string, string>> files = fileManager.GetAssociatedFiles(includedModules);

            if (outputs.flatListGeneration)
            {
                using (StreamWriter flatFileWriter = File.CreateText(outputs.flatListPath))
                {
                    foreach (var f in files)
                    {
                        if (Path.GetExtension(f.Key) != ".h")
                        {
                            flatFileWriter.WriteLine(f.Key);
                        }
                    }

                    if (outputs.initGeneration)
                    {
                        flatFileWriter.WriteLine(outputs.initPath);
                    }
                }
            }

            if (outputs.mapHeaderGeneration)
            {
                using (StreamWriter writer = File.CreateText(outputs.mapHeaderPath))
                {
                    foreach (var f in files)
                    {
                        if (Path.GetExtension(f.Key) == ".h")
                        {
                            writer.WriteLine("#define " + f.Value + " \"" + f.Key + "\"\r\n");
                        }
                    }
                }
            }

            if (outputs.gvGeneration && outputs.gvContent != null && outputs.gvContent.Count() > 0)
            {
                CfgGraphViz graphGenerator = new CfgGraphViz(dependencyManager, interfaceTranslator);
                HashSet<string> filter = new HashSet<string>(outputs.gvContent);
                string dot = "digraph G {\r\nsplines=true;\r\nsep=\"+25,25\";\r\noverlap=scalexy;\r\nnodesep=0.6;\r\nnode [fontsize=11];\r\n";
                dot += graphGenerator.GetDOT(includedModules.Where(x => true).Select(x => x.Key), filter);
                dot += "\r\n}\r\n";
                using (StreamWriter dotFile = File.CreateText(outputs.gvPath))
                {
                    dotFile.Write(dot);
                }
            }

            if (outputs.initGeneration)
            {
				IEnumerable<string> modulesWithCtor = null;
                CfgInitSequence initSequence = new CfgInitSequence(dependencyManager, interfaceTranslator, metadata);
				IEnumerable<KeyValuePair<string, bool>> ctors = initSequence.GetInitSequence(includedModules, out modulesWithCtor);

                string ctorSequence = "";

				foreach (var m in modulesWithCtor)
                {
                    ctorSequence += "#include FX_INTERFACE(" + m + ")\r\n";
                }

                //
                // Function calls.
                //
                ctorSequence += "void fx_dj_init_once(void){\r\n";
                foreach (KeyValuePair<string, bool> kv in ctors)
                {
                    ctorSequence += kv.Key + "();\r\n";
                }

                ctorSequence += "}\r\nvoid fx_dj_init_each(void){\r\n";

                foreach (KeyValuePair<string, bool> kv in ctors)
                {
                    if (kv.Value)
                    {
                        ctorSequence += kv.Key + "();\r\n";
                    }
                }
                ctorSequence += "}\r\n";

                using (StreamWriter ctorFileWriter = File.CreateText(outputs.initPath))
                {
                    ctorFileWriter.Write(ctorSequence);
                }
            }

            if (outputs.optionsGeneration)
            {
                optionsManager.UpdateOptions(includedModules);

                if (optionsManager.Options.Count() > 0)
                {
                    using (StreamWriter optionsWriter = File.CreateText(outputs.optionsPath))
                    {
                        foreach (var option in optionsManager.Options)
                        {
                            switch (option.type)
                            {
                                case OptionType.Enumeration:
                                    var a = (EnumOption)option;
                                    optionsWriter.WriteLine("#define " + option.name + " " + a.value[a.index]);
                                    break;
                                case OptionType.Integer:
                                    var b = (IntegerOption)option;
                                    optionsWriter.WriteLine("#define " + option.name + " " + b.value);
                                    break;
                            }
                        }
                    }
                }
            }

            if (outputs.mappingGeneration)
            {
                using (StreamWriter outputWriter = File.CreateText(outputs.mappingPath))
                {
                    IDictionary<string, string> mapping = interfaceTranslator.Mapping;

                    foreach (var s in mapping.Keys)
                    {
                        outputWriter.WriteLine(s + " = " + mapping[s]);
                    }
                }
            }
        }
    }
}
