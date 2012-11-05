#region Copyright information
// <copyright file="StaticResxLocalizationProviderBase.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

namespace WP7LocalizeExtension.Providers
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Resources;
    using System.Reflection;
    using System.Globalization;
    using System.IO;
    using System.Collections.ObjectModel;
    using System.Windows.Media;
    #endregion

    /// <summary>
    /// A singleton RESX provider that uses attached properties that are stored in static variables.
    /// Needed for Windows Phone support.
    /// </summary>
    public class StaticResxLocalizationProvider : ResxLocalizationProviderBase
    {
        #region Dependency Properties
        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultDictionary to set the fallback resource dictionary.
        /// </summary>
        public static readonly DependencyProperty DefaultDictionaryProperty =
                DependencyProperty.RegisterAttached(
                "DefaultDictionary",
                typeof(string),
                typeof(StaticResxLocalizationProvider),
                new PropertyMetadata(null, AttachedPropertyChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultAssembly to set the fallback assembly.
        /// </summary>
        public static readonly DependencyProperty DefaultAssemblyProperty =
            DependencyProperty.RegisterAttached(
                "DefaultAssembly",
                typeof(string),
                typeof(StaticResxLocalizationProvider),
                new PropertyMetadata(null, AttachedPropertyChanged));
        #endregion

        #region Dependency Property Callback
        /// <summary>
        /// Indicates, that one of the attached properties changed.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void AttachedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Instance.OnProviderChanged(null);
        }
        #endregion

        #region Dependency Property Management
        #region Get
        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default dictionary.
        /// </summary>
        /// <param name="obj">The dependency object to get the default dictionary from.</param>
        /// <returns>The default dictionary.</returns>
        public static string GetDefaultDictionary(DependencyObject obj)
        {
            return defaultDictionary;
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to get the default assembly from.</param>
        /// <returns>The default assembly.</returns>
        public static string GetDefaultAssembly(DependencyObject obj)
        {
            return defaultAssembly;
        }
        #endregion

        #region Set
        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default dictionary.
        /// </summary>
        /// <param name="obj">The dependency object to set the default dictionary to.</param>
        /// <param name="value">The dictionary.</param>
        public static void SetDefaultDictionary(DependencyObject obj, string value)
        {
            defaultDictionary = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetDefaultAssembly(DependencyObject obj, string value)
        {
            defaultAssembly = value;
        }
        #endregion
        #endregion

        #region Variables
        private static string defaultAssembly = "";
        private static string defaultDictionary = "";
        #endregion

        #region Singleton Variables, Properties & Constructor
        /// <summary>
        /// The instance of the singleton.
        /// </summary>
        private static StaticResxLocalizationProvider instance;

        /// <summary>
        /// Lock object for the creation of the singleton instance.
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// Gets the <see cref="StaticResxLocalizationProvider"/> singleton.
        /// </summary>
        public static StaticResxLocalizationProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (instance == null)
                            instance = new StaticResxLocalizationProvider();
                    }
                }

                // return the existing/new instance
                return instance;
            }
        }

        /// <summary>
        /// The singleton constructor.
        /// </summary>
        private StaticResxLocalizationProvider()
        {
            ResourceManagerList = new Dictionary<string, ResourceManager>();
            AvailableCultures = new ObservableCollection<CultureInfo>();
            AvailableCultures.Add(CultureInfo.InvariantCulture);
        }
        #endregion

        #region Abstract assembly & dictionary lookup
        /// <summary>
        /// Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The assembly name, if available.</returns>
        protected override string GetAssembly(DependencyObject target)
        {
            return defaultAssembly; 
        }

        /// <summary>
        /// Get the dictionary from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The dictionary name, if available.</returns>
        protected override string GetDictionary(DependencyObject target)
        {
            return defaultDictionary;
        }
        #endregion
    }
}
