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

using CommandLine;
using Prosoft.FXMGR.ConfigFramework;
using Prosoft.FXMGR.ConfigModules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Prosoft.FXMGR.ConsoleTool
{
    using FxInterface = KeyValuePair<string, string>;

    class Program
    {
        class Options
        {
            [OptionList('p', "path", DefaultValue = null, Separator = ',', HelpText = "Set of component folders.")]
            public IList<string> Folders { get; set; }

            [Option('a', "alias", DefaultValue = null, HelpText = "Default interface to implementation mapping.")]
            public string AliasFile { get; set; }

            [Option('o', "output", DefaultValue = null, HelpText = "Output file.")]
            public string OutputFile { get; set; }

            [Option('t', "target", DefaultValue = null, HelpText = "Target module.")]
            public string Target { get; set; }

            [Option('h', "header", DefaultValue = null, HelpText = "Mapping header.")]
            public string MappingHeader { get; set; }

            [Option('c', "ctors", DefaultValue = null, HelpText = "Init sequence.")]
            public string CtorsFile { get; set; }

            [Option('s', "options", DefaultValue = null, HelpText = "Options header.")]
            public string OptionsFile { get; set; }

            [Option('e', "aspects", DefaultValue = null, HelpText = "Aspects file.")]
            public string AspectsFile { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return "Example: fx_mgr -f folder1,folder2 -a alias.txt -t MY_MODULE -o files.txt";
            }
        }

        static void Main(string[] args)
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine("FX-MGR ver." + version.ToString());

            //
            // Redirect warnings to console.
            //
            var sw = new StreamWriter(Console.OpenStandardOutput());
            sw.AutoFlush = true;
            Console.SetOut(sw);
            Logger.SetOutputStream(sw);

            string projectPath = null;

            //
            // Required params.
            //
            IEnumerable<string> sourceFolders = null;
            string outputFile = null;
            IEnumerable<string> targetModules = null;

            //
            // Optional params.
            //
            string optionsFile = null;
            string aspectsFile = null;
            string mappingFile = null;
            string templateFile = null;

            var options = new Options();

            //
            // Parse command line options.
            //
            try
            {
                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    if (options.Folders != null && options.Target != null && options.OutputFile != null)
                    {
                        projectPath = Directory.GetCurrentDirectory();
                        sourceFolders = new List<string>(options.Folders);
                        outputFile = options.OutputFile;
                        templateFile = options.AliasFile;
                        mappingFile = options.MappingHeader;
                        optionsFile = options.OptionsFile;
                        aspectsFile = options.AspectsFile;

                        targetModules = new string[] { options.Target };
                    }
                    else
                    {
                        Console.WriteLine("Incorrect input options.");
                        return;
                    }
                }
            }
            catch (ParserException)
            {
                Console.WriteLine("Command line error.");
                return;
            }
            catch (IOException)
            {
                Console.WriteLine("Can't access input file.");
                return;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Can't access input file.");
                return;
            }

            DependencyManager dependencyManager;
            MetadataStorage metadataStorage;
            InterfaceTranslator interfaceTranslator;
            IEnumerable<FxInterface> includedModules;

            //
            // Analyze metadata, construct framefork classes and include selected modules.
            //
            try
            {
                //
                // Replace possible slashes in folder paths, convert them into absolute paths. 
                //
                var temp1 = sourceFolders.Where(x => x != "").Select(x => x.Replace('/', '\\'));
                var temp2 = temp1.Select(x => Path.GetFullPath(x));
                metadataStorage = new MetadataStorage(temp2, new string[] { "*.h", "*.c", "*.s*" });

                bool metadataAnalyzingSuccess = metadataStorage.AnalyzeSources();
                if (!metadataAnalyzingSuccess) return;

                //
                // Add generated items if aspects or options generation were specified.
                //
                if (optionsFile != null)
                {
                    metadataStorage.AddGeneratedItem(Path.GetFullPath(optionsFile), "CFG_OPTIONS", "GENERATED");
                }

                if (aspectsFile != null)
                {
                    metadataStorage.AddGeneratedItem(Path.GetFullPath(aspectsFile), "CFG_ASPECTS", "GENERATED");
                }

                dependencyManager = new DependencyManager(metadataStorage.Metadata);
                interfaceTranslator = new InterfaceTranslator(dependencyManager);

                if (templateFile != null)
                {
                    interfaceTranslator.LoadTemplate(templateFile);
                }

                ModelController modelController = new ModelController(metadataStorage.Metadata, dependencyManager, interfaceTranslator);

                foreach (string m in targetModules)
                {
                    IModelItem modelNode = modelController.GetModelItemByName(m);
                    modelController.IncludeModule(modelNode);
                }

                //
                // Get collection of all included modules.
                //
                includedModules = modelController.IncludedModules;
            }
            catch (IOException)
            {
                Console.WriteLine("Some input files cannot be read.");
                return;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            //
            //---------- Generate output
            //
            try
            {
                if (options.CtorsFile != null)
                {
                    IEnumerable<string> modulesWithCtor = null;
                    CfgInitSequence initSequence = new CfgInitSequence(dependencyManager, interfaceTranslator, metadataStorage.Metadata);
                    IEnumerable<KeyValuePair<string, bool>> ctors = initSequence.GetInitSequence(includedModules, out modulesWithCtor);

                    string ctorSequence = "";

                    //
                    // Imports.
                    //
                    foreach (var m in modulesWithCtor)
                    {
                        ctorSequence += "#include FX_INTERFACE(" + m + ")\r\n";
                    }

                    //
                    // Function calls.
                    //
                    ctorSequence += "\r\nvoid fx_dj_init_once(void){\r\n";
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

                    using (StreamWriter ctorFileWriter = File.CreateText(Path.Combine(projectPath, options.CtorsFile)))
                    {
                        ctorFileWriter.Write(ctorSequence);
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Init sequence file cannot be written.");
                return;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            //
            // Generate options.
            //
            try
            {
                if (optionsFile != null)
                {
                    CfgOptions optionsManager = new CfgOptions(metadataStorage.Metadata);
                    optionsManager.UpdateOptions(includedModules);

                    if (optionsManager.Options.Count() > 0)
                    {
                        using (StreamWriter optionsWriter = File.CreateText(optionsFile))
                        {
                            foreach (var option in optionsManager.Options)
                            {
                                optionsWriter.WriteLine("/*" + option.description + "*/");

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
            }
            catch (IOException)
            {
                Console.WriteLine("Options file cannot be written.");
                return;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            //
            // Generate aspects.
            //
            try
            {
                if (aspectsFile != null)
                {
                    CfgAspects aspectsManager = new CfgAspects(metadataStorage.Metadata);
                    var aspects = aspectsManager.GetAspects(includedModules);

                    if (aspects.Count() > 0)
                    {
                        using (StreamWriter aspectsWriter = File.CreateText(aspectsFile))
                        {
                            foreach (var aspect in aspects)
                            {
                                aspectsWriter.WriteLine();
                                aspectsWriter.WriteLine("#define " + aspect.Key + " \\");
                                string joinedAspects = String.Join("\\\r\n", aspect.Value);
                                aspectsWriter.Write(joinedAspects);
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Aspects file cannot be written.");
                return;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            //
            // Generate list of files to be built.
            //
            try
            {
                CfgFiles fileManager = new CfgFiles(metadataStorage.Metadata);
                IEnumerable<KeyValuePair<string, string>> files = fileManager.GetAssociatedFiles(includedModules);

                if (mappingFile != null)
                {
                    using (StreamWriter mappingWriter = File.CreateText(mappingFile))
                    {
                        var headers = files.Where(x => Path.GetExtension(x.Key) == ".h").Select(x => x);

                        mappingWriter.WriteLine("#define FX_METADATA(x)");
                        mappingWriter.WriteLine("#define __FX_INTERFACE(i) i");
                        mappingWriter.WriteLine("#define FX_INTERFACE(i) __FX_INTERFACE(I_##i)");

                        foreach (var h in headers)
                        {
                            mappingWriter.WriteLine("#define I_" + h.Value + " \"" + h.Key + "\"");
                        }
                    }
                }

                if (!Directory.Exists(outputFile))
                {
                    using (StreamWriter outputWriter = File.CreateText(outputFile))
                    {
                        var sources = files.Where(x => Path.GetExtension(x.Key) != ".h").Select(x => x);

                        foreach (var s in sources)
                        {
                            outputWriter.WriteLine(s.Key);
                        }

                        if (options.CtorsFile != null)
                        {
                            outputWriter.WriteLine(Path.Combine(projectPath, options.CtorsFile));
                        }
                    }
                }
                else
                {
                    foreach (var f in files)
                    {
                        if (Path.GetExtension(f.Key) == ".h")
                        {
                            File.Copy(f.Key, Path.Combine(outputFile, f.Value + Path.GetExtension(f.Key)));
                        }
                        else
                        {
                            File.Copy(f.Key, Path.Combine(outputFile, Path.GetFileName(f.Key)));
                        }
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Output file cannot be written.");
                return;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }
    }
}
