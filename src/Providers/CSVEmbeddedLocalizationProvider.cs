#region Copyright information
// <copyright file="CSVEmbeddedLocalizationProvider.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Sébastien Sevrin</author>
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;

using WPFLocalizeExtension.Engine;
using XAMLMarkupExtensions.Base;

namespace WPFLocalizeExtension.Providers
{
    /// <summary>
    /// A singleton CSV provider that uses attached properties and the Parent property to iterate through the visual tree.
    /// </summary>
    public class CSVEmbeddedLocalizationProvider : CSVLocalizationProviderBase
    {
        #region Dependency Properties
        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultDictionary to set the fallback resource dictionary.
        /// </summary>
        public static readonly DependencyProperty DefaultDictionaryProperty =
                DependencyProperty.RegisterAttached(
                "DefaultDictionary",
                typeof(string),
                typeof(CSVEmbeddedLocalizationProvider),
                new PropertyMetadata(null, AttachedPropertyChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultAssembly to set the fallback assembly.
        /// </summary>
        public static readonly DependencyProperty DefaultAssemblyProperty =
            DependencyProperty.RegisterAttached(
                "DefaultAssembly",
                typeof(string),
                typeof(CSVEmbeddedLocalizationProvider),
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
            Instance.OnProviderChanged(obj);
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
            return obj.GetValueSync<string>(DefaultDictionaryProperty);
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to get the default assembly from.</param>
        /// <returns>The default assembly.</returns>
        public static string GetDefaultAssembly(DependencyObject obj)
        {
            return obj.GetValueSync<string>(DefaultAssemblyProperty);
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
            obj.SetValueSync(DefaultDictionaryProperty, value);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetDefaultAssembly(DependencyObject obj, string value)
        {
            obj.SetValueSync(DefaultAssemblyProperty, value);
        }
        #endregion
        #endregion

        #region Variables
        /// <summary>
        /// A dictionary for notification classes for changes of the individual target Parent changes.
        /// </summary>
        private readonly ParentNotifiers _parentNotifiers = new ParentNotifiers();
        #endregion

        #region Singleton Variables, Properties & Constructor
        /// <summary>
        /// The instance of the singleton.
        /// </summary>
        private static CSVEmbeddedLocalizationProvider _instance;

        /// <summary>
        /// Lock object for the creation of the singleton instance.
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// Gets the <see cref="CSVEmbeddedLocalizationProvider"/> singleton.
        /// </summary>
        public static CSVEmbeddedLocalizationProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                            _instance = new CSVEmbeddedLocalizationProvider();
                    }
                }

                // return the existing/new instance
                return _instance;
            }
        }

        /// <summary>
        /// The singleton constructor.
        /// </summary>
        private CSVEmbeddedLocalizationProvider()
        {
            ResourceManagerList = new Dictionary<string, ResourceManager>();
            AvailableCultures = new ObservableCollection<CultureInfo> {CultureInfo.InvariantCulture};
        }

        private bool _hasHeader;
        /// <summary>
        /// A flag indicating, if it has a header row.
        /// </summary>
        public bool HasHeader
        {
            get => _hasHeader;
            set => _hasHeader = value;
        }
        #endregion

        #region Abstract assembly & dictionary lookup
        /// <summary>
        /// An action that will be called when a parent of one of the observed target objects changed.
        /// </summary>
        /// <param name="obj">The target <see cref="DependencyObject"/>.</param>
        private void ParentChangedAction(DependencyObject obj)
        {
            OnProviderChanged(obj);
        }

        /// <summary>
        /// Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The assembly name, if available.</returns>
        protected override string GetAssembly(DependencyObject target)
        {
            return target?.GetValueOrRegisterParentNotifier<string>(DefaultAssemblyProperty, ParentChangedAction, _parentNotifiers); 
        }

        /// <summary>
        /// Get the dictionary from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The dictionary name, if available.</returns>
        protected override string GetDictionary(DependencyObject target)
        {
            return target?.GetValueOrRegisterParentNotifier<string>(DefaultDictionaryProperty, ParentChangedAction, _parentNotifiers);
        }

        /// <summary>
        /// Get the localized object.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target object.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public override object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
            string ret = null;

            var filename = "";

            // Call this function to provide backward compatibility.
            ParseKey(key, out var assembly, out var dictionary, out key);

            // Now try to read out the default assembly and/or dictionary.
            if (string.IsNullOrEmpty(assembly))
                assembly = GetAssembly(target);

            if (string.IsNullOrEmpty(dictionary))
                dictionary = GetDictionary(target);

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemblyInAppDomain in loadedAssemblies)
            {
                    // get the assembly name object
                    var assemblyName = new AssemblyName(assemblyInAppDomain.FullName);
            
                    // check if the name of the assembly is the seached one
                if (assemblyName.Name == assembly)
                {
                    //filename = assemblyInAppDomain.GetManifestResourceNames().Where(r => r.Contains(dictionary)).FirstOrDefault();
                    filename = assemblyInAppDomain.GetManifestResourceNames().FirstOrDefault(r => r.Contains($"{dictionary}{(string.IsNullOrEmpty(culture.Name) ? "" : "-")}{culture.Name}"));
                    if (filename != null)
                    {
                        using (var reader = new StreamReader(assemblyInAppDomain.GetManifestResourceStream(filename) ?? throw new InvalidOperationException(), Encoding.Default))
                        {
                            if (HasHeader && !reader.EndOfStream)
                                reader.ReadLine();

                            // Read each line and split it.
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();
                                if (line != null)
                                {
                                    var parts = line.Split(";".ToCharArray());

                                    if (parts.Length < 2)
                                        continue;

                                    // Check the key (1st column).
                                    if (parts[0] != key)
                                        continue;

                                    // Get the value (2nd column).
                                    ret = parts[1];
                                }
                                break;
                            }
                        }
                    }
                }
            }

            // Nothing found -> Raise the error message.
            if (ret == null)
                OnProviderError(target, key, "The key does not exist in " + filename + ".");

            return ret;
        }
        #endregion
    }
}
