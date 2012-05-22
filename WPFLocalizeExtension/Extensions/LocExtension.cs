namespace WPFLocalizeExtension.Extensions
{
    #region Uses
    using System;
    using System.Windows.Markup;
    using System.Windows;
    using System.Reflection;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using XAMLMarkupExtensions.Base;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Windows.Data;
    using WPFLocalizeExtension.Engine;
    using System.Collections;
    #endregion

    /// <summary>
    /// A generic localization extension.
    /// Based on <see cref="https://github.com/MrCircuit/WPFLocalizationExtension"/>
    /// </summary>
    [ContentProperty("ResourceIdentifierKey")]
    public class LocExtension : NestedMarkupExtension, IWeakEventListener, INotifyPropertyChanged
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
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion

        #region Variables
        private static Dictionary<Type, TypeConverter> TypeConverters = new Dictionary<Type, TypeConverter>();
        private static Dictionary<string, object> ResourceBuffer = new Dictionary<string, object>();

        /// <summary>
        /// Holds the name of the Assembly where the .resx is located
        /// </summary>
        private string assembly;

        /// <summary>
        /// Holds the Name of the .resx dictionary.
        /// If it's null, "Resources" will get returned
        /// </summary>
        private string dict;

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
        /// Gets or sets the Key to a .resx object
        /// </summary>
        public string Key
        {
            get { return this.key; }
            set
            {
                string oldKey = this.key;
                this.key = value;

                if (oldKey != this.key)
                    UpdateNewValue();
            }
        }

        /// <summary>
        /// Gets or sets the name of the Assembly where the .resx is located.
        /// </summary>
        public string Assembly
        {
            get { return this.assembly; }
            set { this.assembly = !string.IsNullOrEmpty(value) ? value : null; }
        }

        /// <summary>
        /// Gets or sets the name of the Dict where the .resx is located.
        /// </summary>
        public string Dict
        {
            get { return this.dict; }
            set { this.dict = !string.IsNullOrEmpty(value) ? value : null; }
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
            get { return string.Format("{0}:{1}:{2}", this.Assembly, this.Dict, this.Key ?? "(null)"); }
            set { ParseKey(value, out this.assembly, out this.dict, out this.key); }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        public LocExtension()
        {
            // Register this extension as an event listener on the first target.
            base.OnFirstTarget = () =>
            {
                LocalizeDictionary.Instance.AddEventListener(this);
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public LocExtension(string key)
            : this()
        {
            // parse the key value and split it up if necessary
            ParseKey(key, out this.assembly, out this.dict, out this.key);
        } 
        #endregion

        #region IWeakEventListener implementation
        /// <summary>
        /// This method will be called through the interface, passed to the
        /// <see cref="LocalizeDictionary"/>.<see cref="LocalizeDictionary.WeakLocChangedEventManager"/> to get notified on culture changed
        /// </summary>
        /// <param name="managerType">The manager Type.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event argument.</param>
        /// <returns>
        /// True if the listener handled the event. It is considered an error by the <see cref="T:System.Windows.WeakEventManager"/> handling in WPF to register a listener for an event that the listener does not handle. Regardless, the method should return false if it receives an event that it does not recognize or handle.
        /// </returns>
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            // if the passed handler is type of LocalizeDictionary.WeakLocChangedEventManager, handle it
            if (managerType == typeof(LocalizeDictionary.WeakLocChangedEventManager))
            {
                // Update, if this object is in our endpoint list.
                if ((sender == null) || IsEndpointObject(sender))
                    UpdateNewValue();

                // return true, to notify the event was processed
                return true;
            }

            // return false, to notify the event was not processed
            return false;
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
                    cultureInfo = CultureInfo.CreateSpecificCulture(this.ForceCulture);
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
                        throw new ArgumentException("Cannot create a CultureInfo with '" + this.ForceCulture + "'", ex);
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

        #region Assembly/Dictionary/Key
        /// <summary>
        /// Returns the resource assembly name that may be subject to the information stored in the endpoint.
        /// </summary>
        /// <param name="endPoint">Information about the endpoint.</param>
        /// <returns>The name of the resource assembly.</returns>
        public string GetAssembly(TargetInfo endPoint)
        {
            string ret = null;

            if (assembly != null)
                ret = assembly;
            else if ((endPoint != null) && endPoint.IsDependencyObject)
                ret = LocalizeDictionary.GetDefaultAssembly((DependencyObject)endPoint.TargetObject);

            return ret;
        }

        /// <summary>
        /// Returns the resource dictionary name that may be subject to the information stored in the endpoint.
        /// </summary>
        /// <param name="endPoint">Information about the endpoint.</param>
        /// <returns>The name of the resource dictionary.</returns>
        public string GetDict(TargetInfo endPoint)
        {
            string ret = null;

            if (dict != null)
                ret = dict;
            else if ((endPoint != null) && endPoint.IsDependencyObject)
                ret = LocalizeDictionary.GetDefaultDictionary((DependencyObject)endPoint.TargetObject);

            return ret;
        }

        /// <summary>
        /// Parses a key ([[Assembly:]Dict:]Key and return the parts of it.
        /// </summary>
        /// <param name="inKey">The key to parse.</param>
        /// <param name="outAssembly">The found or default assembly.</param>
        /// <param name="outDict">The found or default dictionary.</param>
        /// <param name="outKey">The found or default key.</param>
        private void ParseKey(string inKey, out string outAssembly, out string outDict, out string outKey)
        {
            // reset the vars to null
            outAssembly = null;
            outDict = null;
            outKey = null;

            // its a assembly/dict/key pair
            if (!string.IsNullOrEmpty(inKey))
            {
                string[] split = inKey.Trim().Split(":".ToCharArray(), 3);

                // assembly:dict:key
                if (split.Length == 3)
                {
                    outAssembly = !string.IsNullOrEmpty(split[0]) ? split[0] : null;
                    outDict = !string.IsNullOrEmpty(split[1]) ? split[1] : null;
                    outKey = split[2];
                }

                // dict:key
                // assembly = ExecutingAssembly
                if (split.Length == 2)
                {
                    outDict = !string.IsNullOrEmpty(split[0]) ? split[0] : null;
                    outKey = split[1];
                }

                // key
                // assembly = ExecutingAssembly
                // dict = standard resourcedictionary
                if (split.Length == 1)
                {
                    outKey = split[0];
                }
            }
            else
            {
                // if the passed value is null pr empty, throw an exception if in runtime
                if (!LocalizeDictionary.Instance.GetIsInDesignMode())
                {
                    throw new ArgumentNullException("inKey");
                }
            }
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

            // Get target type. Change ImageSource to BitmapSource in order to use our own converter.
            Type targetType = info.TargetPropertyType;

            if (targetType.Equals(typeof(System.Windows.Media.ImageSource)))
                targetType = typeof(BitmapSource);

            // In case of a list target, get the correct list element type.
            if ((info.TargetPropertyIndex != -1) && typeof(IList).IsAssignableFrom(info.TargetPropertyType))
                targetType = info.TargetPropertyType.GetGenericArguments()[0];
            
            // Try to get the localized input from the resource.
            string resourceAssembly = GetAssembly(endPoint);
            string resourceDictionary = GetDict(endPoint);
            string resourceKey = this.Key;
            CultureInfo ci = GetForcedCultureOrDefault();

            // Extract the names of the endpoint object and property
            string epName = "";
            string epProp = "";

            if (endPoint.TargetObject is FrameworkElement)
                epName = (string)((FrameworkElement)endPoint.TargetObject).GetValue(FrameworkElement.NameProperty);
            else if (endPoint.TargetObject is FrameworkContentElement)
                epName = (string)((FrameworkContentElement)endPoint.TargetObject).GetValue(FrameworkContentElement.NameProperty);

            if (endPoint.TargetProperty is PropertyInfo)
                epProp = ((PropertyInfo)endPoint.TargetProperty).Name;
            else if (endPoint.TargetProperty is DependencyProperty)
                epProp = ((DependencyProperty)endPoint.TargetProperty).Name;

            // TODO: What are these names during design time good for? Any suggestions?
            if (epProp.Contains("FrameworkElementWidth5"))
                epProp = "Height";
            else if (epProp.Contains("FrameworkElementWidth6"))
                epProp = "Width";
            else if (epProp.Contains("FrameworkElementMargin12"))
                epProp = "Margin";

            string resKeyBase = ci.Name + ":" + targetType.Name + ":" + resourceAssembly + ":" + resourceDictionary + ":";
            string resKeyNameProp = epName + LocalizeDictionary.Instance.Separation + epProp;
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
                object input = LocalizeDictionary.Instance.GetLocalizedObject(resourceAssembly, resourceDictionary, resourceKey, ci);

                if (input == null)
                {
                    // Try get the key + Name of the DependencyObject [Separator] Property name
                    input = LocalizeDictionary.Instance.GetLocalizedObject(resourceAssembly, resourceDictionary, resKeyNameProp, ci);

                    if (input == null)
                    {
                        // Try get the key + just the Name of the DependencyObject
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resourceAssembly, resourceDictionary, resKeyName, ci);

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
            object result = null;
            Type resourceType = input.GetType();

            // Simplest cases: The target type is object or same as the input.
            if (targetType.Equals(typeof(System.Object)) || resourceType.Equals(targetType))
                return input;

            // Check, if a converter was supplied by the user.
            if (converter != null)
                return converter.Convert(input, targetType, converterParameter, ci);

            // Register missing type converters - this class will do this only once per appdomain.
            RegisterMissingTypeConverters.Register();

            // Is the type already known?
            if (!TypeConverters.ContainsKey(targetType))
            {
                // Get the type converter and store it in the dictionary (even if it is NULL).
                TypeConverters.Add(targetType, TypeDescriptor.GetConverter(targetType));
            }

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

        #region Resolve functions
        // TODO: Add resolve functions that support retrieval of assembly and dictionary from a given target

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        /// <exception>
        /// If the Assembly, Dict, Key pair was not found.
        /// </exception>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue)
        {
            // return the resolved localized value with the current or forced culture.
            return this.ResolveLocalizedValue(out resolvedValue, this.GetForcedCultureOrDefault());
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
        /// <exception>
        /// If the Assembly, Dict, Key pair was not found.
        /// </exception>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue, CultureInfo targetCulture)
        {
            // define the default value of the resolved value
            resolvedValue = default(TValue);

            // get the localized object from the dictionary
            string resKey = targetCulture.Name + ":" + typeof(TValue).Name + ":" + this.Assembly + ":" + this.Dict + ":" + this.Key;

            if (ResourceBuffer.ContainsKey(resKey))
            {
                resolvedValue = (TValue)ResourceBuffer[resKey];
            }
            else
            {
                object localizedObject = LocalizeDictionary.Instance.GetLocalizedObject(this.Assembly, this.Dict, this.Key, targetCulture);
                object result = ConvertAndBufferResult(localizedObject, typeof(TValue), targetCulture, resKey);
                if (result is TValue)
                    resolvedValue = (TValue)result;
            }

            if (resolvedValue != null)
                return true;

            // return false: resulve was not successfully.
            return false;
        }
        #endregion

        #region Code-behind binding
        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target dependency object</param>
        /// <param name="targetProperty">The target dependency property</param>
        /// <returns>
        /// TRUE if the binding was setup successfully, otherwise FALSE (Binding already exists).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="targetProperty"/> is
        /// not a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
        /// </exception>
        public bool SetBinding(DependencyObject targetObject, object targetProperty)
        {
            return SetBinding(targetObject, targetProperty, -1);
        }

        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target dependency object</param>
        /// <param name="targetProperty">The target dependency property</param>
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
    }
}