### General instructions

As explained in the section [Supported platforms](Supported-platform.md), the LocalizationExtension supports **resx** files for the storage of localized values. Create such a resource file with a suitable name and check its access modifier, if needed (refer to [Common mistakes - Access modifier for resource assemblies](Common-mistakes.md#access-modifier-for-resource-assemblies)). Now, create further resource files for each culture your application will support and give them the same name as the first one - just add the general or specific culture code (e.g. "en-US", "de", "de-AT", ...) before the ".resx" ending yielding: _Name.CultureCode_.resx. Don't forget the dot before the culture code.

Now, populate the resource files with your key/value pairs. Be sure to have the same name of a particular key for all cultures. You may use the automatic key retrieval mechanism (refer to [Keys](Keys.md)). 

### Pay attention to:
* The correct naming scheme of culture-specific resource files 
* The _Custom Tool_ property of the main resource file (ResXFileCodeGenerator), the others leave this field empty 
* The _Build_ Action property of all resource files (Embedded Resource) 
* The access modifier, if needed 
* Consistent key naming 
* **Rebuild** the project where the resource is located after adding new keys and values in order to see it in design time.

### How to get a treeview of culture-specific resources
1. Unload the project 
1. Edit the corresponding **csproj** file 
1. Locate the tags of the resources and rewrite them using the _DependentUpon_ syntax:
```xml
    <EmbeddedResource Include="Strings.de.resx">
      <SubType>Designer</SubType>
      <DependentUpon>Strings.resx</DependentUpon>
    </EmbeddedResource>
    <EmbeddedResource Include="Strings.resx">
      <Generator>ResXFileCodeGenerator</Generator>
      <LastGenOutput>Strings.Designer.cs</LastGenOutput>
    </EmbeddedResource>
```