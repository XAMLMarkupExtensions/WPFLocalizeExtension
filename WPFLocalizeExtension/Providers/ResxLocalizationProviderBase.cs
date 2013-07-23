#region Copyright information
// <copyright file="ResxLocalizationProviderBase.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
// <author>Bernhard Millauer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.Providers
#elif SILVERLIGHT
namespace SLLocalizeExtension.Providers
#else
namespace WPFLocalizeExtension.Providers
#endif
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Windows;
    #endregion

    /// <summary>
    /// The base for RESX file providers.
    /// </summary>
    public abstract class ResxLocalizationProviderBase : DependencyObject, ILocalizationProvider
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
            {
                throw new ArgumentNullException("assembly");
            }

            if (assembly.FullName == null)
            {
                throw new NullReferenceException("assembly.FullName is null");
            }

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

        #region ResourceManager management
        /// <summary>
        /// Thread-safe access to the resource manager dictionary.
        /// </summary>
        /// <param name="thekey">Key.</param>
        /// <param name="result">Value.</param>
        /// <returns>Success of the operation.</returns>
        protected bool TryGetValue(string thekey, out ResourceManager result)
        {
            lock (ResourceManagerListLock) { return this.ResourceManagerList.TryGetValue(thekey, out result); }
        }

        /// <summary>
        /// Thread-safe access to the resource manager dictionary.
        /// </summary>
        /// <param name="thekey">Key.</param>
        /// <param name="value">Value.</param>
        protected void Add(string thekey, ResourceManager value)
        {
            lock (ResourceManagerListLock) { this.ResourceManagerList.Add(thekey, value); }
        }

        /// <summary>
        /// Tries to remove a key from the resource manager dictionary.
        /// </summary>
        /// <param name="thekey">Key.</param>
        protected void TryRemove(string thekey)
        {
            lock (ResourceManagerListLock) { if (this.ResourceManagerList.ContainsKey(thekey)) this.ResourceManagerList.Remove(thekey); }
        }

        /// <summary>
        /// Thread-safe access to the AvailableCultures list.
        /// </summary>
        /// <param name="c">The CultureInfo.</param>
        protected void AddCulture(CultureInfo c)
        {
            lock (AvailableCultureListLock)
            {
                if (!this.AvailableCultures.Contains(c))
                    this.AvailableCultures.Add(c);
            }
        }

        /// <summary>
        /// Updates the list of available cultures using the given resource location.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly.</param>
        /// <param name="resourceDictionary">The dictionary to look up.</param>
        /// <returns>True, if the update was successful.</returns>
        public bool UpdateCultureList(string resourceAssembly, string resourceDictionary)
        {
            return GetResourceManager(resourceAssembly, resourceDictionary) != null;
        }

        /// <summary>
        /// Looks up in the cached <see cref="ResourceManager"/> list for the searched <see cref="ResourceManager"/>.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly.</param>
        /// <param name="resourceDictionary">The dictionary to look up.</param>
        /// <returns>
        /// The found <see cref="ResourceManager"/>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the ResourceManagers cannot be looked up
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the searched <see cref="ResourceManager"/> wasn't found
        /// </exception>
        protected ResourceManager GetResourceManager(string resourceAssembly, string resourceDictionary)
        {
            PropertyInfo propInfo;
            MethodInfo methodInfo;
            Assembly assembly = null;
            ResourceManager resManager;
            string foundResource = null;
            string resManagerNameToSearch = "." + resourceDictionary + ResourceFileExtension;
            string[] availableResources;

            //var resManKey = designPath + resourceAssembly + resManagerNameToSearch;
            var resManKey = resourceAssembly + resManagerNameToSearch;

#if !SILVERLIGHT
            if (AppDomain.CurrentDomain.FriendlyName.Contains("XDesProc"))
            {
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                foreach (var process in Process.GetProcesses())
                {
                    if (!process.ProcessName.Contains(".vshost"))
                        continue;

                    var dir = Path.GetDirectoryName(process.Modules[0].FileName);
                    var files = Directory.GetFiles(dir, resourceAssembly + ".*", SearchOption.AllDirectories);

                    if (files.Length > 0)
                    {
                        files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                        bool updateManager = false;

                        foreach (var f in files)
                        {
                            try
                            {
                                var dst = Path.Combine(assemblyDir, f.Replace(dir + "\\", ""));
                                if (!File.Exists(dst) || (Directory.GetLastWriteTime(dst) < Directory.GetLastWriteTime(f)))
                                {
                                    var dstDir = Path.GetDirectoryName(dst);
                                    if (!Directory.Exists(dstDir))
                                        Directory.CreateDirectory(dstDir);
                                    File.Copy(f, dst, true);
                                    updateManager = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                updateManager = true;
                            }
                        }

                        if (updateManager)
                            TryRemove(resManKey);

                        var file = Path.Combine(assemblyDir, resourceAssembly + ".exe");
                        if (!File.Exists(file))
                            file = Path.Combine(assemblyDir, resourceAssembly + ".dll");

                        assembly = Assembly.LoadFrom(file);
                        break;
                    }
                }
            }
#endif

            if (!TryGetValue(resManKey, out resManager))
            {
                // If the assembly cannot be loaded, throw an exception
                if (assembly == null)
                {
                    try
                    {
                        // go through every assembly loaded in the app domain
                        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var assemblyInAppDomain in loadedAssemblies)
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

                // NOTE: Inverted this IF (nesting is bad, I know) so we just create a new ResourceManager.  -gen3ric
                if (foundResource != null)
                {
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
                            foreach (var type in assembly.GetExportedTypes())
                            {
                                if (type.Name == resourceDictionary)
                                {
                                    resourceManagerType = type;
                                    break;
                                }
                            }
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
                }
                else
                {
                    resManager = new ResourceManager(resManagerNameToSearch, assembly);
                }

                // if no one was found, exception
                if (resManager == null)
                    throw new ArgumentException(string.Format("No resource manager for dictionary '{0}' in assembly '{1}' found! ({1}.{0})", resourceDictionary, resourceAssembly));

                // Add the ResourceManager to the cachelist
                Add(resManKey, resManager);

                try
                {
#if SILVERLIGHT
                    var cultures = CultureInfoHelper.GetCultures();

                    foreach (var c in cultures)
                    {
                        var dir = c.Name + "/";

                        foreach (var p in Deployment.Current.Parts)
                            if (p.Source.StartsWith(dir))
                            {
                                AddCulture(c);
                                break;
                            }
                    }
#else
                    var assemblyLocation = Path.GetDirectoryName(assembly.Location);

                    // Get the list of all cultures.
                    var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                    foreach (var c in cultures)
                    {
                        var dir = Path.Combine(assemblyLocation, c.Name);
                        if (Directory.Exists(dir) && Directory.GetFiles(dir, "*.resources.dll").Length > 0)
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

        #region ILocalizationProvider implementation
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

                if (!String.IsNullOrEmpty(assembly) && !String.IsNullOrEmpty(dictionary))
                    GetResourceManager(assembly, dictionary);
            }
            catch
            {
            }

            if (ProviderChanged != null)
                ProviderChanged(this, new ProviderChangedEventArgs(target));
        }

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ProviderError"/> event.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="key">The key.</param>
        /// <param name="message">The error message.</param>
        protected virtual void OnProviderError(DependencyObject target, string key, string message)
        {
            if (ProviderError != null)
                ProviderError(this, new ProviderErrorEventArgs(target, key, message));
        }

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ValueChanged"/> event.
        /// </summary>
        /// <param name="key">The key where the value was changed.</param>
        /// <param name="value">The new value.</param>
        /// <param name="tag">A custom tag.</param>
        protected virtual void OnValueChanged(string key, object value, object tag)
        {
            if (ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs(key, value, tag));
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
            string assembly = "";
            string dictionary = "";

            // Call this function to provide backward compatibility.
            ParseKey(key, out assembly, out dictionary, out key);

            // Now try to read out the default assembly and/or dictionary.
            if (String.IsNullOrEmpty(assembly))
                assembly = GetAssembly(target);
            if (String.IsNullOrEmpty(dictionary))
                dictionary = GetDictionary(target);

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
        public ObservableCollection<CultureInfo> AvailableCultures { get; protected set; }
        #endregion
    }
}
