# Key strategy

## Resx Provider 

To find a resx ressource the system needs an assembly name, a ressource name and the key for the resx ressource.
The easiest way to provide the Assembly and Ressource via the Attached properties:

ResxLocalizationProvider.DefaultAssembly  and ResxLocalizationProvider.DefaultDictionary

You can attach this properties to any element in the visual tree and this is valid for all child nodes.
If you want to override you can add this again on a child node and override so all childs.

[ADD SCOPE EXAMPLE PICTURE]

In the key retrieval process there is the possibility to directly define the dictionary & assembly.

```xaml
{lex:Loc KEY}
{lex:Loc DICTIONARY:KEY}
{lex:Loc ASSEMBLY:DICTIONARY:KEY}
```

### Inheriting Resx

The is also an Inheriting Resx Proviver there all attached properties are defined with the FrameworkPropertyMetadataOptions.Inherits attribute,
so that WPF make on each child node a copy. This reduces the overhead because no search on parent nodes is nessesary, but decrease massive the
WPF perfomance because of the copy process. So we intent not to use it.

### Automatic key retrieval
If the control already got or can get a value for its _Name_ or _x:Name_ property, the key may be neglected, provided, that a key exists that matches one of the following criteria (checked in exactly this order): 

* _ControlName_PropertyName_ 
* _ControlName_

Using our button example we could imagine the following scenario:
```xaml
<Button Name="MyButton" Content="{lex:Loc}" />
```
As no key was provided, the extension will first try to resolve a resource key named _MyButton_Content_. If this fails, it will then look for a resource key named _MyButton_. If this fails too, no value will be provided.

The separation character (default is underscore) can be individually set up for each control using the _LocalizeDictionary.Separation_ property.
