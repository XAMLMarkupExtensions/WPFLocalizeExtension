### Access modifier for resource assemblies
The LocalizationExtension will fail to look up a localized value, if the resource is compiled with its access modifier set to internal. To set up the resource build tool to use a public access modifier, open the resource file and change the value of the field.

### NeutralResourcesLanguage in Assembly Manifest
The Assembly.cs contains sometimes the line _[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]_ which has side effects on resource lookups.

Please remove it.

### Bad ResX Filename
As long as you are using custom ResX files that are not located under the Properties folder in your project you **may not call the file Resource**. It will break the lookup mechanism for the resources.