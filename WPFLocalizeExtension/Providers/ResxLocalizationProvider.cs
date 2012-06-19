#region Copyright information
// <copyright file="ResxLocalizationProvider.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
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
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Resources;
    using System.Reflection;
    using System.Globalization;
    using System.IO;
    using System.Collections.ObjectModel;
    using System.Windows.Media;
    using XAMLMarkupExtensions.Base; 
#if SILVERLIGHT
    using SLLocalizeExtension.Engine;
#else
    using WPFLocalizeExtension.Engine;
#endif
    #endregion

    /// <summary>
    /// The very basic resource provider with RESX files.
    /// </summary>
    public class ResxLocalizationProvider : DependencyObject, ILocalizationProvider
    {
        #region Dependency Properties
        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultDictionary to set the fallback resource dictionary.
        /// </summary>
        public static readonly DependencyProperty DefaultDictionaryProperty =
                DependencyProperty.RegisterAttached(
                "DefaultDictionary",
                typeof(string),
                typeof(ResxLocalizationProvider),
                new PropertyMetadata(null, AttachedPropertyChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> DefaulAssembly to set the fallback assembly.
        /// </summary>
        public static readonly DependencyProperty DefaultAssemblyProperty =
            DependencyProperty.RegisterAttached(
                "DefaulAssembly",
                typeof(string),
                typeof(ResxLocalizationProvider),
                new PropertyMetadata(null, AttachedPropertyChanged));
        #endregion

        #region Dependency Property Callback
        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.DefaultDictionary if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void AttachedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (Instance.ProviderChanged != null)
                Instance.ProviderChanged(Instance, new ProviderChangedEventArgs(obj));
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
            return (obj != null) ? (string)obj.GetValue(DefaultDictionaryProperty) : null;
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to get the default assembly from.</param>
        /// <returns>The default assembly.</returns>
        public static string GetDefaultAssembly(DependencyObject obj)
        {
            return (obj != null) ? (string)obj.GetValue(DefaultAssemblyProperty) : null;
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
            obj.SetValue(DefaultDictionaryProperty, value);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetDefaultAssembly(DependencyObject obj, string value)
        {
            obj.SetValue(DefaultAssemblyProperty, value);
        }
        #endregion
        #endregion

        #region Variables
        /// <summary>
        /// Holds the default <see cref="ResourceDictionary"/> name
        /// </summary>
        private const string ResourcesName = "Resources";

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
        private Dictionary<string, ResourceManager> ResourceManagerList;

        /// <summary>
        /// Lock object for concurrent access to the resource manager list.
        /// </summary>
        private object ResourceManagerListLock = new object();

        /// <summary>
        /// Lock object for concurrent access to the available culture list.
        /// </summary>
        private object AvailableCultureListLock = new object();

        /// <summary>
        /// A dictionary for notification classes for changes of the individual target Parent changes.
        /// </summary>
        private Dictionary<DependencyObject, ParentChangedNotifier> parentNotifiers = new Dictionary<DependencyObject, ParentChangedNotifier>();
        #endregion

        #region Helper functions
        /// <summary>
        /// Returns the <see cref="AssemblyName"/> of the passed assembly instance
        /// </summary>
        /// <param name="assembly">The Assembly where to get the name from</param>
        /// <returns>The Assembly name</returns>
        private string GetAssemblyName(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            if (assembly.FullName == null)
            {
                throw new NullReferenceException("assembly.FullName is null");
            }

            return assembly.FullName.Split(',')[0];
        }
        #endregion

        #region ResourceManager Management
        /// <summary>
        /// Thread-safe access to the resource manager dictionary.
        /// </summary>
        /// <param name="thekey">Key.</param>
        /// <param name="result">Value.</param>
        /// <returns>Success of the operation.</returns>
        public bool TryGetValue(string thekey, out ResourceManager result)
        {
            lock (ResourceManagerListLock) { return this.ResourceManagerList.TryGetValue(thekey, out result); }
        }

        /// <summary>
        /// Thread-safe access to the resource manager dictionary.
        /// </summary>
        /// <param name="thekey">Key.</param>
        /// <param name="value">Value.</param>
        public void Add(string thekey, ResourceManager value)
        {
            lock (ResourceManagerListLock) { this.ResourceManagerList.Add(thekey, value); }
        }

        /// <summary>
        /// Thread-safe access to the AvailableCultures list.
        /// </summary>
        /// <param name="c">The CultureInfo.</param>
        public void AddCulture(CultureInfo c)
        {
            lock (AvailableCultureListLock)
            {
                if (!this.AvailableCultures.Contains(c))
                    this.AvailableCultures.Add(c);
            }
        } 

        /// <summary>
        /// Looks up in the cached <see cref="ResourceManager"/> list for the searched <see cref="ResourceManager"/>.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly (e.g.: <c>BaseLocalizeExtension</c>). NULL = Name of the executing assembly</param>
        /// <param name="resourceDictionary">The dictionary to look up (e.g.: ResHelp, Resources, ...). NULL = Name of the default resource file (Resources)</param>
        /// <returns>
        /// The found <see cref="ResourceManager"/>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the ResourceManagers cannot be looked up
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the searched <see cref="ResourceManager"/> wasn't found
        /// </exception>
        private ResourceManager GetResourceManager(string resourceAssembly, string resourceDictionary)
        {
            // Check validity of the assembly and dictionary.
            if (resourceAssembly == null)
                resourceAssembly = this.GetAssemblyName(Assembly.GetExecutingAssembly());

            if (resourceDictionary == null)
                resourceDictionary = ResourcesName;

            PropertyInfo propInfo;
            MethodInfo methodInfo;
            Assembly assembly = null;
            ResourceManager resManager;
            string foundResource = null;
            string resManagerNameToSearch = "." + resourceDictionary + ResourceFileExtension;
            string[] availableResources;
            var keyManKey = resourceAssembly + resManagerNameToSearch;

            if (!TryGetValue(keyManKey, out resManager))
            {
                // if the assembly cannot be loaded, throw an exception
                try
                {
                    // go through every assembly loaded in the app domain
                    foreach (Assembly assemblyInAppDomain in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        // check if the name pf the assembly is not null
                        if (assemblyInAppDomain.FullName != null)
                        {
                            // get the assembly name object
                            AssemblyName assemblyName = new AssemblyName(assemblyInAppDomain.FullName);

                            // check if the name of the assembly is the seached one
                            if (assemblyName.Name == resourceAssembly)
                            {
                                // assigne the assembly
                                assembly = assemblyInAppDomain;

                                // stop the search here
                                break;
                            }
                        }
                    }

                    // check if the assembly is still null
                    if (assembly == null)
                    {
                        // assign the loaded assembly
#if SILVERLIGHT
                        var name = new AssemblyName(resourceAssembly);
                        assembly = Assembly.Load(name.FullName);
#else
                        assembly = Assembly.Load(new AssemblyName(resourceAssembly));
#endif
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("The Assembly '{0}' cannot be loaded.", resourceAssembly), ex);
                }

                // get all available resourcenames
                availableResources = assembly.GetManifestResourceNames();

                // The proposed approach of Andras (http://wpflocalizeextension.codeplex.com/discussions/66098?ProjectName=wpflocalizeextension)
                var possiblePrefixes = new List<string>(assembly.GetTypes().Select((t) => t.Namespace).Distinct());

                for (int i = 0; i < availableResources.Length; i++)
                {
                    if (availableResources[i].EndsWith(resManagerNameToSearch))
                    {
                        var matches = possiblePrefixes.Where((p) => availableResources[i].StartsWith(p + "."));
                        if (matches.Count() != 0)
                        {
                            // take the first occurrence and break
                            foundResource = availableResources[i];
                            break;
                        }
                    }
                }

                // if no one was found, exception
                if (foundResource == null)
                {
                    throw new ArgumentException(
                        string.Format(
                            "No resource manager for dictionary '{0}' in assembly '{1}' found! ({1}.{0})",
                            resourceDictionary,
                            resourceAssembly));
                }

                // remove ".resources" from the end
                foundResource = foundResource.Substring(0, foundResource.Length - ResourceFileExtension.Length);

                //// Resources.{foundResource}.ResourceManager.GetObject()
                //// ^^ prop-info      ^^ method get

                try
                {
                    // get the propertyinfo from resManager over the type from foundResource
                    var resourceManagerType = assembly.GetType(foundResource);

                    // check if the resource manager was found.
                    // if not, assume that the assembly was build with VisualBasic.
                    // in this case try to manipulate the resource identifier.
                    if (resourceManagerType == null)
                    {
#if SILVERLIGHT
                        var assemblyName = resourceAssembly;
#else
                        var assemblyName = assembly.GetName().Name;
#endif
                        resourceManagerType = assembly.GetType(foundResource.Replace(assemblyName, assemblyName + ".My.Resources"));
                    }

                    propInfo = resourceManagerType.GetProperty(ResourceManagerName, ResourceBindingFlags);

                    // get the GET-method from the methodinfo
                    methodInfo = propInfo.GetGetMethod(true);

                    // get the static ResourceManager property
                    object resManObject = methodInfo.Invoke(null, null);

                    // cast it to a ResourceManager for better working with
                    resManager = (ResourceManager)resManObject;
                }
                catch (Exception ex)
                {
                    // this error has to get thrown because this has to work
                    throw new InvalidOperationException("Cannot resolve the ResourceManager!", ex);
                }

                // Add the ResourceManager to the cachelist
                Add(keyManKey, resManager);

                try
                {
                    var assemblyLocation = Path.GetDirectoryName(assembly.Location);

#if SILVERLIGHT
                // Get all directories named like a specific culture.
                var dirs = Directory.EnumerateDirectories(assemblyLocation, "??-??").ToList();
                // Get all directories named like a culture.
                dirs.AddRange(Directory.EnumerateDirectories(assemblyLocation, "??"));

                var cultures = CultureInfoHelper.GetCultures();

                foreach (var c in cultures)
                {
                    var dir = Path.Combine(assemblyLocation, c.Name);
                    if (Directory.Exists(dir) &&
                        Directory.EnumerateFiles(dir, "*.resources.dll").ToList().Count > 0)
                        AddCulture(c);
                }
#else
                    // Get all directories named like a specific culture.
                    var dirs = Directory.GetDirectories(assemblyLocation, "??-??").ToList();
                    // Get all directories named like a culture.
                    dirs.AddRange(Directory.GetDirectories(assemblyLocation, "??"));

                    // Get the list of all cultures.
                    var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                    foreach (var c in cultures)
                    {
                        var dir = Path.Combine(assemblyLocation, c.Name);
                        if (Directory.Exists(dir) &&
                            Directory.GetFiles(dir, "*.resources.dll").Length > 0)
                            AddCulture(c);
                    }
#endif
                }
                catch
                {
                    // This may lead to problems with Silverlight
                }
            }

            // return the found ResourceManager
            return resManager;
        }
        #endregion

        #region Backward compatibility
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
                string[] split = inKey.Trim().Split(":".ToCharArray());

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
                {
                    outKey = split[0];
                }
            }
        }
        #endregion

        #region ILocalizationProvider implementation
        /// <summary>
        /// Gets fired when the provider changed.
        /// </summary>
        public event ProviderChangedEventHandler ProviderChanged;

        /// <summary>
        /// An event when an error occurred.
        /// </summary>
        public event ProviderErrorEventHandler ProviderError;

        /// <summary>
        /// An action that will be called by the <see cref="ParentChangedNotifier"/>.
        /// </summary>
        /// <param name="obj">The target <see cref="DependencyObject"/>.</param>
        private void ParentChangedAction(DependencyObject obj)
        {
            if (ProviderChanged != null)
                ProviderChanged(this, new ProviderChangedEventArgs(obj));
        }

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ProviderError"/> event.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="key">The key.</param>
        /// <param name="message">The error message.</param>
        private void OnProviderError(DependencyObject target, string key, string message)
        {
            if (ProviderError != null)
                ProviderError(this, new ProviderErrorEventArgs(target, key, message));
        }

        /// <summary>
        /// Get the localized object.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target object.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
            string assembly = "";
            string dictionary = "";

            // Call this function to provide backward compatibility.
            ParseKey(key, out assembly, out dictionary, out key);

            // Now try to read out the default assembly and/or dictionary.
            if (target != null)
            {
                if (String.IsNullOrEmpty(assembly))
                    assembly = target.GetValueOrRegisterParentNotifier<string>(ResxLocalizationProvider.DefaultAssemblyProperty, ParentChangedAction, parentNotifiers);
                if (String.IsNullOrEmpty(dictionary))
                    dictionary = target.GetValueOrRegisterParentNotifier<string>(ResxLocalizationProvider.DefaultDictionaryProperty, ParentChangedAction, parentNotifiers);
            }

            // Final validation of the values.
            if (String.IsNullOrEmpty(assembly))
            {
                OnProviderError(target, key, "No assembly provided.");
                return null;
            }

            if (String.IsNullOrEmpty(dictionary))
            {
                OnProviderError(target, key, "No dictionary provided.");
                return null;
            }

            if (String.IsNullOrEmpty(key))
            {
                OnProviderError(target, key, "No key provided.");
                return null;
            }

            // declaring local resource manager
            ResourceManager resManager;

            // try to get the resouce manager
            try
            {
                resManager = GetResourceManager(assembly, dictionary);
            }
            catch (Exception e)
            {
                OnProviderError(target, key, "Error retrieving the resource manager\r\n" + e.Message);
                return null;
            }

            // finally, return the searched object as type of the generic type
            return resManager.GetObject(key, culture);
        }

        /// <summary>
        /// An observable list of available cultures.
        /// </summary>
        public ObservableCollection<CultureInfo> AvailableCultures { get; private set; }
        #endregion

        #region Singleton Variables, Properties & Constructor
        /// <summary>
        /// The instance of the singleton.
        /// </summary>
        private static ResxLocalizationProvider instance;

        /// <summary>
        /// Lock object for the creation of the singleton instance.
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// Gets the <see cref="ResxLocalizationProvider"/> singleton.
        /// </summary>
        public static ResxLocalizationProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (instance == null)
                            instance = new ResxLocalizationProvider();
                    }
                }

                // return the existing/new instance
                return instance;
            }
        }

        /// <summary>
        /// The singleton constructor.
        /// </summary>
        private ResxLocalizationProvider()
        {
            ResourceManagerList = new Dictionary<string, ResourceManager>();
            AvailableCultures = new ObservableCollection<CultureInfo>();
            AvailableCultures.Add(CultureInfo.InvariantCulture);
        }
        #endregion
    }
}
