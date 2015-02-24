#region Copyright information
// <copyright file="LocExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

#if SILVERLIGHT
namespace SLLocalizeExtension.Extensions
#else
namespace WPFLocalizeExtension.Extensions
#endif
{
    #region Uses
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
    using Engine;
    using Providers;
    using TypeConverters;
    using XAMLMarkupExtensions.Base;
    #endregion

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
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region Variables
        private static object resourceBufferLock = new object();
        private static Dictionary<string, object> ResourceBuffer = new Dictionary<string, object>();

        /// <summary>
        /// Holds the Key to a .resx object
        /// </summary>
        private string key;

        /// <summary>
        /// A custom converter, supplied in the XAML code.
        /// </summary>
        private IValueConverter converter = null;

        /// <summary>
        /// A parameter that can be supplied along with the converter object.
        /// </summary>
        private object converterParameter = null;

        /// <summary>
        /// The last endpoint that was used for this extension.
        /// </summary>
        private SafeTargetInfo lastEndpoint = null;
        #endregion

        #region Resource buffer handling.
        /// <summary>
        /// Clears the common resource buffer.
        /// </summary>
        public static void ClearResourceBuffer()
        {
            lock (resourceBufferLock)
            {
                if (ResourceBuffer != null)
                    ResourceBuffer.Clear();
            }
            
            ResourceBuffer = null;
        }


        /// <summary>
        /// Adds an item to the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        internal static void SafeAddItemToResourceBuffer(string key, object item)
        {
            lock (resourceBufferLock)
            {
                if (!LocalizeDictionary.Instance.DisableCache && !ResourceBuffer.ContainsKey(key))
                    ResourceBuffer.Add(key, item);
            }
        }

        /// <summary>
        /// Removes an item from the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void SafeRemoveItemFromResourceBuffer(string key)
        {
            lock (resourceBufferLock)
            {
                if (ResourceBuffer.ContainsKey(key))
                    ResourceBuffer.Remove(key);
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
                var ep = ext.lastEndpoint;
                
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

            if (property is PropertyInfo)
                epProp = ((PropertyInfo)property).Name;
            else if (property is DependencyProperty)
            {
#if SILVERLIGHT
                epProp = ((DependencyProperty)property).ToString();
#else
                epProp = ((DependencyProperty)property).Name;
#endif
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
            get { return key; }
            set
            {
                if (key != value)
                {
                    key = value;
                    UpdateNewValue();

                    OnNotifyPropertyChanged("Key");
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
                if (converter == null)
                    converter = new DefaultConverter();

                return converter;
            }
            set { converter = value; }
        }

        /// <summary>
        /// Gets or sets the converter parameter.
        /// </summary>
        public object ConverterParameter
        {
            get { return converterParameter; }
            set { converterParameter = value; }
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
#if SILVERLIGHT
#else
        [ConstructorArgument("key")]
#endif
        public string InitializeValue { get; set; }

        /// <summary>
        /// Gets or sets the Key that identifies a resource (Assembly:Dictionary:Key)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ResourceIdentifierKey
        {
            get { return key ?? "(null)"; }
            set { key = value; }
        }
        #endregion

        #region Constructors & Dispose
        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        public LocExtension()
            : base()
        {
            // Register this extension as an event listener on the first target.
            base.OnFirstTarget = () =>
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
            this.Key = key;
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
#if !SILVERLIGHT
                if (LocalizeDictionary.Instance.DefaultProvider is InheritingResxLocalizationProvider)
                {
                    UpdateNewValue();
                    break;
                }
#endif

                var doParent = dObj;
                while (doParent != null)
                {
                    if (sender == doParent)
                    {
                        UpdateNewValue();
                        break;
                    }
#if !SILVERLIGHT
                    if (!(doParent is Visual) && !(doParent is Visual3D) && !(doParent is FrameworkContentElement))
                    {
                        UpdateNewValue();
                        break;
                    }
#endif
                    try
                    {
                        DependencyObject doParent2;

#if !SILVERLIGHT
                        if (doParent is FrameworkContentElement)
                            doParent2 = ((FrameworkContentElement)doParent).Parent;
                        else
#endif
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
                var keysToRemove = new List<string>();
                var ci = args.Tag as CultureInfo;

                foreach (var key in ResourceBuffer.Keys.ToList())
                {
                    if (key.EndsWith(args.Key))
                    {
                        if (ci == null || key.StartsWith(ci.Name))
                        {
                            if (ResourceBuffer[key] != args.Value)
                                SafeRemoveItemFromResourceBuffer(key);
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
            if (!string.IsNullOrEmpty(this.ForceCulture))
            {
                // try to create a valid cultureinfo, if defined
                try
                {
                    // try to create a specific culture from the forced one
                    // cultureInfo = CultureInfo.CreateSpecificCulture(this.ForceCulture);
                    cultureInfo = new CultureInfo(this.ForceCulture);
                }
                catch (ArgumentException ex)
                {
                    // on error, check if designmode is on
                    if (LocalizeDictionary.Instance.GetIsInDesignMode())
                    {
                        // cultureInfo will be set to the current specific culture
#if SILVERLIGHT
                        cultureInfo = LocalizeDictionary.Instance.Culture;
#else
                        cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
#endif
                    }
                    else
                    {
                        // tell the customer, that the forced culture cannot be converted propperly
                        throw new ArgumentException("Cannot create a CultureInfo with '" + this.ForceCulture + "'", ex);
                    }
                }
            }
            else
            {
                // take the current specific culture
#if SILVERLIGHT
                cultureInfo = LocalizeDictionary.Instance.Culture;
#else
                cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
#endif
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
                lastEndpoint = SafeTargetInfo.FromTargetInfo(endPoint);

            var targetObject = endPoint.TargetObject as DependencyObject;

            // Get target type. Change ImageSource to BitmapSource in order to use our own converter.
            var targetType = info.TargetPropertyType;

            if (targetType.Equals(typeof(System.Windows.Media.ImageSource)))
                targetType = typeof(BitmapSource);

            // In case of a list target, get the correct list element type.
            if ((info.TargetPropertyIndex != -1) && typeof(IList).IsAssignableFrom(info.TargetPropertyType))
                targetType = info.TargetPropertyType.GetGenericArguments()[0];
            
            // Try to get the localized input from the resource.
            var resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(this.Key, targetObject);
            var ci = GetForcedCultureOrDefault();

            // Extract the names of the endpoint object and property
            var epProp = GetPropertyName(endPoint.TargetProperty);
            var epName = "";
            
            if (endPoint.TargetObject is FrameworkElement)
                epName = ((FrameworkElement)endPoint.TargetObject).GetValueSync<string>(FrameworkElement.NameProperty);
#if SILVERLIGHT
#else
            else if (endPoint.TargetObject is FrameworkContentElement)
                epName = ((FrameworkContentElement)endPoint.TargetObject).GetValueSync<string>(FrameworkContentElement.NameProperty);
#endif

            var resKeyBase = ci.Name + ":" + targetType.Name + ":";
            var resKeyNameProp = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName + LocalizeDictionary.GetSeparation(targetObject) + epProp, targetObject);
            var resKeyName = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName, targetObject);
            
            // Check, if the key is already in our resource buffer.
            object input = null;
            var isDefaultConverter = this.Converter is DefaultConverter;

            if (!String.IsNullOrEmpty(resourceKey))
            {
                // We've got a resource key. Try to look it up or get it from the dictionary.
                if (isDefaultConverter && ResourceBuffer.ContainsKey(resKeyBase + resourceKey))
                    result = ResourceBuffer[resKeyBase + resourceKey];
                else
                {
                    input = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, targetObject, ci);
                    resKeyBase += resourceKey;
                }
            }
            else
            {
                // Try the automatic lookup function.
                // First, look for a resource entry named: [FrameworkElement name][Separator][Property name]
                if (isDefaultConverter && ResourceBuffer.ContainsKey(resKeyBase + resKeyNameProp))
                    result = ResourceBuffer[resKeyBase + resKeyNameProp];
                else
                {
                    // It was not stored in the buffer - try to retrieve it from the dictionary.
                    input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyNameProp, targetObject, ci);

                    if (input == null)
                    {
                        // Now, try to look for a resource entry named: [FrameworkElement name]
                        // Note - this has to be nested here, as it would take precedence over the first step in the buffer lookup step.
                        if (isDefaultConverter && ResourceBuffer.ContainsKey(resKeyBase + resKeyName))
                            result = ResourceBuffer[resKeyBase + resKeyName];
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

            // If no result was found, convert the input and add it to the buffer.
            if (result == null)
            {
                if (input != null)
                {
                    result = this.Converter.Convert(input, targetType, this.ConverterParameter, ci);
                    if (isDefaultConverter)
                        SafeAddItemToResourceBuffer(resKeyBase, result);
                }
                else
                {
                    if (LocalizeDictionary.Instance.OnNewMissingKeyEvent(this, key))
                        UpdateNewValue();

                    if (!string.IsNullOrEmpty(key) && (targetType == typeof(String) || targetType == typeof(object)))
                        result = "Key: " + key;
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
#if SILVERLIGHT
            var targetCulture = LocalizeDictionary.Instance.Culture;
#else
            var targetCulture = LocalizeDictionary.Instance.SpecificCulture;
#endif
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
#if SILVERLIGHT
            var targetCulture = LocalizeDictionary.Instance.Culture;
#else
            var targetCulture = LocalizeDictionary.Instance.SpecificCulture;
#endif
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
            var result = default(TValue);

            var resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(key, target);

            // Get the localized object from the dictionary
            var resKey = targetCulture.Name + ":" + typeof(TValue).Name + ":" + resourceKey;
            var isDefaultConverter = converter is DefaultConverter;

            if (isDefaultConverter && ResourceBuffer.ContainsKey(resKey))
                result = (TValue)ResourceBuffer[resKey];
            else
            {
                var localizedObject = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, target, targetCulture);

                if (localizedObject == null)
                    return result;

                if (converter == null)
                    converter = new DefaultConverter();

                var tmp = converter.Convert(localizedObject, typeof(TValue), converterParameter, targetCulture);

                if (tmp is TValue)
                {
                    result = (TValue)tmp;
                    if (isDefaultConverter)
                        SafeAddItemToResourceBuffer(resKey, result);
                }
            }

            return result;
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
            return this.ResolveLocalizedValue(out resolvedValue, this.GetForcedCultureOrDefault(), null);
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
            return this.ResolveLocalizedValue(out resolvedValue, this.GetForcedCultureOrDefault(), target);
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
            string resKey = targetCulture.Name + ":" + typeof(TValue).Name + ":" + resourceKey;
            var isDefaultConverter = this.Converter is DefaultConverter;

            if (isDefaultConverter && ResourceBuffer.ContainsKey(resKey))
            {
                resolvedValue = (TValue)ResourceBuffer[resKey];
            }
            else
            {
                object localizedObject = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, target, targetCulture);

                if (localizedObject == null)
                    return false;

                object result = this.Converter.Convert(localizedObject, typeof(TValue), this.ConverterParameter, targetCulture);
                
                if (result is TValue)
                {
                    resolvedValue = (TValue)result;
                    if (isDefaultConverter)
                        SafeAddItemToResourceBuffer(resKey, resolvedValue);
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
            return SetBinding((object)targetObject, targetProperty, -1);
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
#if SILVERLIGHT
                // Dirty reflection hack - get the property type (property not included in the SL DependencyProperty class) from the internal declared field.
                targetPropertyType = typeof(DependencyProperty).GetField("_propertyType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(targetProperty) as Type;
#else
                targetPropertyType = ((DependencyProperty)targetProperty).PropertyType;
#endif
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
            return "Loc:" + key;
        }
    }
}