Keys are entered either directly after the extension name or using the Key property of the LocExtension class. Both will yield the same result (here with the key named Test):
```xaml
<Button Content="{lex:Loc Test}" />
<Button Content="{lex:Loc Key=Test}" />
```
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

***
Previous topic: [Our first localized text](Our-first-localized-text)  
Next topic: [Multiple assemblies and dictionaries](Multiple-assemblies-and-dictionaries)