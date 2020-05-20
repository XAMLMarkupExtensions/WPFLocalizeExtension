The project was restructured to separate the target identification and value conversion in the markup extension from the logic that actually provides the value by introducing the ILocalizationProvider interface. This enables us to plug in other provider services without touching the base, the LocExtension. 

### Changing the provider
The provider can be changed using the **LocalizeDictionary.Provider** attached property at any node in the XAML document. Depending on the provider, a static singleton reference (e.g. resx provider) or an instance (e.g. csv provider) must be assigned to this property.

The default provider is set to the ResxLocalizationProvider. If you need another application wide default provider, just overwrite the static backed-up attached property **LocalizeDictionary.DefaultProvider**. 

### Provider features
Besides its localized look up functionality, providers can give you a list of available cultures. This observable collection can be read out or bound - whatever you need or prefer. The list is additionally observed by the LocalizeDictionary instance providing a merged bindable list of all available cultures.

Furthermore, providers fire events, when critical values in the provider changed (triggering an update of the LocExtension) or when an error occured. 

### Implementing custom providers
To implement your own provider, create a class that implements the ILocalizationProvider interface. There is no restriction concerning its base class.
```c#
public interface ILocalizationProvider
{
    /// <summary>
    /// Get the localized object.
    /// </summary>
    /// <param name="key">The key to the value.</param>
    /// <param name="target">The target <see cref="DependencyObject"/>.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The value corresponding to the key and culture.</returns>
    object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture);

    /// <summary>
    /// An observable list of available cultures.
    /// </summary>
    ObservableCollection<CultureInfo> AvailableCultures { get; }

    /// <summary>
    /// An event when the provider changed.
    /// </summary>
    event ProviderChangedEventHandler ProviderChanged;

    /// <summary>
    /// An event when an error occurred.
    /// </summary>
    event ProviderErrorEventHandler ProviderError;
}
```
You can then assign your own provider to the properties of LocalizeDictionary.

***
Previous topic: [Multiple assemblies and dictionaries](Multiple-assemblies-and-dictionaries)  
Next topic: [LocProxy & EnumComboBox](LocProxy-&-EnumComboBox)