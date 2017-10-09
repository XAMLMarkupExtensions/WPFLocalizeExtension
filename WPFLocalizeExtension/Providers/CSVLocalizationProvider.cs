#region Copyright information
// <copyright file="CSVLocalizationProvider.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Sébastien Sevrin</author>
#endregion

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;

using WPFLocalizeExtension.Engine;
using XAMLMarkupExtensions.Base;

namespace WPFLocalizeExtension.Providers
{
    /// <summary>
    /// A singleton CSV provider that uses attached properties and the Parent property to iterate through the visual tree.
    /// </summary>
    public class CSVLocalizationProvider : CSVLocalizationProviderBase
    {
        #region Dependency Properties
        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultDictionary to set the fallback resource dictionary.
        /// </summary>
        public static readonly DependencyProperty DefaultDictionaryProperty =
                DependencyProperty.RegisterAttached(
                "DefaultDictionary",
                typeof(string),
                typeof(CSVLocalizationProvider),
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
        private static CSVLocalizationProvider _instance;

        /// <summary>
        /// Lock object for the creation of the singleton instance.
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// Gets the <see cref="CSVLocalizationProvider"/> singleton.
        /// </summary>
        public static CSVLocalizationProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                            _instance = new CSVLocalizationProvider();
                    }
                }

                // return the existing/new instance
                return _instance;
            }
        }

        /// <summary>
        /// The singleton constructor.
        /// </summary>
        private CSVLocalizationProvider()
        {
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

        #region Abstract dictionary lookup
        /// <summary>
        /// An action that will be called when a parent of one of the observed target objects changed.
        /// </summary>
        /// <param name="obj">The target <see cref="DependencyObject"/>.</param>
        private void ParentChangedAction(DependencyObject obj)
        {
            OnProviderChanged(obj);
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
        /// Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The assembly name, if available.</returns>
        protected override string GetAssembly(DependencyObject target)
        {
            return target?.GetValueOrRegisterParentNotifier<string>(CSVEmbeddedLocalizationProvider.DefaultAssemblyProperty, ParentChangedAction, _parentNotifiers);
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
            const string filename = "";

            // Call this function to provide backward compatibility.
            ParseKey(key, out _, out var dictionary, out key);

            // Now try to read out the default assembly and/or dictionary.
            if (string.IsNullOrEmpty(dictionary))
                dictionary = GetDictionary(target);

            // Try to get the culture specific file.
            const string csvDirectory = "Localization";
            var csvPath = "";

            while (culture != CultureInfo.InvariantCulture)
            {
                csvPath = Path.Combine(csvDirectory, dictionary + (string.IsNullOrEmpty(culture.Name) ? "" : "." + culture.Name) + ".csv");

                if (File.Exists(csvPath))
                    break;

                culture = culture.Parent;
            }

            if (!File.Exists(csvPath))
            {
                // Take the invariant culture.
                csvPath = Path.Combine(csvDirectory, dictionary + ".csv");

                if (!File.Exists(csvPath))
                {
                    OnProviderError(target, key, "A file for the provided culture " + culture.EnglishName + " does not exist at " + csvPath + ".");
                    return null;
                }
            }

            // Open the file.
            using (var reader = new StreamReader(csvPath, Encoding.Default))
            {
                // Skip the header if needed.
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

            // Nothing found -> Raise the error message.
            if (ret == null)
                OnProviderError(target, key, "The key does not exist in " + filename + ".");

            return ret;
        }
        #endregion
    }
}
