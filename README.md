# LocalizationExtension #
### Is a really easy way to localize any type of DependencyProperties or native Properties on DependencyObjects ###

### Features:###

* First of all: ITS FREE (and will stay free - refer to license section below)
* Obtain stable results in
	* WPF applications using .NET 3.5 and higher
	* **New:** Silverlight 5.0 applications
* Supports binding-like write style like "Text = {lex:LocText ResAssembly:ResFile:ResKey}"
	* Define a default assembly and / or resource file to reduce the key to ResAssembly::ResKey, ResFile:ResKey or even ResKey
	* If no key is specified, the Name and Property Name of the target are used (e.g. MyButton_Content)
	* Default assembly, dictionary and culture can be changed dynamically
	* Default assembly and dictionary inherit along the visual tree and can be redefined at each node
* It is available at designtime (MS Expression Blend 3.0, MS Expression Blend 4.0, MS VisualStudio 2008 (Normal and SP1), MS VisualStudio 2010
    * not for dynamic loaded assemblies which only can be found at runtime and as long the resource (.resx) is built at designtime
	* not in Silverlight at designtime (generally not supported by Ms)
	* Offers a DesignValue Property to support custom values during design mode
* Full support of various application scenarios
	* Works with normal dependency properties
	* Works with normal properties (e.g. Ribbon)
	* Works with control/data templates
* Various culture setup features
	* Works with the .resx-fallback mechanism (e.g. en-us -> en -> invariant culture)
	* Supports culture forcing (e.g. "this object has to be in english all the time")
	* Buffering allows fast switching of the language at runtime
	* Offers a design language for visual testing at designtime
	* Offers a "SpecificCulture" to use as IFormatProvider (e.g. (123.20).ToString(LocalizeDictionary.SpecificCulture) = "123.20" or "123,20")
	* Does not alter the culture on Thread.CurrentCulture or Thread.CurrentUICulture (can be changed easily)
* Code behind features:
	* Can be used in code behind to bind localized values to dynamic generated controls
	* Implements INotifyPropertyChanged for advanced use
	* Offers some functionality to check and get resource values in code behind (e.g. ResolveLocalizedValue)
* Easy to use
	* Can be used with any resource file (.resx) accross all assemblies (also the dynamic loaded one at runtime)
	* Does not need any initializing process (like "call xyz to register a special localize dictionary")
	* Can localize any type of data type, as long a TypeConverter exists for it
* Example extensions included for
	* Formating e.g. "this is the '{0}' value" (not bindable at the moment)
	* Prefix and suffix values (currently with LocText extension)
	* Upper and lower Text
* Last, but not least
	* Does not create any memory leaks
	* Leaves the UID property untouched
	* Is in use in various productive systems

### License: ###
Microsoft Public License (Ms-PL)

-----

### Homepage: ###
http://wpflocalizeextension.codeplex.com/  
http://root-project.org/

### NuGet Package: ###
https://nuget.org/packages/WpfLocalizeExtension/

### Based on XAML Markup Extensions: ###
http://xamlmarkupextensions.codeplex.com/