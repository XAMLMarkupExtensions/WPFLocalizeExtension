#region Copyright information
// <copyright file="BindingLocExtension.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

namespace WPFLocalizeExtension.Extensions
{
    using XAMLMarkupExtensions.Base;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.ComponentModel;
    using System;
    using System.Windows;
    using System.Reflection;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using WPFLocalizeExtension.Engine;
    using WPFLocalizeExtension.TypeConverters;
    using System.Globalization;
    using System.Windows.Media.Imaging;
    using System.Collections;

    /// <summary>
    /// A loc extension that returns a binding instead of a value.
    /// </summary>
    public class BindingLocExtension : MarkupExtension, INotifyPropertyChanged, IDictionaryEventListener
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
        internal void RaisePropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region Variables
        private static Dictionary<Type, TypeConverter> TypeConverters = new Dictionary<Type, TypeConverter>();
        private static Dictionary<string, object> ResourceBuffer = new Dictionary<string, object>();
        private object value;
        private DependencyObject targetDependencyObject = null;
        private DependencyProperty targetDependencyProperty = null;

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
        #endregion

        #region Properties
        /// <summary>
        /// The value, the internal binding is pointing at.
        /// </summary>
        public object Value
        {
            get { return value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    RaisePropertyChanged("Value");
                }
            }
        }

        /// <summary>
        /// Gets or sets the Key to a .resx object
        /// </summary>
        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                if (key != value)
                {
                    key = value;
                    UpdateNewValue();
                    RaisePropertyChanged("Key");
                }
            }
        }

        /// <summary>
        /// Gets or sets the custom value converter.
        /// </summary>
        public IValueConverter Converter
        {
            get { return converter; }
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
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        public BindingLocExtension()
            : base()
        {
            LocalizeDictionary.DictionaryEvent.AddListener(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public BindingLocExtension(string key)
            : this()
        {
            this.Key = key;
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
            if (sender == null || sender == targetDependencyObject)
                UpdateNewValue();
        } 
        #endregion

        private void UpdateNewValue()
        {
            if (targetDependencyObject == null || targetDependencyProperty == null)
                return;

            var info = new TargetInfo(targetDependencyObject, targetDependencyProperty, targetDependencyProperty.PropertyType, -1);
            this.Value = FormatOutput(info, info);
        }

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
        public object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            object result = null;

            if (endPoint == null)
                return null;

            var targetObject = endPoint.TargetObject as DependencyObject;

            // Get target type. Change ImageSource to BitmapSource in order to use our own converter.
            Type targetType = info.TargetPropertyType;

            if (targetType.Equals(typeof(System.Windows.Media.ImageSource)))
                targetType = typeof(BitmapSource);

            // In case of a list target, get the correct list element type.
            if ((info.TargetPropertyIndex != -1) && typeof(IList).IsAssignableFrom(info.TargetPropertyType))
                targetType = info.TargetPropertyType.GetGenericArguments()[0];

            // Try to get the localized input from the resource.
            string resourceKey = this.Key;

            CultureInfo ci = GetForcedCultureOrDefault();

            // Extract the names of the endpoint object and property
            string epName = "";
            string epProp = "";

            if (endPoint.TargetObject is FrameworkElement)
                epName = (string)((FrameworkElement)endPoint.TargetObject).GetValue(FrameworkElement.NameProperty);
#if SILVERLIGHT
#else
            else if (endPoint.TargetObject is FrameworkContentElement)
                epName = (string)((FrameworkContentElement)endPoint.TargetObject).GetValue(FrameworkContentElement.NameProperty);
#endif

            if (endPoint.TargetProperty is PropertyInfo)
                epProp = ((PropertyInfo)endPoint.TargetProperty).Name;
#if SILVERLIGHT
            else if (endPoint.TargetProperty is DependencyProperty)
                epProp = ((DependencyProperty)endPoint.TargetProperty).ToString();
#else
            else if (endPoint.TargetProperty is DependencyProperty)
                epProp = ((DependencyProperty)endPoint.TargetProperty).Name;
#endif

            // What are these names during design time good for? Any suggestions?
            if (epProp.Contains("FrameworkElementWidth5"))
                epProp = "Height";
            else if (epProp.Contains("FrameworkElementWidth6"))
                epProp = "Width";
            else if (epProp.Contains("FrameworkElementMargin12"))
                epProp = "Margin";

            string resKeyBase = ci.Name + ":" + targetType.Name + ":";
            string resKeyNameProp = epName + LocalizeDictionary.GetSeparation(targetObject) + epProp;
            string resKeyName = epName;

            // Check, if the key is already in our resource buffer.
            if (ResourceBuffer.ContainsKey(resKeyBase + resourceKey))
                result = ResourceBuffer[resKeyBase + resourceKey];
            else if (ResourceBuffer.ContainsKey(resKeyBase + resKeyNameProp))
                result = ResourceBuffer[resKeyBase + resKeyNameProp];
            else if (ResourceBuffer.ContainsKey(resKeyBase + resKeyName))
                result = ResourceBuffer[resKeyBase + resKeyName];
            else
            {
                object input = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, targetObject, ci);

                if (input == null)
                {
                    // Try get the key + Name of the DependencyObject [Separator] Property name
                    input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyNameProp, targetObject, ci);

                    if (input == null)
                    {
                        // Try get the key + just the Name of the DependencyObject
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyName, targetObject, ci);

                        if (input == null)
                            return null;

                        resKeyBase += resKeyName;
                    }
                    else
                        resKeyBase += resKeyNameProp;
                }
                else
                    resKeyBase += resourceKey;

                result = ConvertAndBufferResult(input, targetType, ci, resKeyBase);
            }

            return result;
        }
        #endregion

        #region Value conversion and buffering
        /// <summary>
        /// Converts the input according to the given target type and stores it in the resource buffer under the given resKey.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="ci">The culture info, if used with a TypeConverter.</param>
        /// <param name="resKey">The key for the resource buffer.</param>
        /// <returns></returns>
        private object ConvertAndBufferResult(object input, Type targetType, CultureInfo ci, string resKey)
        {
            if (input == null)
                return null;

            object result = null;
            Type resourceType = input.GetType();

            // Simplest cases: The target type is object or same as the input.
            if (targetType.Equals(typeof(System.Object)) || resourceType.Equals(targetType))
                return input;

            // Check, if a converter was supplied by the user.
            if (converter != null)
                return converter.Convert(input, targetType, converterParameter, ci);

#if SILVERLIGHT
            // Is the type already known?
            if (!TypeConverters.ContainsKey(targetType))
            {
                if (typeof(Enum).IsAssignableFrom(targetType))
                {
                    TypeConverters.Add(targetType, new EnumConverter(targetType));
                }
                else
                {
                    Type converterType = null;
                    var attributes = targetType.GetCustomAttributes(typeof(TypeConverterAttribute), false);

                    if (attributes.Length == 1)
                    {
                        var converterAttribute = (TypeConverterAttribute)attributes[0];
                        converterType = Type.GetType(converterAttribute.ConverterTypeName);
                    }

                    if (converterType == null)
                    {
                        // Find a suitable "common" converter.
                        if (targetType == typeof(double))
                            converterType = typeof(DoubleConverter);
                        else if (targetType == typeof(Thickness))
                            converterType = typeof(ThicknessConverter);
                        else if (targetType == typeof(Brush))
                            converterType = typeof(BrushConverter);
                        else
                            return input;
                    }

                    // Get the type converter and store it in the dictionary (even if it is NULL).
                    TypeConverters.Add(targetType, Activator.CreateInstance(converterType) as TypeConverter);
                }
            }
#else
            // Register missing type converters - this class will do this only once per appdomain.
            RegisterMissingTypeConverters.Register();

            // Is the type already known?
            if (!TypeConverters.ContainsKey(targetType))
            {
                var c = TypeDescriptor.GetConverter(targetType);

                if (targetType == typeof(Thickness))
                    c = new WPFLocalizeExtension.TypeConverters.ThicknessConverter();

                // Get the type converter and store it in the dictionary (even if it is NULL).
                TypeConverters.Add(targetType, c);
            }
#endif

            // Get the converter.
            TypeConverter conv = TypeConverters[targetType];

            // No converter or not convertable?
            if ((conv == null) || !conv.CanConvertFrom(resourceType))
                return null;

            // Finally, try to convert the value.
            try
            {
                result = conv.ConvertFrom(input);
            }
            catch
            {
                result = Activator.CreateInstance(targetType);
            }

            ResourceBuffer.Add(resKey, result);

            return result;
        }
        #endregion

        #region MarkupExtension implementation
        /// <summary>
        /// Returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>The object value to set on the property where the extension is applied.</returns>
        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                if (service == null)
                    return this;

                object targetObject = service.TargetObject;
                if (targetObject == null)
                    return this;

                object targetProperty = service.TargetProperty;
                if (targetProperty == null)
                    return this;

                Type c = targetObject.GetType();

                targetDependencyProperty = targetProperty as DependencyProperty;
                if (targetDependencyProperty != null)
                    targetDependencyObject = targetObject as DependencyObject;
                else
                {
                    PropertyInfo info = targetProperty as PropertyInfo;
                    if (info == null)
                    {
                        if (!typeof(Collection<BindingBase>).IsAssignableFrom(targetProperty.GetType()))
                            throw new InvalidOperationException("No valid target provided (Check 1).");
                    }
                    else
                    {
                        Type propertyType = info.PropertyType;

                        if (!typeof(MarkupExtension).IsAssignableFrom(propertyType) || !propertyType.IsAssignableFrom(this.GetType()))
                            throw new InvalidOperationException("No valid target provided (Check 2).");
                    }
                }

                if ((targetDependencyObject != null) && (targetDependencyProperty != null))
                {
                    UpdateNewValue();
                    var binding = new Binding("Value");
                    binding.Source = this;
                    
                    return binding.ProvideValue(serviceProvider);
                }
            }

            return this;
        } 
        #endregion
    }
}
