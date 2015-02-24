#region Copyright information
// <copyright file="CSVEmbeddedLocalizationProvider.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Sébastien Sevrin</author>
#endregion

#if SILVERLIGHT
namespace SLLocalizeExtension.Providers
#else
namespace WPFLocalizeExtension.Providers
#endif
{
    #region Uses
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
    using Engine;
    using XAMLMarkupExtensions.Base;
    #endregion

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
        private Dictionary<DependencyObject, ParentChangedNotifier> parentNotifiers = new Dictionary<DependencyObject, ParentChangedNotifier>();
        #endregion

        #region Singleton Variables, Properties & Constructor
        /// <summary>
        /// The instance of the singleton.
        /// </summary>
        private static CSVEmbeddedLocalizationProvider instance;

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
                if (instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (instance == null)
                            instance = new CSVEmbeddedLocalizationProvider();
                    }
                }

                // return the existing/new instance
                return instance;
            }
        }

        /// <summary>
        /// The singleton constructor.
        /// </summary>
        private CSVEmbeddedLocalizationProvider()
        {
            ResourceManagerList = new Dictionary<string, ResourceManager>();
            AvailableCultures = new ObservableCollection<CultureInfo>();
            AvailableCultures.Add(CultureInfo.InvariantCulture);
        }

        private bool hasHeader = false;
        /// <summary>
        /// A flag indicating, if it has a header row.
        /// </summary>
        public bool HasHeader
        {
            get { return hasHeader; }
            set
            {
                hasHeader = value;
                //OnProviderChanged(null); 
            }
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
            if (target == null)
                return null;

            return target.GetValueOrRegisterParentNotifier<string>(CSVEmbeddedLocalizationProvider.DefaultAssemblyProperty, ParentChangedAction, parentNotifiers); 
        }

        /// <summary>
        /// Get the dictionary from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The dictionary name, if available.</returns>
        protected override string GetDictionary(DependencyObject target)
        {
            if (target == null)
                return null;

            return target.GetValueOrRegisterParentNotifier<string>(CSVEmbeddedLocalizationProvider.DefaultDictionaryProperty, ParentChangedAction, parentNotifiers);
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

            string filename = "";

            string assembly = "";
            string dictionary = "";

            // Call this function to provide backward compatibility.
            ParseKey(key, out assembly, out dictionary, out key);

            // Now try to read out the default assembly and/or dictionary.
            if (String.IsNullOrEmpty(assembly))
                assembly = GetAssembly(target);
            if (String.IsNullOrEmpty(dictionary))
                dictionary = GetDictionary(target);


            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemblyInAppDomain in loadedAssemblies)
            {
                // check if the name pf the assembly is not null
                if (assemblyInAppDomain.FullName != null)
                {
                    // get the assembly name object
                    AssemblyName assemblyName = new AssemblyName(assemblyInAppDomain.FullName);


                    // check if the name of the assembly is the seached one
                    if (assemblyName.Name == assembly)
                    {
                        //filename = assemblyInAppDomain.GetManifestResourceNames().Where(r => r.Contains(dictionary)).FirstOrDefault();
                        filename = assemblyInAppDomain.GetManifestResourceNames().Where(r => r.Contains(string.Format("{0}{1}{2}", dictionary, string.IsNullOrEmpty(culture.Name) ? "" : "-", culture.Name))).FirstOrDefault();
                        if (filename != null)
                        {
                            using (StreamReader reader = new StreamReader(assemblyInAppDomain.GetManifestResourceStream(filename), Encoding.Default))
                            {
                                if (this.HasHeader && !reader.EndOfStream)
                                    reader.ReadLine();

                                // Read each line and split it.
                                while (!reader.EndOfStream)
                                {
                                    var line = reader.ReadLine();
                                    var parts = line.Split(";".ToCharArray());

                                    if (parts.Length < 2)
                                        continue;

                                    // Check the key (1st column).
                                    if (parts[0] != key)
                                        continue;

                                    // Get the value (2nd column).
                                    ret = parts[1];
                                    break;
                                }
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
