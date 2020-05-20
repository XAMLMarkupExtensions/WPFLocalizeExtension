The library is under static development in order to improve it continuously with more features. This change log will give you an overview over the past milestones. 

### v1.1.0 > v2.0.0
* Closed issues 7128, 6129 and 4018 
* Fixed bug of discussion _NullReferenceException_ in _LocExtension_ 
* Introduced _ILocalizationProvider_ interface 
* Call _GetLocalizedObject(...)_ for resolving a localized value 
* Sign in to _ProviderChanged_ and _ProviderError_ for notifications about provider changes and errors 
* Get the list of _AvailableCultures_ 
* _AttachedProperty_ Provider and _DefaultProvider_ for _LocalizeDictionary_ 
* _ResxLocalizationProvider_ singleton implementation for full backward compatibility (for small changes please refer to [Common mistakes - Access modifier for resource assemblies](Common-mistakes#access-modifier-for-resource-assemblies)) 
* _CSVLocalizationProvider_ example
* Needs at least XAML Markup Extensions v1.1.3

### v1.0.4 > v1.1.0
* Small bug fixes 
* Silverlight 5.0 support

### v1.0.3 > v1.0.4
* Changed to XAML Markup Extensions v1.0.2 (list support)

### v1.0.2 > v1.0.3
* Changed extension base to XAML Markup Extensions 
* Default dictionary and assembly feature 
* Automatic key retrieval feature 
* Nesting capability feature

### v20091031 > v1.0.2
* Small bug fixes 
* Source code transferred to GitHub 
* Deployment over NuGet

***
Previous topic: [Supported platforms](Supported-platforms)  
Next topic: [Installation and dependencies](Installation-and-dependencies)