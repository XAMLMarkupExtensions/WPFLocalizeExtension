The project is based on the MarkupExtension class of the .NET framework. All classes that derive from this class are asked by the framework to deliver a value to the target object and property.

The LocalizationExtension uses this feature in order to provide the culture-specific values to localized properties. The general syntax of markup extensions that is applied to a particular _Property_ of a certain _Element_ is as follows:
```xaml
<Element Property="{ExtensionName ...}" />
```
The _ExtensionName_ has to be replaced by the class name of the extension without its ending "Extension". Usually, the XAML editor will propose the possible extensions to you. After the name, several properties of the extension itself can be initialized. It is obvious that this mechanism gives us the possibility to easily mark the localized property and give necessary information to look up its resource key.

***
Previous topic: [Localization](Localization)  
Next topic: [Supported platforms](Supported-platforms)