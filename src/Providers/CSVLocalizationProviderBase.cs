#region Copyright information
// <copyright file="CSVLocalizationProviderBase.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Sébastien Sevrin</author>
#endregion

using System;
using System.Collections.Generic;
using System.Windows;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Collections.ObjectModel;

namespace WPFLocalizeExtension.Providers
{
    /// <summary>
    /// The base for CSV file providers.
    /// </summary>
    public abstract class CSVLocalizationProviderBase : DependencyObject, ILocalizationProvider
    {
        #region Variables
        /// <summary>
        /// Holds the name of the Resource Manager.
        /// </summary>
        private const string ResourceManagerName = "ResourceManager";

        /// <summary>
        /// Holds the extension of the resource files.
        /// </summary>
        private const string ResourceFileExtension = ".resources";

        /// <summary>
        /// Holds the binding flags for the reflection to find the resource files.
        /// </summary>
        private const BindingFlags ResourceBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        /// <summary>
        /// Gets the used ResourceManagers with their corresponding <c>namespaces</c>.
        /// </summary>
        protected Dictionary<string, ResourceManager> ResourceManagerList;

        /// <summary>
        /// Lock object for concurrent access to the resource manager list.
        /// </summary>
        protected object ResourceManagerListLock = new object();

        /// <summary>
        /// Lock object for concurrent access to the available culture list.
        /// </summary>
        protected object AvailableCultureListLock = new object();
        #endregion

        #region Helper functions
        /// <summary>
        /// Returns the <see cref="AssemblyName"/> of the passed assembly instance
        /// </summary>
        /// <param name="assembly">The Assembly where to get the name from</param>
        /// <returns>The Assembly name</returns>
        protected string GetAssemblyName(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (assembly.FullName == null)
                throw new NullReferenceException("assembly.FullName is null");

            return assembly.FullName.Split(',')[0];
        }

        /// <summary>
        /// Parses a key ([[Assembly:]Dict:]Key and return the parts of it.
        /// </summary>
        /// <param name="inKey">The key to parse.</param>
        /// <param name="outAssembly">The found or default assembly.</param>
        /// <param name="outDict">The found or default dictionary.</param>
        /// <param name="outKey">The found or default key.</param>
        public static void ParseKey(string inKey, out string outAssembly, out string outDict, out string outKey)
        {
            // Reset everything to null.
            outAssembly = null;
            outDict = null;
            outKey = null;

            if (!string.IsNullOrEmpty(inKey))
            {
                var split = inKey.Trim().Split(":".ToCharArray());

                // assembly:dict:key
                if (split.Length == 3)
                {
                    outAssembly = !string.IsNullOrEmpty(split[0]) ? split[0] : null;
                    outDict = !string.IsNullOrEmpty(split[1]) ? split[1] : null;
                    outKey = split[2];
                }

                // dict:key
                if (split.Length == 2)
                {
                    outDict = !string.IsNullOrEmpty(split[0]) ? split[0] : null;
                    outKey = split[1];
                }

                // key
                if (split.Length == 1)
                    outKey = split[0];
            }
        }
        #endregion

        #region Abstract assembly & dictionary lookup
        /// <summary>
        /// Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The assembly name, if available.</returns>
        protected abstract string GetAssembly(DependencyObject target);

        /// <summary>
        /// Get the dictionary from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The dictionary name, if available.</returns>
        protected abstract string GetDictionary(DependencyObject target);
        #endregion

        #region Culture Management
        /// <summary>
        /// Thread-safe access to the AvailableCultures list.
        /// </summary>
        /// <param name="c">The CultureInfo.</param>
        protected void AddCulture(CultureInfo c)
        {
            lock (AvailableCultureListLock)
            {
                if (!AvailableCultures.Contains(c))
                    AvailableCultures.Add(c);
            }
        }
        #endregion

        #region ILocalizationProvider implementation
        /// <summary>
        /// Uses the key and target to build a fully qualified resource key (Assembly, Dictionary, Key)
        /// </summary>
        /// <param name="key">Key used as a base to find the full key</param>
        /// <param name="target">Target used to help determine key information</param>
        /// <returns>Returns an object with all possible pieces of the given key (Assembly, Dictionary, Key)</returns>
        public FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(string key, DependencyObject target)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            ParseKey(key, out var assembly, out var dictionary, out key);

            if (target == null)
                return new FQAssemblyDictionaryKey(key, assembly, dictionary);

            if (string.IsNullOrEmpty(assembly))
                assembly = GetAssembly(target);

            if (string.IsNullOrEmpty(dictionary))
                dictionary = GetDictionary(target);

            return new FQAssemblyDictionaryKey(key, assembly, dictionary);
        }

        /// <summary>
        /// Gets fired when the provider changed.
        /// </summary>
        public event ProviderChangedEventHandler ProviderChanged;

        /// <summary>
        /// An event that is fired when an error occurred.
        /// </summary>
        public event ProviderErrorEventHandler ProviderError;

        /// <summary>
        /// An event that is fired when a value changed.
        /// </summary>
        public event ValueChangedEventHandler ValueChanged;

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ProviderChanged"/> event.
        /// </summary>
        /// <param name="target">The target object.</param>
        protected virtual void OnProviderChanged(DependencyObject target)
        {
            try
            {
                var assembly = GetAssembly(target);
                var dictionary = GetDictionary(target);

                //if (!String.IsNullOrEmpty(assembly) && !String.IsNullOrEmpty(dictionary))
                //    GetResourceManager(assembly, dictionary);
            }
            catch
            {
                // ignored
            }

            ProviderChanged?.Invoke(this, new ProviderChangedEventArgs(target));
        }

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ProviderError"/> event.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="key">The key.</param>
        /// <param name="message">The error message.</param>
        protected virtual void OnProviderError(DependencyObject target, string key, string message)
        {
            ProviderError?.Invoke(this, new ProviderErrorEventArgs(target, key, message));
        }

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ValueChanged"/> event.
        /// </summary>
        /// <param name="key">The key where the value was changed.</param>
        /// <param name="value">The new value.</param>
        /// <param name="tag">A custom tag.</param>
        protected virtual void OnValueChanged(string key, object value, object tag)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(key, value, tag));
        }

        /// <summary>
        /// Get the localized object.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target object.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public virtual object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
            throw new InvalidOperationException("GetLocalizedObject needs to be overriden");
        }

        /// <summary>
        /// An observable list of available cultures.
        /// </summary>
        public ObservableCollection<CultureInfo> AvailableCultures { get; protected set; }
        #endregion
    }
}