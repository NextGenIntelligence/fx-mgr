Fx-Mgr is a configuration tool for C/C++ applications. It is based on metadata concept: every header and source file should contain metadata written in a JSON-like language. Metadata describes relationships between files and modules, also special technique is utilized in order to make #include understand modules instead of files. The configurator is responsible for metadata extraction, application model building, determining what files should be built, options and external definitions tracking, aspect weaving, dependency injection and so on.
The configurator contains the following components:
1) Metadata provider: DLL which is used to extract and parse metadata contained in source files (in the current version it is implemented just with regular expressions, more intellectual implementation should use the preprocessor in order to extract imports properly).
2) Configuration framework: set of classes implementing dependency tracking, interface to implementation translation and support of abstract application model in terms of modules and their dependencies.
3) Configuration modules: set of classes implementing some functionality based on framework, for example determining which files have to be built in a given application configuration.
4) Configurator application based on common framework. Two implementations are in depelopment: console and GUI application. GUI one is still in the prototype stage and uses proprietary GUI controls (DevExpress).

Examples for demonstrating functionality described above with the console application are available here: 
http://fxrtos.ru/Media/Storage/fx-mgr-demo.zip

Precompiled FX-Mgr binaries:
http://fxrtos.ru/Media/Storage/fxmgr_0_3.zip

Special thanks to Antoine Aubry for the YAML parser and to Angel Johnson for the verification.
