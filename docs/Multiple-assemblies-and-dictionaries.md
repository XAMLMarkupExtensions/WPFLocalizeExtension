In the previous sections we presumed that a default assembly and dictionary indicating the location of the resource file is set up in the XAML document root. There may be situations, where more than one assembly and/or dictionary is used for a single XAML code file. This scenario is fully supported by the LocExtension and **ResxLocalizationProvider**. 

### Switching assemblies and dictionaries
You may change the values of **ResxLocalizationProvider.DefaultAssembly** and **ResxLocalizationProvider.DefaultDictionary** on arbitrary locations throughout the VisualTree of your XAML code - where and as often you need it. The various LocExtension instances will automatically resolve the corresponding values. This implies, that you may also change both values dynamically during runtime. Refer to the test application AssemblyTest in the source code of the library. 

### Directly provided assembly and/or dictionary
You may also define the assembly or dictionary that will be used at each instance of LocExtension by using their properties _Assembly_ or _Dict_ respectively. Alternatively, you may use the constructor syntax already described for the keys. The following three buttons will have the same text:
```xaml
<Button Content="{lex:Loc MyAssembly:MyResource:Test}" />
// If you need different Assembly or Dic you should use the attached properties of ResxLocalizationProvider
<Button Content="{lex:Loc Test}" />
<Button Content="{lex:Loc Key=Test}" />
```
### Assembly name differing from the default namespace
This scenario is supported by the ResxLocalizationProvider. In case you have several resource files in a single assembly with the same name but in different namespaces, you have to provide a qualified name of your dictionary.

***
Previous topic: [Keys](Keys)  
Next topic: [Localization providers](Localization-providers)