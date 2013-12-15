#region Copyright information
// <copyright file="BLoc.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.Extensions
#elif SILVERLIGHT
namespace SLLocalizeExtension.Extensions
#else
namespace WPFLocalizeExtension.Extensions
#endif
{
    using System;
    using System.Windows.Data;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Windows;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Media.Imaging;
    using System.Collections;
#if WINDOWS_PHONE
    using WP7LocalizeExtension.TypeConverters;
    using WP7LocalizeExtension.Engine;
#elif SILVERLIGHT
    using SLLocalizeExtension.TypeConverters;
    using SLLocalizeExtension.Engine;
#else
    using WPFLocalizeExtension.TypeConverters;
    using WPFLocalizeExtension.Engine;
#endif

    /// <summary>
    /// A localization extension based on <see cref="Binding"/>.
    /// </summary>
    public class BLoc : Binding, INotifyPropertyChanged, IDictionaryEventListener, IDisposable
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

        #region Variables & Properties
        private static object resourceBufferLock = new object();
        private static Dictionary<string, object> ResourceBuffer = new Dictionary<string, object>();

        private object value = null;
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

        private string key;
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
        /// Gets or sets the culture to force a fixed localized object
        /// </summary>
        public string ForceCulture { get; set; } 
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

        #region Constructors & Dispose
        /// <summary>
        /// Initializes a new instance of the <see cref="BLoc"/> class.
        /// </summary>
        public BLoc()
            : base()
        {
            LocalizeDictionary.DictionaryEvent.AddListener(this);
            this.Path = new PropertyPath("Value");
            this.Source = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BLoc"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public BLoc(string key)
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
        ~BLoc()
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
            this.Value = FormatOutput();
        }

        #region Future TargetMarkupExtension implementation
        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        public object FormatOutput()
        {
            object result = null;

            // Try to get the localized input from the resource.
            string resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(Key, null);

            CultureInfo ci = GetForcedCultureOrDefault();

            string key = ci.Name + ":";

            // Check, if the key is already in our resource buffer.
            if (ResourceBuffer.ContainsKey(key + resourceKey))
                result = ResourceBuffer[key + resourceKey];
            else
            {
                result = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, null, ci);

                if (result == null)
                    return null;
                else
                {
                    key += resourceKey;
                    SafeAddItemToResourceBuffer(key, result);
                }
            }

            return result;
        }
        #endregion
    }
}
