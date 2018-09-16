#region Copyright information
// <copyright file="LocExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Providers;
using WPFLocalizeExtension.TypeConverters;
using XAMLMarkupExtensions.Base;

namespace WPFLocalizeExtension.Extensions
{
    /// <summary>
    /// A generic localization extension.
    /// </summary>
    [ContentProperty("ResourceIdentifierKey")]
    public class LocExtension : NestedMarkupExtension, INotifyPropertyChanged, IDictionaryEventListener, IDisposable
    {
        #region PropertyChanged Logic
        /// <summary>
        /// Informiert über sich ändernde Eigenschaften.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify that a property has changed
        /// </summary>
        /// <param name="property">
        /// The property that changed
        /// </param>
        internal void OnNotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region Variables
        private static readonly object ResourceBufferLock = new object();
        private static readonly object ResolveLock = new object();

        private static Dictionary<string, object> _resourceBuffer = new Dictionary<string, object>();

        /// <summary>
        /// Holds the Key to a .resx object
        /// </summary>
        private string _key;

        /// <summary>
        /// A custom converter, supplied in the XAML code.
        /// </summary>
        private IValueConverter _converter;

        /// <summary>
        /// A parameter that can be supplied along with the converter object.
        /// </summary>
        private object _converterParameter;

        /// <summary>
        /// The last endpoint that was used for this extension.
        /// </summary>
        private SafeTargetInfo _lastEndpoint;
        #endregion

        #region Resource buffer handling.
        /// <summary>
        /// Clears the common resource buffer.
        /// </summary>
        public static void ClearResourceBuffer()
        {
            lock (ResourceBufferLock)
            {
                _resourceBuffer?.Clear();
                _resourceBuffer = null;
            }
        }

        /// <summary>
        /// Adds an item to the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        internal static void SafeAddItemToResourceBuffer(string key, object item)
        {
            lock (ResourceBufferLock)
            {
                if (!LocalizeDictionary.Instance.DisableCache && !_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Add(key, item);
            }
        }

        /// <summary>
        /// Removes an item from the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void SafeRemoveItemFromResourceBuffer(string key)
        {
            lock (ResourceBufferLock)
            {
                if (_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Remove(key);
            }
        }
        #endregion

        #region GetBoundExtension
        /// <summary>
        /// Gets the extension that is bound to a given target. Please note, that only the last endpoint of each extension can be evaluated.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="property">The target property name.</param>
        /// <param name="propertyIndex">The index in the property (if applicable).</param>
        /// <returns>The bound extension or null, if not available.</returns>
        public static LocExtension GetBoundExtension(object target, string property, int propertyIndex = -1)
        {
            foreach (var ext in LocalizeDictionary.DictionaryEvent.EnumerateListeners<LocExtension>())
            {
                var ep = ext._lastEndpoint;
                
                if (ep.TargetObjectReference.Target == null)
                    continue;

                var epProp = GetPropertyName(ep.TargetProperty);

                if (ep.TargetObjectReference.Target == target &&
                    epProp == property &&
                    ep.TargetPropertyIndex == propertyIndex)
                    return ext;
            }

            return null;
        }

        /// <summary>
        /// Get the name of a property (regular or DependencyProperty).
        /// </summary>
        /// <param name="property">The property object.</param>
        /// <returns>The name of the property.</returns>
        private static string GetPropertyName(object property)
        {
            var epProp = "";

            if (property is PropertyInfo info)
                epProp = info.Name;
            else if (property is DependencyProperty)
            {
                epProp = ((DependencyProperty)property).Name;
            }

            // What are these names during design time good for? Any suggestions?
            if (epProp.Contains("FrameworkElementWidth5"))
                epProp = "Height";
            else if (epProp.Contains("FrameworkElementWidth6"))
                epProp = "Width";
            else if (epProp.Contains("FrameworkElementMargin12"))
                epProp = "Margin";

            return epProp;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the Key to a .resx object
        /// </summary>
        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    UpdateNewValue();

                    OnNotifyPropertyChanged(nameof(Key));
                }
            }
        }

        /// <summary>
        /// Gets or sets the custom value converter.
        /// </summary>
        public IValueConverter Converter
        {
            get
            {
                if (_converter == null)
                    _converter = new DefaultConverter();

                return _converter;
            }
            set => _converter = value;
        }

        /// <summary>
        /// Gets or sets the converter parameter.
        /// </summary>
        public object ConverterParameter
        {
            get => _converterParameter;
            set => _converterParameter = value;
        }

        /// <summary>
        /// Gets or sets the culture to force a fixed localized object
        /// </summary>
        public string ForceCulture { get; set; }

        /// <summary>
        /// Gets or sets the initialize value.
        /// This is ONLY used to support the localize extension in blend!
        /// </summary>
        /// <value>The initialize value.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [ConstructorArgument("key")]
        public string InitializeValue { get; set; }

        /// <summary>
        /// Gets or sets the Key that identifies a resource (Assembly:Dictionary:Key)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ResourceIdentifierKey
        {
            get => _key ?? "(null)";
            set => _key = value;
        }
        #endregion

        #region Constructors & Dispose
        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        public LocExtension()
        {
            // Register this extension as an event listener on the first target.
            OnFirstTarget = () =>
            {
                LocalizeDictionary.DictionaryEvent.AddListener(this);
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public LocExtension(string key)
            : this()
        {
            Key = key;
        }

        /// <summary>
        /// Removes the listener from the dictionary.
        /// <para>The "new" keyword is just a temporary hack in order to keep XAMLMarkupExtensions on the current version.</para>
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();
            LocalizeDictionary.DictionaryEvent.RemoveListener(this);
        }

        /// <summary>
        /// The finalizer.
        /// </summary>
        ~LocExtension()
        {
            Dispose();
        }
        #endregion

        #region IDictionaryEventListener implementation
        /// <summary>
        /// This method is called when the resource somehow changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        public void ResourceChanged(DependencyObject sender, DictionaryEventArgs e)
        {
            ClearItemFromResourceBuffer(e);
            if (sender == null)
            {
                UpdateNewValue();
                return;
            }

            // Update, if this object is in our endpoint list.
            var targetDOs = (from p in GetTargetPropertyPaths()
                             select p.EndPoint.TargetObject as DependencyObject);

            foreach (var dObj in targetDOs)
            {
                if (LocalizeDictionary.Instance.DefaultProvider is InheritingResxLocalizationProvider)
                {
                    UpdateNewValue();
                    break;
                }

                var doParent = dObj;
                while (doParent != null)
                {
                    if (sender == doParent)
                    {
                        UpdateNewValue();
                        break;
                    }
                    if (!(doParent is Visual) && !(doParent is Visual3D) && !(doParent is FrameworkContentElement))
                    {
                        UpdateNewValue();
                        break;
                    }
                    try
                    {
                        DependencyObject doParent2;

                        if (doParent is FrameworkContentElement element)
                            doParent2 = element.Parent;
                        else
                            doParent2 = doParent.GetParent(true);

                        if (doParent2 == null && doParent is FrameworkElement)
                            doParent2 = ((FrameworkElement)doParent).Parent;

                        doParent = doParent2;
                    }
                    catch
                    {
                        UpdateNewValue();
                        break;
                    }
                }
            }
        }

        private void ClearItemFromResourceBuffer(DictionaryEventArgs dictionaryEventArgs)
        {
            if (dictionaryEventArgs.Type == DictionaryEventType.ValueChanged && dictionaryEventArgs.Tag is ValueChangedEventArgs)
            {
                var args = (ValueChangedEventArgs)dictionaryEventArgs.Tag;
                var ci = args.Tag as CultureInfo;

                lock (ResolveLock)
                {
                    foreach (var key in _resourceBuffer.Keys.ToList())
                    {
                        if (key.EndsWith(args.Key))
                        {
                            if (ci == null || key.StartsWith(ci.Name))
                            {
                                if (_resourceBuffer[key] != args.Value)
                                    SafeRemoveItemFromResourceBuffer(key);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Forced culture handling
        /// <summary>
        /// If Culture property defines a valid <see cref="CultureInfo"/>, a <see cref="CultureInfo"/> instance will get
        /// created and returned, otherwise <see cref="LocalizeDictionary"/>.Culture will get returned.
        /// </summary>
        /// <returns>The <see cref="CultureInfo"/></returns>
        /// <exception cref="System.ArgumentException">
        /// thrown if the parameter Culture don't defines a valid <see cref="CultureInfo"/>
        /// </exception>
        protected CultureInfo GetForcedCultureOrDefault()
        {
            // define a culture info
            CultureInfo cultureInfo;

            // check if the forced culture is not null or empty
            if (!string.IsNullOrEmpty(ForceCulture))
            {
                // try to create a valid cultureinfo, if defined
                try
                {
                    // try to create a specific culture from the forced one
                    // cultureInfo = CultureInfo.CreateSpecificCulture(this.ForceCulture);
                    cultureInfo = new CultureInfo(ForceCulture);
                }
                catch (ArgumentException ex)
                {
                    // on error, check if designmode is on
                    if (LocalizeDictionary.Instance.GetIsInDesignMode())
                    {
                        // cultureInfo will be set to the current specific culture
                        cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
                    }
                    else
                    {
                        // tell the customer, that the forced culture cannot be converted propperly
                        throw new ArgumentException("Cannot create a CultureInfo with '" + ForceCulture + "'", ex);
                    }
                }
            }
            else
            {
                // take the current specific culture
                cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
            }

            // return the evaluated culture info
            return cultureInfo;
        }
        #endregion

        #region TargetMarkupExtension implementation
        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">Information about the endpoint.</param>
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            object result = null;

            if (endPoint == null)
                return null;
            else
                _lastEndpoint = SafeTargetInfo.FromTargetInfo(endPoint);

            var targetObject = endPoint.TargetObject as DependencyObject;

            // Get target type. Change ImageSource to BitmapSource in order to use our own converter.
            var targetType = info.TargetPropertyType;

            if (targetType == typeof(ImageSource))
                targetType = typeof(BitmapSource);

            // In case of a list target, get the correct list element type.
            if ((info.TargetPropertyIndex != -1) && typeof(IList).IsAssignableFrom(info.TargetPropertyType))
                targetType = info.TargetPropertyType.GetGenericArguments()[0];
            
            // Try to get the localized input from the resource.
            var resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(Key, targetObject);
            var ci = GetForcedCultureOrDefault();

            // Extract the names of the endpoint object and property
            var epProp = GetPropertyName(endPoint.TargetProperty);
            var epName = "";
            
            if (endPoint.TargetObject is FrameworkElement)
                epName = ((FrameworkElement)endPoint.TargetObject).GetValueSync<string>(FrameworkElement.NameProperty);
            else if (endPoint.TargetObject is FrameworkContentElement)
                epName = ((FrameworkContentElement)endPoint.TargetObject).GetValueSync<string>(FrameworkContentElement.NameProperty);

            var resKeyBase = ci.Name + ":" + targetType.Name + ":";
            // Check, if the key is already in our resource buffer.
            object input = null;
            var isDefaultConverter = Converter is DefaultConverter;

            if (!String.IsNullOrEmpty(resourceKey))
            {
                // We've got a resource key. Try to look it up or get it from the dictionary.
                lock (ResourceBufferLock)
                {
                    if (isDefaultConverter && _resourceBuffer.ContainsKey(resKeyBase + resourceKey))
                        result = _resourceBuffer[resKeyBase + resourceKey];
                    else
                    {
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, targetObject, ci);
                        resKeyBase += resourceKey;
                    }
                }
            }
            else
            {
                var resKeyNameProp = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName + LocalizeDictionary.GetSeparation(targetObject) + epProp, targetObject);
                
                // Try the automatic lookup function.
                // First, look for a resource entry named: [FrameworkElement name][Separator][Property name]
                lock (ResourceBufferLock)
                {
                    if (isDefaultConverter && _resourceBuffer.ContainsKey(resKeyBase + resKeyNameProp))
                        result = _resourceBuffer[resKeyBase + resKeyNameProp];
                    else
                    {
                        // It was not stored in the buffer - try to retrieve it from the dictionary.
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyNameProp, targetObject, ci);

                        if (input == null)
                        {
                            var resKeyName = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName, targetObject);

                            // Now, try to look for a resource entry named: [FrameworkElement name]
                            // Note - this has to be nested here, as it would take precedence over the first step in the buffer lookup step.
                            if (isDefaultConverter && _resourceBuffer.ContainsKey(resKeyBase + resKeyName))
                                result = _resourceBuffer[resKeyBase + resKeyName];
                            else
                            {
                                input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyName, targetObject, ci);
                                resKeyBase += resKeyName;
                            }
                        }
                        else
                            resKeyBase += resKeyNameProp;
                    }
                }
            }

            // If no result was found, convert the input and add it to the buffer.
            if (result == null)
            {
                if (input != null)
                {
                    result = Converter.Convert(input, targetType, ConverterParameter, ci);
                    if (isDefaultConverter)
                        SafeAddItemToResourceBuffer(resKeyBase, result);
                }
                else
                {
                    if (LocalizeDictionary.Instance.OnNewMissingKeyEvent(this, _key))
                        UpdateNewValue();

                    if (LocalizeDictionary.Instance.OutputMissingKeys
                        && !string.IsNullOrEmpty(_key) && (targetType == typeof(String) || targetType == typeof(object)))
                        result = "Key: " + _key;
                }
            }

            return result;
        }

        /// <summary>
        /// This method must return true, if an update shall be executed when the given endpoint is reached.
        /// This method is called each time an endpoint is reached.
        /// </summary>
        /// <param name="endpoint">Information on the specific endpoint.</param>
        /// <returns>True, if an update of the path to this endpoint shall be performed.</returns>
        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            // This extension must be updated, when an endpoint is reached.
            return true;
        }
        #endregion

        #region Resolve functions
        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <typeparam name="TValue">The type of the returned value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="converter">An optional converter.</param>
        /// <param name="converterParameter">An optional converter parameter.</param>
        /// <returns>The resolved localized object.</returns>
        public static TValue GetLocalizedValue<TValue>(string key, IValueConverter converter = null, object converterParameter = null)
        {
            var targetCulture = LocalizeDictionary.Instance.SpecificCulture;
            return GetLocalizedValue<TValue>(key, targetCulture, null, converter, converterParameter);
        }

        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <typeparam name="TValue">The type of the returned value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <param name="converter">An optional converter.</param>
        /// <param name="converterParameter">An optional converter parameter.</param>
        /// <returns>The resolved localized object.</returns>
        public static TValue GetLocalizedValue<TValue>(string key, CultureInfo targetCulture, IValueConverter converter = null, object converterParameter = null)
        {
            return GetLocalizedValue<TValue>(key, targetCulture, null, converter, converterParameter);
        }

        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <typeparam name="TValue">The type of the returned value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="converter">An optional converter.</param>
        /// <param name="converterParameter">An optional converter parameter.</param>
        /// <returns>The resolved localized object.</returns>
        public static TValue GetLocalizedValue<TValue>(string key, DependencyObject target, IValueConverter converter = null, object converterParameter = null)
        {
            var targetCulture = LocalizeDictionary.Instance.SpecificCulture;
            return GetLocalizedValue<TValue>(key, targetCulture, target, converter, converterParameter);
        }

        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <typeparam name="TValue">The type of the returned value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="converter">An optional converter.</param>
        /// <param name="converterParameter">An optional converter parameter.</param>
        /// <returns>The resolved localized object.</returns>
        public static TValue GetLocalizedValue<TValue>(string key, CultureInfo targetCulture, DependencyObject target, IValueConverter converter = null, object converterParameter = null)
        {
            lock (ResolveLock)
            {
                var result = default(TValue);

                var resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(key, target);

                // Get the localized object from the dictionary
                var resKey = targetCulture.Name + ":" + typeof (TValue).Name + ":" + resourceKey;
                var isDefaultConverter = converter is DefaultConverter;

                if (isDefaultConverter && _resourceBuffer.ContainsKey(resKey))
                    result = (TValue) _resourceBuffer[resKey];
                else
                {
                    var localizedObject = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, target,
                        targetCulture);

                    if (localizedObject == null)
                        return result;

                    if (converter == null)
                        converter = new DefaultConverter();

                    var tmp = converter.Convert(localizedObject, typeof (TValue), converterParameter, targetCulture);

                    if (tmp is TValue value)
                    {
                        result = value;
                        if (isDefaultConverter)
                            SafeAddItemToResourceBuffer(resKey, result);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue)
        {
            // return the resolved localized value with the current or forced culture.
            return ResolveLocalizedValue(out resolvedValue, GetForcedCultureOrDefault(), null);
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair and the given target.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <param name="target">The target object.</param>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue, DependencyObject target)
        {
            // return the resolved localized value with the current or forced culture.
            return ResolveLocalizedValue(out resolvedValue, GetForcedCultureOrDefault(), target);
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue, CultureInfo targetCulture)
        {
            return ResolveLocalizedValue(out resolvedValue, targetCulture, null);
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair and the given target.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <param name="target">The target object.</param>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue, CultureInfo targetCulture, DependencyObject target)
        {
            // define the default value of the resolved value
            resolvedValue = default(TValue);

            var resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(Key, target);

            // get the localized object from the dictionary
            var resKey = targetCulture.Name + ":" + typeof(TValue).Name + ":" + resourceKey;
            var isDefaultConverter = Converter is DefaultConverter;

            lock (ResourceBufferLock)
            {
                if (isDefaultConverter && _resourceBuffer.ContainsKey(resKey))
                {
                    resolvedValue = (TValue)_resourceBuffer[resKey];
                }
                else
                {
                    var localizedObject = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, target, targetCulture);

                    if (localizedObject == null)
                        return false;

                    var result = Converter.Convert(localizedObject, typeof(TValue), ConverterParameter, targetCulture);
                
                    if (result is TValue value)
                    {
                        resolvedValue = value;
                        if (isDefaultConverter)
                            SafeAddItemToResourceBuffer(resKey, resolvedValue);
                    }
                }
            }

            if (resolvedValue != null)
                return true;

            return false;
        }
        #endregion

        #region Code-behind binding
        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target dependency object</param>
        /// <param name="targetProperty">The target property</param>
        /// <returns>
        /// TRUE if the binding was setup successfully, otherwise FALSE (Binding already exists).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="targetProperty"/> is
        /// not a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
        /// </exception>
        public bool SetBinding(DependencyObject targetObject, object targetProperty)
        {
            return SetBinding((object)targetObject, targetProperty, -1);
        }

        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target object</param>
        /// <param name="targetProperty">The target property</param>
        /// <returns>
        /// TRUE if the binding was setup successfully, otherwise FALSE (Binding already exists).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="targetProperty"/> is
        /// not a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
        /// </exception>
        public bool SetBinding(object targetObject, object targetProperty)
        {
            return SetBinding(targetObject, targetProperty, -1);
        }

        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target dependency object</param>
        /// <param name="targetProperty">The target property</param>
        /// <param name="targetPropertyIndex">The index of the target property. (only used for Lists)</param>
        /// <returns>
        /// TRUE if the binding was setup successfully, otherwise FALSE (Binding already exists).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="targetProperty"/> is
        /// not a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
        /// </exception>
        public bool SetBinding(DependencyObject targetObject, object targetProperty, int targetPropertyIndex)
        {
            return SetBinding((object)targetObject, targetProperty, targetPropertyIndex);
        }

        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target object</param>
        /// <param name="targetProperty">The target property</param>
        /// <param name="targetPropertyIndex">The index of the target property. (only used for Lists)</param>
        /// <returns>
        /// TRUE if the binding was setup successfully, otherwise FALSE (Binding already exists).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="targetProperty"/> is
        /// not a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
        /// </exception>
        public bool SetBinding(object targetObject, object targetProperty, int targetPropertyIndex)
        {
            var existingBinding = (from info in GetTargetPropertyPaths()
                                   where (info.EndPoint.TargetObject == targetObject) && (info.EndPoint.TargetProperty == targetProperty)
                                   select info).FirstOrDefault();

            // Return false, if the binding already exists
            if (existingBinding != null)
                return false;

            Type targetPropertyType = null;

            if (targetProperty is DependencyProperty)
                targetPropertyType = ((DependencyProperty)targetProperty).PropertyType;
            else if (targetProperty is PropertyInfo)
                targetPropertyType = ((PropertyInfo)targetProperty).PropertyType;

            var result = ProvideValue(new SimpleProvideValueServiceProvider(targetObject, targetProperty, targetPropertyType, targetPropertyIndex));

            SetPropertyValue(result, new TargetInfo(targetObject, targetProperty, targetPropertyType, targetPropertyIndex), false);

            return true;
        }
        #endregion

        /// <summary>
        /// Overridden, to return the key of this instance.
        /// </summary>
        /// <returns>Loc: + key</returns>
        public override string ToString()
        {
            return "Loc:" + _key;
        }
    }
}