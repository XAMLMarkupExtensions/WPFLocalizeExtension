# LocalizationExtension #
## is a realy easy way to localize any type of DependencyProperties or native Properties on DependencyObects ##

### Features:###

* first of all: ITS FREE (and will stay free)
* is in a real stabel state
* supports binding-like write style like "Text = {lex:LocText ResAssembly:ResFile:ResKey}"
    * if ResAssembly and / or ResFile is the default, you can skip these parameters to ResAssembly::ResKey, ResFile:ResKey or only ResKey
* works with the .resx-fallback mechanism (e.g. en-us -> en -> independent culture)
* is available at designtime (MS Expression Blend 3.0, MS Expression Blend 4.0, MS VisualStudio 2008 (Normal and SP1), MS VisualStudio 2010
    * not for dynamic loaded assemblies which only can be found at runtime and as long the resource (.resx) is builded at designtime
* supports culture forcing (e.g. "this object has to be in english all the time")
* works with normal dependency properties
* works with normal properties (e.g. Ribbon)
* works with control/data templates
* The LocalizeExtension offers DesignValue to support custom values during design mode
* can be used in code behind to bind localized values to dynamic generated controls
* implements INotifyPropertyChanged for advanced use
* supports formating e.g. "this is the '{0}' value" (not bindable at the moment)
* supports prefix and suffix values (currently with LocText extension)
* is in use in productive systems (like my public relation product)
* switching of the language to runtime affects NO timeslice
* can be used with any resource file (.resx) accross all assemblies (also the dynamic loaded one at runtime)
* dont need any initializing process (like "call xyz to register a special localize dictionary")
* change of the choosen language is possible at designtime
* can localize any type of data type, as long a converter (TypeConverter) exists for it extend LocalizeExtension)
* has build in support for Text, upper Text, lower Text, Images, Brushes, Double, Thickness and Flow direction
* dont affects any memory leaks
* leaves the UID property untouched
* offers a "SpecificCulture" to use as IFormatProvider (e.g. (123.20).ToString(LocalizeDictionary.SpecificCulture) = "123.20" or "123,20")
* offers some functionality to check and get resource values in code behind (e.g. ResolveLocalizedValue)
* do not alter the culture on Thread.CurrentCulture or Thread.CurrentUICulture (can be changed easily)

-----

### Homepage: ###
http://wpflocalizeextension.codeplex.com/  
http://root-project.org/

### NuGet Package: ###
https://nuget.org/packages/WpfLocalizeExtension/

### Based on XAML Markup Extensions: ###
http://xamlmarkupextensions.codeplex.com/