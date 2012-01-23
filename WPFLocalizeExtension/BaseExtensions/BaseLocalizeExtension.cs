using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using WPFLocalizeExtension.Engine;
using System.Linq;

namespace WPFLocalizeExtension.BaseExtensions
{
    /// <summary>
    /// Implements the BaseLocalizeExtension.
    /// Represents a LocalizationExtension which provides a localized object of a .resx dictionary.
    /// </summary>
    /// <typeparam name="TValue">The type of the provided value.</typeparam>
    /// <remarks>
    /// If a content between two tags in xaml is set, this has the higher priority and will overwrite the settled properties
    /// </remarks>
    [MarkupExtensionReturnType(typeof(object))]
    [ContentProperty("ResourceIdentifierKey")]
    public abstract class BaseLocalizeExtension<TValue> : MarkupExtension, IWeakEventListener, INotifyPropertyChanged
    {
        /// <summary>
        /// Holds the collection of assigned dependency objects as WeakReferences
        /// </summary>
        private readonly Dictionary<WeakReference, object> targetObjects;

        /// <summary>
        /// Holds the name of the Assembly where the .resx is located
        /// </summary>
        private string assembly;

        /// <summary>
        /// The current value
        /// </summary>
        private TValue currentValue;

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
        /// Initializes a new instance of the BaseLocalizeExtension class.
        /// </summary>
        protected BaseLocalizeExtension()
        {
            // initialize the collection of the assigned dependency objects
            this.targetObjects = new Dictionary<WeakReference, object>();
        }

        /// <summary>
        /// Initializes a new instance of the BaseLocalizeExtension class.
        /// </summary>
        /// <param name="key">Three types are supported:
        /// Direct: passed key = key;
        /// Dict/Key pair: this have to be separated like ResXDictionaryName:ResourceKey
        /// Assembly/Dict/Key pair: this have to be separated like ResXDictionaryName:ResourceKey</param>
        /// <remarks>
        /// This constructor register the <see cref="EventHandler"/><c>OnCultureChanged</c> on <c>LocalizeDictionary</c>
        /// to get an acknowledge of changing the culture
        /// </remarks>
        protected BaseLocalizeExtension(string key)
            : this()
        {
            // parse the key value and split it up if necessary
            LocalizeDictionary.ParseKey(key, out this.assembly, out this.dict, out this.key);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private KeyValuePair<WeakReference, object> GetTargetKVP()
        {
            return (from o in targetObjects
                    where (o.Key.Target != null)
                    select o).FirstOrDefault();
        }

        protected DependencyObject GetTargetDependencyObject()
        {
            var target = GetTargetObject();

            if (target == null)
                return null;

            // If nested, return the target's dependency object, or its targets targets dep obj ...
            var t = target as BaseLocalizeExtension<TValue>;

            if (t != null)
                return t.GetTargetDependencyObject();

            // finally, try to cast into a DependencyObject
            return target as DependencyObject;
        }

        protected object GetTargetObject()
        {
            var key = GetTargetKVP().Key;

            if (key == null)
                return null;

            return key.Target;
        }

        protected object GetTargetProperty()
        {
            return GetTargetKVP().Value;
        }

        /// <summary>
        /// Gets or sets the name of the Assembly where the .resx is located.
        /// If it's null, the executing assembly (where this LocalizeEngine is located at) will get returned
        /// </summary>
        public string Assembly
        {
            get
            {
                // Get the last loaded default assembly - this is needed during design time and at startup.
                string defaultAssembly = null;

                // If there is a DependencyObject in the targets list, then try to get the inheritet value of the dependency property.
                if (targetObjects != null)
                {
                    var target = GetTargetDependencyObject();

                    while (target != null)
                    {
                        defaultAssembly = LocalizeDictionary.GetDefaultAssembly(target);

                        if (String.IsNullOrEmpty(defaultAssembly))
                            target = LogicalTreeHelper.GetParent(target);
                        else
                            target = null;
                    }
                }

                // Return the assembly that was parsed or the default assembly.
                return this.assembly ?? defaultAssembly;
            }

            set { this.assembly = !string.IsNullOrEmpty(value) ? value : null; }
        }

        /// <summary>
        /// Gets the current value.
        /// This property has only a value, if the <c>BaseLocalizeExtension</c> is bound to a target.
        /// </summary>
        /// <value>The current value.</value>
        public TValue CurrentValue
        {
            get { return this.currentValue; }

            private set
            {
                this.currentValue = value;
                this.RaiseNotifyPropertyChanged("CurrentValue");
            }
        }

        /// <summary>
        /// Gets or sets the design value.
        /// </summary>
        /// <value>The design value.</value>
        [DesignOnly(true)]
        public object DesignValue { get; set; }

        /// <summary>
        /// Gets or sets the Name of the .resx dictionary.
        /// If it's null, "Resources" will get returned
        /// </summary>
        public string Dict
        {
            get
            {
                // Get the last loaded default dictionary - this is needed during design time and at startup.
                string defaultDictionary = null;

                // If there is a DependencyObject in the targets list, then try to get the inheritet value of the dependency property.
                if (targetObjects != null)
                {
                    var target = GetTargetDependencyObject();

                    while (target != null)
                    {
                        defaultDictionary = LocalizeDictionary.GetDefaultDictionary(target);

                        if (String.IsNullOrEmpty(defaultDictionary))
                            target = LogicalTreeHelper.GetParent(target);
                        else
                            target = null;
                    }
                }

                // Return the dictionary that was parsed or the default assembly.
                return this.dict ?? defaultDictionary;
            }

            set { this.dict = !string.IsNullOrEmpty(value) ? value : null; }
        }

        /// <summary>
        /// Gets or sets the culture to force a fixed localized object
        /// </summary>
        public string ForceCulture { get; set; }

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
                    this.HandleNewValue();
            }
        }

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

            set { LocalizeDictionary.ParseKey(value, out this.assembly, out this.dict, out this.key); }
        }

        /// <summary>
        /// Gets the collection of <see cref="DependencyObject"/> as WeakReferences and the target property.
        /// </summary>
        public Dictionary<WeakReference, object> TargetObjects
        {
            get { return this.targetObjects; }
        }

        /// <summary>
        /// Provides the Value for the first Binding
        /// </summary>
        /// <param name="serviceProvider">The <see cref="System.Windows.Markup.IProvideValueTarget"/> provided from the <see cref="MarkupExtension"/></param>
        /// <returns>
        /// The found item from the .resx directory or null if not found
        /// </returns>
        /// <remarks>
        /// This method register the <see cref="EventHandler"/><c>OnCultureChanged</c> on <c>LocalizeDictionary</c>
        /// to get an acknowledge of changing the culture, if the passed <see cref="TargetObjects"/> type of <see cref="DependencyObject"/>.
        /// !PROOF: On every single <see cref="UserControl"/>, Window, and Page,
        /// there is a new SharedDP reference, and so there is every time a new <c>BaseLocalizeExtension</c>!
        /// Because of this, we don't need to notify every single DependencyObjects to update their value (for GC).
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// thrown if <paramref name="serviceProvider"/> is not type of <see cref="System.Windows.Markup.IProvideValueTarget"/>
        /// </exception>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // if the service provider is null, return this
            if (serviceProvider == null)
            {
                return this;
            }

            // try to cast the passed serviceProvider to a IProvideValueTarget
            IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            // if the cast fails, return this
            if (service == null)
            {
                return this;
            }

            // if the service.TargetObject is a Binding, throw an exception
            if (service.TargetObject is Binding)
            {
                throw new InvalidOperationException("Use as binding is not supported!");
            }

            // declare a target property
            object targetProperty = null;

            // check if the service.TargetProperty is a DependencyProperty or a PropertyInfo
            if (service.TargetProperty is DependencyProperty ||
                service.TargetProperty is PropertyInfo)
            {
                // set the target property to the service.TargetProperty
                targetProperty = service.TargetProperty;
            }

            // check if the target property is null
            if (targetProperty == null)
            {
                // return this.
                return this;
            }

            // if the service.TargetObject is System.Windows.SharedDp (= not a DependencyObject), we return "this".
            // the SharedDp will call this instance later again.
            if (!(service.TargetObject is DependencyObject) && !(service.TargetProperty is PropertyInfo))
            {
                // by returning "this", the provide value will be called later again.
                return this;
            }

            // indicates, if the target object was found
            bool foundInWeakReferences = false;

            // search for the target in the target object list
            foreach (KeyValuePair<WeakReference, object> wr in this.targetObjects)
            {
                // if the target is the target of the weakreference
                if (wr.Key.Target == service.TargetObject && wr.Value == service.TargetProperty)
                {
                    // set the flag to true and stop searching
                    foundInWeakReferences = true;
                    break;
                }
            }

            // if the target is a dependency object and it's not collected already, collect it
            //if (service.TargetObject is DependencyObject && !foundInWeakReferences)
            if (!foundInWeakReferences)
            {
                // if it's the first object, add an event handler too
                if (this.targetObjects.Count == 0)
                {
                    // add this localize extension to the WeakEventManager on LocalizeDictionary
                    LocalizeDictionary.Instance.AddEventListener(this);
                }

                // add the target as an dependency object as weakreference to the dependency object list
                this.targetObjects.Add(new WeakReference(service.TargetObject), service.TargetProperty);

                // adds this localize extension to the ObjectDependencyManager to ensure the lifetime along with the targetobject
                ObjectDependencyManager.AddObjectDependency(new WeakReference(service.TargetObject), this);
            }

            // return the new value for the DependencyProperty
            return LocalizeDictionary.Instance.GetLocalizedObject<object>(
                this.Assembly, this.Dict, this.Key, this.GetForcedCultureOrDefault());
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        /// <exception>
        /// If the Assembly, Dict, Key pair was not found.
        /// </exception>
        public bool ResolveLocalizedValue(out TValue resolvedValue)
        {
            // return the resolved localized value with the current or forced culture.
            return this.ResolveLocalizedValue(out resolvedValue, this.GetForcedCultureOrDefault());
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        /// <exception>
        /// If the Assembly, Dict, Key pair was not found.
        /// </exception>
        public bool ResolveLocalizedValue(out TValue resolvedValue, CultureInfo targetCulture)
        {
            // define the default value of the resolved value
            resolvedValue = default(TValue);

            // get the localized object from the dictionary
            object localizedObject = LocalizeDictionary.Instance.GetLocalizedObject<object>(
                this.Assembly, this.Dict, this.Key, targetCulture);

            // check if the found localized object can be converted with this extension
            if (this.CanProvideValue(localizedObject.GetType()))
            {
                // format the localized object
                object formattedOutput = this.FormatOutput(localizedObject);

                // check if the formatted output is not null
                if (formattedOutput != null)
                {
                    // set the content of the resolved value
                    resolvedValue = (TValue) formattedOutput;
                }

                // return true: resolve was successfully
                return true;
            }

            // return false: resulve was not successfully.
            return false;
        }

        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>BaseLocalizeExtension</c>.
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
            if (!(targetProperty is DependencyProperty ||
                  targetProperty is PropertyInfo))
            {
                throw new ArgumentException(
                    "The targetProperty should be a DependencyProperty or PropertyInfo!", "targetProperty");
            }

            // indicates, if the target object was found
            bool foundInWeakReferences = false;

            // search for the target in the target object list
            foreach (KeyValuePair<WeakReference, object> wr in this.targetObjects)
            {
                // if the target is the target of the weakreference
                if (wr.Key.Target == targetObject && wr.Value == targetProperty)
                {
                    // set the flag to true and stop searching
                    foundInWeakReferences = true;
                    break;
                }
            }

            // if the target it's not collected already, collect it
            if (!foundInWeakReferences)
            {
                // if it's the first object, add an event handler too
                if (this.targetObjects.Count == 0)
                {
                    // add this localize extension to the WeakEventManager on LocalizeDictionary
                    LocalizeDictionary.Instance.AddEventListener(this);
                }

                // add the target as an dependency object as weakreference to the dependency object list
                this.targetObjects.Add(new WeakReference(targetObject), targetProperty);

                // adds this localize extension to the ObjectDependencyManager to ensure the lifetime along with the targetobject
                ObjectDependencyManager.AddObjectDependency(new WeakReference(targetObject), this);

                // get the initial value of the dependency property
                object output = this.FormatOutput(
                    LocalizeDictionary.Instance.GetLocalizedObject<object>(
                        this.Assembly, 
                        this.Dict, 
                        this.Key, 
                        this.GetForcedCultureOrDefault()));

                // set the value to the dependency object
                this.SetTargetValue(
                    targetObject,
                    targetProperty,
                    output);

                // return true, the binding was successfully
                return true;
            }

            // return false, the binding already exists
            return false;
        }

        /// <summary>
        /// Returns the Key that identifies a resource (Assembly:Dictionary:Key)
        /// </summary>
        /// <returns>Format: Assembly:Dictionary:Key</returns>
        public override sealed string ToString()
        {
            return base.ToString() + " -> " + this.ResourceIdentifierKey;
        }

        /// <summary>
        /// This method will be called through the interface, passed to the
        /// <see cref="LocalizeDictionary"/>.<see cref="LocalizeDictionary.WeakCultureChangedEventManager"/> to get notified on culture changed
        /// </summary>
        /// <param name="managerType">The manager Type.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event argument.</param>
        /// <returns>
        /// true if the listener handled the event. It is considered an error by the <see cref="T:System.Windows.WeakEventManager"/> handling in WPF to register a listener for an event that the listener does not handle. Regardless, the method should return false if it receives an event that it does not recognize or handle.
        /// </returns>
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            // if the passed handler is type of LocalizeDictionary.WeakCultureChangedEventManager, handle it
            if (managerType == typeof(LocalizeDictionary.WeakCultureChangedEventManager))
            {
                // call to handle the new value
                this.HandleNewValue();

                // return true, to notify the event was processed
                return true;
            }

            // return false, to notify the event was not processed
            return false;
        }

        /// <summary>
        /// Determines whether if the <paramref name="checkType"/> is the <paramref name="targetType"/>.
        /// </summary>
        /// <param name="checkType">Type of the check.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="checkType"/> is type of the <paramref name="targetType"/>; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsTypeOf(Type checkType, Type targetType)
        {
            // if the checkType is null (possible base type), return false
            if (checkType == null)
            {
                return false;
            }

            // if the targetType (wrong call) is null, return false
            if (targetType == null)
            {
                return false;
            }

            // if we search a generic type
            if (targetType.IsGenericType)
            {
                // and the checkType is a generic (BaseType)
                if (checkType.IsGenericType)
                {
                    // and the signature is the same
                    if (checkType.GetGenericTypeDefinition() == targetType)
                    {
                        // return true
                        return true;
                    }
                }

                // otherwise call the same method again with the base type
                return this.IsTypeOf(checkType.BaseType, targetType);
            }

            // if we search a non generic type and its equal
            if (checkType.Equals(targetType))
            {
                // return true
                return true;
            }

            // otherwise call the same method again with the base type
            return this.IsTypeOf(checkType.BaseType, targetType);
        }

        /// <summary>
        /// This method is used to modify the passed object into the target format
        /// </summary>
        /// <param name="input">The object that will be modified</param>
        /// <returns>Returns the modified object</returns>
        protected abstract object FormatOutput(object input);

        /// <summary>
        /// Determines whether this instance can provide a value for the specified resource type.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns><c>true</c> if instance can provide a value for the specified resource type; otherwise, <c>false</c>.</returns>
        protected virtual bool CanProvideValue(Type resourceType)
        {
            return true;
        }

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

        /// <summary>
        /// This method gets the new value for the target property and call <see cref="SetNewValue"/>.
        /// </summary>
        protected virtual void HandleNewValue()
        {
            // gets the new value and set it to the dependency property on the dependency object
            this.SetNewValue(
                LocalizeDictionary.Instance.GetLocalizedObject<object>(
                    this.Assembly, this.Dict, this.Key, this.GetForcedCultureOrDefault()));
        }

        /// <summary>
        /// Set the Value of the <see cref="DependencyProperty"/> to the passed Value
        /// </summary>
        /// <param name="newValue">The new Value</param>
        protected void SetNewValue(object newValue)
        {
            // set the new value to the current value, if its the type of TValue
            if (newValue is TValue)
            {
                this.CurrentValue = (TValue) newValue;
            }

            // if the list of dependency objects is empty or the target property is null, return
            if (this.targetObjects.Count == 0)
            {
                return;
            }

            // step through all dependency objects as WeakReference and refresh the value of the dependency property
            foreach (KeyValuePair<WeakReference, object> dpo in this.targetObjects)
            {
                // set the new value of the target, if the target DependencyTarget is still alive
                if (dpo.Key.IsAlive)
                {
                    if (dpo.Value is DependencyProperty)
                        this.SetTargetValue((DependencyObject) dpo.Key.Target, dpo.Value, newValue);
                    if (dpo.Value is PropertyInfo)
                        ((PropertyInfo)dpo.Value).SetValue(dpo.Key.Target, newValue, null);
                }
            }
        }

        /// <summary>
        /// Raises the notify property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void RaiseNotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Sets the target value.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="value">The value.</param>
        private void SetTargetValue(DependencyObject targetObject, object targetProperty, object value)
        {
            // check if the target property is a DependencyProperty
            if (targetProperty is DependencyProperty)
            {
                this.SetTargetValue(targetObject, (DependencyProperty) targetProperty, value);
            }
                
            // check if the target property is a PropertyInfo
            if (targetProperty is PropertyInfo)
            {
                this.SetTargetValue(targetObject, (PropertyInfo) targetProperty, value);
            }
        }

        /// <summary>
        /// Sets the target value.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="value">The value.</param>
        private void SetTargetValue(DependencyObject targetObject, DependencyProperty targetProperty, object value)
        {
            targetObject.SetValue(targetProperty, value);
        }

        /// <summary>
        /// Sets the target value.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="value">The value.</param>
        private void SetTargetValue(DependencyObject targetObject, PropertyInfo targetProperty, object value)
        {
            targetProperty.SetValue(targetObject, value, null);
        }
    }
}