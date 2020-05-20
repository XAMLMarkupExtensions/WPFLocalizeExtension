You have to initialize the localization engine in order to get values at design-time and to set up default dictionaries and assemblies (refer to [Multiple assemblies and dictionaries](Multiple-assemblies-and-dictionaries.md)).

First of all, create a _xmlns_ reference to "http://wpflocalizeextension.codeplex.com". The next step is to set the design time culture as well as default assembly and dictionary where the resource is located:
```xaml
<Window xmlns:lex="http://wpflocalizeextension.codeplex.com"
        lex:LocalizeDictionary.DesignCulture="en"
        lex:ResxLocalizationProvider.DefaultAssembly="AssemblyTestResourceLib"
        lex:ResxLocalizationProvider.DefaultDictionary="Strings">
<!-- Some controls -->
</Window>
```
The value of DesignCulture can be changed dynamically during design time and will have an immediate effect on all localized properties. This gives you the possibility to check the correct localization directly in the WPF designer. This example configures the resx provider. For a full explanation on this provider, refer to [Localization providers](Localization-providers.md).
