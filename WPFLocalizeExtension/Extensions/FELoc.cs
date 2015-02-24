#region Copyright information
// <copyright file="FELoc.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

namespace WPFLocalizeExtension.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup.Primitives;
    using System.Windows.Media.Imaging;
    using Engine;
    using TypeConverters;
    using XAMLMarkupExtensions.Base;
    
    /// <summary>
    /// A localization utility based on <see cref="FrameworkElement"/>.
    /// </summary>
    public class FELoc : FrameworkElement, IDictionaryEventListener, INotifyPropertyChanged, IDisposable
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

        #region Private variables
        private static object resourceBufferLock = new object();
        private static Dictionary<string, object> ResourceBuffer = new Dictionary<string, object>();
        
        private ParentChangedNotifier parentChangedNotifier = null;
        private TargetInfo targetInfo = null;
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

        #region DependencyProperty: Key
        /// <summary>
        /// <see cref="DependencyProperty"/> Key to set the resource key.
        /// </summary>
        public static readonly DependencyProperty KeyProperty =
                DependencyProperty.Register(
                "Key",
                typeof(string),
                typeof(FELoc),
                new PropertyMetadata(null, DependencyPropertyChanged));

        /// <summary>
        /// The resource key.
        /// </summary>
        public string Key
        {
            get { return this.GetValueSync<string>(KeyProperty); }
            set { this.SetValueSync(KeyProperty, value); }
        }
        #endregion

        #region DependencyProperty: Converter
        /// <summary>
        /// <see cref="DependencyProperty"/> Converter to set the <see cref="IValueConverter"/> used to adapt to the target.
        /// </summary>
        public static readonly DependencyProperty ConverterProperty =
                DependencyProperty.Register(
                "Converter",
                typeof(IValueConverter),
                typeof(FELoc),
                new PropertyMetadata(new DefaultConverter(), DependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the custom value converter.
        /// </summary>
        public IValueConverter Converter
        {
            get { return this.GetValueSync<IValueConverter>(ConverterProperty); }
            set { this.SetValueSync(ConverterProperty, value); }
        } 
        #endregion

        #region DependencyProperty: ConverterParameter
        /// <summary>
        /// <see cref="DependencyProperty"/> ConverterParameter.
        /// </summary>
        public static readonly DependencyProperty ConverterParameterProperty =
                DependencyProperty.Register(
                "ConverterParameter",
                typeof(object),
                typeof(FELoc),
                new PropertyMetadata(null, DependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the converter parameter.
        /// </summary>
        public object ConverterParameter
        {
            get { return this.GetValueSync<object>(ConverterParameterProperty); }
            set { this.SetValueSync(ConverterParameterProperty, value); }
        }
        #endregion

        #region DependencyProperty: ForceCulture
        /// <summary>
        /// <see cref="DependencyProperty"/> ForceCulture.
        /// </summary>
        public static readonly DependencyProperty ForceCultureProperty =
                DependencyProperty.Register(
                "ForceCulture",
                typeof(string),
                typeof(FELoc),
                new PropertyMetadata(null, DependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the forced culture.
        /// </summary>
        public string ForceCulture
        {
            get { return this.GetValueSync<string>(ForceCultureProperty); }
            set { this.SetValueSync(ForceCultureProperty, value); }
        }
        #endregion

        #region DependencyProperty: Content - used for value transfer only!
        ///// <summary>
        ///// <see cref="DependencyProperty"/> ForceCulture.
        ///// </summary>
        //public static readonly DependencyProperty ContentProperty =
        //        DependencyProperty.Register(
        //        "Content",
        //        typeof(object),
        //        typeof(FELoc));

        ///// <summary>
        ///// Gets or sets the content.
        ///// </summary>
        //public object Content
        //{
        //    get { return GetValue(ContentProperty); }
        //    set { SetValue(ContentProperty, value); }
        //}

        private object content;
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public object Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged("Content"); }
        }
        #endregion

        /// <summary>
        /// Indicates, that the key changed.
        /// </summary>
        /// <param name="obj">The FELoc object.</param>
        /// <param name="args">The event argument.</param>
        private static void DependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var loc = obj as FELoc;

            if (loc != null)
                loc.UpdateNewValue();
        }

        #region Parent changed event
        private IList<DependencyProperty> GetAttachedProperties(DependencyObject obj)
        {
            List<DependencyProperty> attached = new List<DependencyProperty>();

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(obj,
                new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) }))
            {
                DependencyPropertyDescriptor dpd =
                    DependencyPropertyDescriptor.FromProperty(pd);

                if (dpd != null && dpd.IsAttached)
                {
                    attached.Add(dpd.DependencyProperty);
                }
            }

            return attached;
        }

        /// <summary>
        /// Based on http://social.msdn.microsoft.com/Forums/en/wpf/thread/580234cb-e870-4af1-9a91-3e3ba118c89c
        /// </summary>
        /// <param name="element">The target object.</param>
        /// <returns>The list of DependencyProperties of the object.</returns>
        private List<DependencyProperty> GetDependencyProperties(Object element)
        {
            List<DependencyProperty> properties = new List<DependencyProperty>();
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);

            if (markupObject != null)
                foreach (MarkupProperty mp in markupObject.Properties)
                    if (mp.DependencyProperty != null)
                        properties.Add(mp.DependencyProperty);

            return properties;
        }

        private void RegisterParentNotifier()
        {
            parentChangedNotifier = new ParentChangedNotifier(this, () =>
            {
                parentChangedNotifier.Dispose();
                parentChangedNotifier = null;
                var targetObject = this.Parent;
                if (targetObject != null)
                {
                    var properties = GetDependencyProperties(targetObject);
                    foreach (var p in properties)
                    {
                        if (targetObject.GetValue(p) == this)
                        {
                            targetInfo = new TargetInfo(targetObject, p, p.PropertyType, -1);

                            Binding binding = new Binding("Content");
                            binding.Source = this;
                            binding.Converter = this.Converter;
                            binding.ConverterParameter = this.ConverterParameter;
                            binding.Mode = BindingMode.OneWay;
                            BindingOperations.SetBinding(targetObject, p, binding);
                            UpdateNewValue();
                        }
                    }
                }
            });
        } 
        #endregion

        #region Constructors & Dispose
        /// <summary>
        /// Initializes a new instance of the <see cref="BLoc"/> class.
        /// </summary>
        public FELoc()
            : base()
        {
            LocalizeDictionary.DictionaryEvent.AddListener(this);
            RegisterParentNotifier();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BLoc"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public FELoc(string key)
            : this()
        {
            this.Key = key;
        }

        /// <summary>
        /// Removes the listener from the dictionary.
        /// </summary>
        public void Dispose()
        {
            LocalizeDictionary.DictionaryEvent.RemoveListener(this);
        }

        /// <summary>
        /// The finalizer.
        /// </summary>
        ~FELoc()
        {
            Dispose();
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

        /// <summary>
        /// This method is called when the resource somehow changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        public void ResourceChanged(DependencyObject sender, DictionaryEventArgs e)
        {
            UpdateNewValue();
        }

        private void UpdateNewValue()
        {
            this.Content = FormatOutput();
        }

        #region Resource loopkup
        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        public object FormatOutput()
        {
            object result = null;

            if (targetInfo == null)
                return null;

            var targetObject = targetInfo.TargetObject as DependencyObject;

            // Get target type. Change ImageSource to BitmapSource in order to use our own converter.
            Type targetType = targetInfo.TargetPropertyType;

            if (targetType.Equals(typeof(System.Windows.Media.ImageSource)))
                targetType = typeof(BitmapSource);

            // Try to get the localized input from the resource.
            string resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(Key, targetObject);

            CultureInfo ci = GetForcedCultureOrDefault();

            // Extract the names of the endpoint object and property
            string epName = "";
            string epProp = "";

            if (targetObject is FrameworkElement)
                epName = ((FrameworkElement)targetObject).GetValueSync<string>(FrameworkElement.NameProperty);
#if SILVERLIGHT
#else
            else if (targetObject is FrameworkContentElement)
                epName = ((FrameworkContentElement)targetObject).GetValueSync<string>(FrameworkContentElement.NameProperty);
#endif

            if (targetInfo.TargetProperty is PropertyInfo)
                epProp = ((PropertyInfo)targetInfo.TargetProperty).Name;
#if SILVERLIGHT
            else if (targetInfo.TargetProperty is DependencyProperty)
                epProp = ((DependencyProperty)targetInfo.TargetProperty).ToString();
#else
            else if (targetInfo.TargetProperty is DependencyProperty)
                epProp = ((DependencyProperty)targetInfo.TargetProperty).Name;
#endif

            // What are these names during design time good for? Any suggestions?
            if (epProp.Contains("FrameworkElementWidth5"))
                epProp = "Height";
            else if (epProp.Contains("FrameworkElementWidth6"))
                epProp = "Width";
            else if (epProp.Contains("FrameworkElementMargin12"))
                epProp = "Margin";

            string resKeyBase = ci.Name + ":" + targetType.Name + ":";
            string resKeyNameProp = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName + LocalizeDictionary.GetSeparation(targetObject) + epProp, targetObject);
            string resKeyName = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName, targetObject);

            // Check, if the key is already in our resource buffer.
            object input = null;

            if (!String.IsNullOrEmpty(resourceKey))
            {
                // We've got a resource key. Try to look it up or get it from the dictionary.
                if (ResourceBuffer.ContainsKey(resKeyBase + resourceKey))
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
                if (ResourceBuffer.ContainsKey(resKeyBase + resKeyNameProp))
                    result = ResourceBuffer[resKeyBase + resKeyNameProp];
                else
                {
                    // It was not stored in the buffer - try to retrieve it from the dictionary.
                    input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyNameProp, targetObject, ci);

                    if (input == null)
                    {
                        // Now, try to look for a resource entry named: [FrameworkElement name]
                        // Note - this has to be nested here, as it would take precedence over the first step in the buffer lookup step.
                        if (ResourceBuffer.ContainsKey(resKeyBase + resKeyName))
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
            if (result == null && input != null)
            {
                result = this.Converter.Convert(input, targetType, this.ConverterParameter, ci);
                SafeAddItemToResourceBuffer(resKeyBase, result);
            }

            return result;
        }
        #endregion
    }
}
