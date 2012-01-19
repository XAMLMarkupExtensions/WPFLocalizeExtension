using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

//// Register this namespace under admirals one with prefix
[assembly: XmlnsDefinition("http://schemas.root-project.org/xaml/presentation", "WPFLocalizeExtension.Engine")]
[assembly: XmlnsDefinition("http://schemas.root-project.org/xaml/presentation", "WPFLocalizeExtension.Extensions")]
//// Assign a default namespace prefix for the schema
[assembly: XmlnsPrefix("http://schemas.root-project.org/xaml/presentation", "lex")]

namespace WPFLocalizeExtension.Engine
{
    /// <summary>
    /// Represents the culture interface for localization
    /// </summary>
    public sealed class LocalizeDictionary : DependencyObject
    {
        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultDictionary to set the fallback resource dictionary.
        /// </summary>
        public static readonly DependencyProperty DefaultDictionaryProperty =
            DependencyProperty.RegisterAttached(
            "DefaultDictionary",
            typeof(string),
            typeof(LocalizeDictionary),
            new FrameworkPropertyMetadata(ResourcesName, FrameworkPropertyMetadataOptions.Inherits, SetDictionaryFromDependencyProperty));

        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultAssembly to set the fallback assembly.
        /// </summary>
        public static readonly DependencyProperty DefaultAssemblyProperty =
            DependencyProperty.RegisterAttached(
            "DefaultAssembly",
            typeof(string),
            typeof(LocalizeDictionary),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, SetAssemblyFromDependencyProperty));
        
        /// <summary>
        /// <see cref="DependencyProperty"/> DesignCulture to set the Culture.
        /// Only supported at DesignTime.
        /// </summary>
        [DesignOnly(true)] public static readonly DependencyProperty DesignCultureProperty =
            DependencyProperty.RegisterAttached(
                "DesignCulture",
                typeof(string),
                typeof(LocalizeDictionary),
                new PropertyMetadata(SetCultureFromDependencyProperty));

        /// <summary>
        /// Holds the default <see cref="ResourceDictionary"/> name
        /// </summary>
        public const string ResourcesName = "Resources";

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
        /// Holds a SyncRoot to be thread safe
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Holds the instance of singleton
        /// </summary>
        private static LocalizeDictionary instance;

        /// <summary>
        /// Holds the current chosen <see cref="CultureInfo"/>
        /// </summary>
        private CultureInfo culture;

        /// <summary>
        /// Prevents a default instance of the <see cref="LocalizeDictionary"/> class from being created.
        /// Static Constructor
        /// </summary>
        private LocalizeDictionary()
        {
            this.ResourceManagerList = new Dictionary<string, ResourceManager>();
        }

        /// <summary>
        /// Get raised if the <see cref="LocalizeDictionary"/>.Culture is changed.
        /// </summary>
        internal event Action OnCultureChanged;

        /// <summary>
        /// Gets the default <see cref="CultureInfo"/> to initialize the <see cref="LocalizeDictionary"/>.<see cref="CultureInfo"/>
        /// </summary>
        public static CultureInfo DefaultCultureInfo
        {
            get { return CultureInfo.InvariantCulture; }
        }

        /// <summary>
        /// Gets the <see cref="LocalizeDictionary"/> singleton.
        /// If the underlying instance is null, a instance will be created.
        /// </summary>
        public static LocalizeDictionary Instance
        {
            get
            {
                // check if the underlying instance is null
                if (instance == null)
                {
                    // if it is null, lock the syncroot.
                    // if another thread is accessing this too, 
                    // it have to wait until the syncroot is released
                    lock (SyncRoot)
                    {
                        // check again, if the underlying instance is null
                        if (instance == null)
                        {
                            // create a new instance
                            instance = new LocalizeDictionary();
                        }
                    }
                }

                // return the existing/new instance
                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> for localization.
        /// On set, <see cref="OnCultureChanged"/> is raised.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// You have to set <see cref="LocalizeDictionary"/>.Culture first or 
        /// wait until System.Windows.Application.Current.MainWindow is created.
        /// Otherwise you will get an Exception.</exception>
        /// <exception cref="System.ArgumentNullException">thrown if Culture will be set to null</exception>
        public CultureInfo Culture
        {
            get
            {
                if (this.culture == null)
                {
                    this.culture = DefaultCultureInfo;

                    //// try {
                    //// culture = System.Windows.Application.Current.MainWindow.Language.GetEquivalentCulture();
                    //// }
                    //// catch (InvalidOperationException) {
                    //// throw;
                    //// }
                    //// catch (Exception ex) {
                    //// throw new InvalidOperationException("Cannot read CultureInfo from " +
                    //// "System.Windows.Application.Current.MainWindow.Language. " +
                    //// "You have to set LocDictionary.Culture first or wait until " + 
                    //// "System.Windows.Application.Current.MainWindow is created.", ex);
                    //// }
                }

                return this.culture;
            }

            set
            {
                // the cultureinfo cannot contains a null reference
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                // Set the CultureInfo
                this.culture = value;

                // Raise the OnCultureChanged event
                if (this.OnCultureChanged != null)
                {
                    this.OnCultureChanged();
                }
            }
        }

        /// <summary>
        /// Gets the used ResourceManagers with their corresponding <c>namespaces</c>.
        /// </summary>
        public Dictionary<string, ResourceManager> ResourceManagerList { get; private set; }

        /// <summary>
        /// Gets the specific <see cref="CultureInfo"/> of the current culture.
        /// This can be used for format manners.
        /// If the Culture is an invariant <see cref="CultureInfo"/>, 
        /// SpecificCulture will also return an invariant <see cref="CultureInfo"/>.
        /// </summary>
        public CultureInfo SpecificCulture
        {
            get { return CultureInfo.CreateSpecificCulture(this.Culture.ToString()); }
        }

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

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> Culture.
        /// Only supported at DesignTime.
        /// If its in Runtime, <see cref="LocalizeDictionary"/>.Culture will be returned.
        /// </summary>
        /// <param name="obj">The dependency object to get the design culture from.</param>
        /// <returns>The design culture at design time or the current culture at runtime.</returns>
        [DesignOnly(true)]
        public static string GetDesignCulture(DependencyObject obj)
        {
            if (Instance.GetIsInDesignMode())
            {
                return (string) obj.GetValue(DesignCultureProperty);
            }
            else
            {
                return Instance.Culture.ToString();
            }
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
                if (!Instance.GetIsInDesignMode())
                {
                    throw new ArgumentNullException("inKey");
                }
            }
        }

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

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> Culture.
        /// Only supported at DesignTime.
        /// </summary>
        /// <param name="obj">The dependency object to set the culture to.</param>
        /// <param name="value">The odds format.</param>
        [DesignOnly(true)]
        public static void SetDesignCulture(DependencyObject obj, string value)
        {
            if (Instance.GetIsInDesignMode())
            {
                obj.SetValue(DesignCultureProperty, value);
            }
        }

        /// <summary>
        /// Attach an WeakEventListener to the <see cref="LocalizeDictionary"/>
        /// </summary>
        /// <param name="listener">The listener to attach</param>
        public void AddEventListener(IWeakEventListener listener)
        {
            // calls AddListener from the inline WeakCultureChangedEventManager
            WeakCultureChangedEventManager.AddListener(listener);
        }

        /// <summary>
        /// Returns the <see cref="AssemblyName"/> of the passed assembly instance
        /// </summary>
        /// <param name="assembly">The Assembly where to get the name from</param>
        /// <returns>The Assembly name</returns>
        public string GetAssemblyName(Assembly assembly)
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
        /// Gets the status of the design mode
        /// </summary>
        /// <returns>TRUE if in design mode, else FALSE</returns>
        public bool GetIsInDesignMode()
        {
            return DesignerProperties.GetIsInDesignMode(this);
        }

        /// <summary>
        /// Returns an object from the passed dictionary with the given name.
        /// If a wrong <typeparamref name="TType"/> is passed, no exception will get thrown (return obj as <typeparamref name="TType"/>).
        /// </summary>
        /// <typeparam name="TType">Type of result type. Have to be a class.</typeparam>
        /// <param name="resourceAssembly">The Assembly where the Resource is located at</param>
        /// <param name="resourceDictionary">Name of the resource directory</param>
        /// <param name="resourceKey">The key for the resource</param>
        /// <param name="cultureToUse">The culture to use.</param>
        /// <returns>
        /// The founded object or NULL if not found or wrong <typeparamref name="TType"/> is given
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="resourceDictionary"/> is null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="resourceDictionary"/> is empty
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="resourceKey"/> is null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="resourceKey"/> is empty
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Ambiguous resource name {<paramref name="resourceDictionary"/>}
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// No resource with name '{<paramref name="resourceDictionary"/>}' founded
        /// </exception>
        public TType GetLocalizedObject<TType>(
            string resourceAssembly,
            string resourceDictionary,
            string resourceKey,
            CultureInfo cultureToUse) where TType : class
        {
            // Validation
            if (String.IsNullOrEmpty(resourceAssembly))
                return null;

            if (String.IsNullOrEmpty(resourceDictionary))
                return null;

            if (String.IsNullOrEmpty(resourceKey))
                return null;

            // declaring local resource manager
            ResourceManager resManager;

            // try to get the resouce manager
            try
            {
                resManager = this.GetResourceManager(resourceAssembly, resourceDictionary);
            }
            catch
            {
                return null;
            }

            // gets the resourceobject with the choosen localization
            TType retVal = resManager.GetObject(resourceKey, cultureToUse) as TType;

            // if the retVal is null, return null
            if (retVal == null && !this.GetIsInDesignMode())
                return null;

            // finally, return the searched object as type of the generic type
            return retVal;
        }

        /// <summary>
        /// Detach an WeakEventListener to the <see cref="LocalizeDictionary"/>
        /// </summary>
        /// <param name="listener">The listener to detach</param>
        public void RemoveEventListener(IWeakEventListener listener)
        {
            // calls RemoveListener from the inline WeakCultureChangedEventManager
            WeakCultureChangedEventManager.RemoveListener(listener);
        }

        #region ResourceKeyExistsChecks - Obsolete?
        /// <summary>
        /// Looks up the ResourceManagers for the searched <paramref name="resourceKey"/> 
        /// in the <paramref name="resourceDictionary"/> in the <paramref name="resourceAssembly"/>
        /// with an Invariant Culture.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly (e.g.: <c>BaseLocalizeExtension</c>). NULL = Name of the executing assembly</param>
        /// <param name="resourceDictionary">The dictionary to look up (e.g.: ResHelp, Resources, ...). NULL = Name of the default resource file (Resources)</param>
        /// <param name="resourceKey">The key of the searched entry (e.g.: <c>btnHelp</c>, Cancel, ...). NULL = Exception</param>
        /// <returns>
        /// TRUE if the searched one is found, otherwise FALSE
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the ResourceManagers cannot be looked up
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the searched <see cref="ResourceManager"/> wasn't found (only in runtime)
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the <paramref name="resourceKey"/> is null or empty
        /// </exception>
        public bool ResourceKeyExists(string resourceAssembly, string resourceDictionary, string resourceKey)
        {
            return this.ResourceKeyExists(resourceAssembly, resourceDictionary, resourceKey, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Looks up the ResourceManagers for the searched <paramref name="resourceKey"/>
        /// in the <paramref name="resourceDictionary"/> in the <paramref name="resourceAssembly"/>
        /// with the passed culture. If the searched one does not exists with the passed culture, is will searched
        /// until the invariant culture is used.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly (e.g.: <c>BaseLocalizeExtension</c>). NULL = Name of the executing assembly</param>
        /// <param name="resourceDictionary">The dictionary to look up (e.g.: ResHelp, Resources, ...). NULL = Name of the default resource file (Resources)</param>
        /// <param name="resourceKey">The key of the searched entry (e.g.: <c>btnHelp</c>, Cancel, ...). NULL = Exception</param>
        /// <param name="cultureToUse">The culture to use.</param>
        /// <returns>
        /// TRUE if the searched one is found, otherwise FALSE
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the ResourceManagers cannot be looked up
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the searched <see cref="ResourceManager"/> wasn't found (only in runtime)
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the <paramref name="resourceKey"/> is null or empty
        /// </exception>
        public bool ResourceKeyExists(
            string resourceAssembly, string resourceDictionary, string resourceKey, CultureInfo cultureToUse)
        {
            try
            {
                return
                    this.GetResourceManager(resourceAssembly, resourceDictionary).GetObject(
                        resourceKey, cultureToUse) != null;
            }
            catch
            {
                return false;

                // This function should also return false on exceptions as they indicate definitely,
                // that the resource (or assembly or dictionary) don't exist.

                //if (this.GetIsInDesignMode())
                //{
                //    return false;
                //}
                //else
                //{
                //    throw;
                //}
            }
        } 
        #endregion

        #region Dependency Property Callbacks
        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.Culture if set in Xaml.
        /// Only supported at DesignTime.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        [DesignOnly(true)]
        private static void SetCultureFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!Instance.GetIsInDesignMode())
            {
                return;
            }

            CultureInfo culture;

            try
            {
                culture = CultureInfo.GetCultureInfo((string)args.NewValue);
            }
            catch
            {
                if (Instance.GetIsInDesignMode())
                {
                    culture = DefaultCultureInfo;
                }
                else
                {
                    throw;
                }
            }

            if (culture != null)
            {
                Instance.Culture = culture;
            }
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.DefaultDictionary if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetDictionaryFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (Instance.OnCultureChanged != null)
            {
                Instance.OnCultureChanged();
            }
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.DefaultAssembly if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetAssemblyFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (Instance.OnCultureChanged != null)
            {
                Instance.OnCultureChanged();
            }
        } 
        #endregion

        /// <summary>
        /// Looks up in the cached <see cref="ResourceManager"/> list for the searched <see cref="ResourceManager"/>.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly (e.g.: <c>BaseLocalizeExtension</c>). NULL = Name of the executing assembly</param>
        /// <param name="resourceDictionary">The dictionary to look up (e.g.: ResHelp, Resources, ...). NULL = Name of the default resource file (Resources)</param>
        /// <returns>
        /// The founded <see cref="ResourceManager"/>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the ResourceManagers cannot be looked up
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the searched <see cref="ResourceManager"/> wasn't found
        /// </exception>
        private ResourceManager GetResourceManager(string resourceAssembly, string resourceDictionary)
        {
            if (resourceAssembly == null)
            {
                resourceAssembly = this.GetAssemblyName(Assembly.GetExecutingAssembly());
            }

            if (resourceDictionary == null)
            {
                resourceDictionary = ResourcesName;
            }

            PropertyInfo propInfo;
            MethodInfo methodInfo;
            Assembly assembly = null;
            ResourceManager resManager;
            string foundedResource = null;
            string resManagerNameToSearch = "." + resourceDictionary + ResourceFileExtension;
            string[] availableResources;

            if (this.ResourceManagerList.ContainsKey(resourceAssembly + resManagerNameToSearch))
            {
                resManager = this.ResourceManagerList[resourceAssembly + resManagerNameToSearch];
            }
            else
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
                        assembly = Assembly.Load(new AssemblyName(resourceAssembly));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("The Assembly '{0}' cannot be loaded.", resourceAssembly), ex);
                }

                // get all available resourcenames
                // availableResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                availableResources = assembly.GetManifestResourceNames();

                // search for the best fitting resourcefile. pattern: ".{NAME}.resources"
                for (int i = 0; i < availableResources.Length; i++)
                {
                    if (availableResources[i].StartsWith(resourceAssembly + ".") &&
                        availableResources[i].EndsWith(resManagerNameToSearch))
                    {
                        // take the first occurrence and break
                        foundedResource = availableResources[i];
                        break;
                    }
                }

                // if no one was found, exception
                if (foundedResource == null)
                {
                    throw new ArgumentException(
                        string.Format(
                            "No resource manager for dictionary '{0}' in assembly '{1}' founded! ({1}.{0})",
                            resourceDictionary,
                            resourceAssembly));
                }

                // remove ".resources" from the end
                foundedResource = foundedResource.Substring(0, foundedResource.Length - ResourceFileExtension.Length);

                //// Resources.{foundedResource}.ResourceManager.GetObject()
                //// ^^ prop-info      ^^ method get

                try
                {
                    // get the propertyinfo from resManager over the type from foundedResource
                    propInfo = assembly.GetType(foundedResource).GetProperty(ResourceManagerName, ResourceBindingFlags);

                    // get the GET-method from the methodinfo
                    methodInfo = propInfo.GetGetMethod(true);

                    // get the static ResourceManager property
                    object resManObject = methodInfo.Invoke(null, null);
                    
                    // cast it to a ResourceManager for better working with
                    resManager = (ResourceManager) resManObject;
                }
                catch (Exception ex)
                {
                    // this error has to get thrown because this has to work
                    throw new InvalidOperationException("Cannot resolve the ResourceManager!", ex);
                }

                // Add the ResourceManager to the cachelist
                this.ResourceManagerList.Add(resourceAssembly + resManagerNameToSearch, resManager);
            }

            // return the founded ResourceManager
            return resManager;
        }

        #region WeakCultureChangedEventManager
        /// <summary>
        /// This in line class is used to handle weak events to avoid memory leaks
        /// </summary>
        internal sealed class WeakCultureChangedEventManager : WeakEventManager
        {
            /// <summary>
            /// Indicates, if the current instance is listening on the source event
            /// </summary>
            private bool isListening;

            /// <summary>
            /// Holds the inner list of listeners
            /// </summary>
            private ListenerList listeners;

            /// <summary>
            /// Prevents a default instance of the <see cref="WeakCultureChangedEventManager"/> class from being created. 
            /// Creates a new instance of WeakCultureChangedEventManager
            /// </summary>
            private WeakCultureChangedEventManager()
            {
                // creates a new list and assign it to listeners
                this.listeners = new ListenerList();
            }

            /// <summary>
            /// Gets the singleton instance of <see cref="WeakCultureChangedEventManager"/>
            /// </summary>
            private static WeakCultureChangedEventManager CurrentManager
            {
                get
                {
                    // store the type of this WeakEventManager
                    Type managerType = typeof(WeakCultureChangedEventManager);

                    // try to retrieve an existing instance of the stored type
                    WeakCultureChangedEventManager manager = (WeakCultureChangedEventManager)GetCurrentManager(managerType);

                    // if the manager does not exists
                    if (manager == null)
                    {
                        // create a new instance of WeakCultureChangedEventManager
                        manager = new WeakCultureChangedEventManager();

                        // add the new instance to the WeakEventManager manager-store
                        SetCurrentManager(managerType, manager);
                    }

                    // return the new / existing WeakCultureChangedEventManager instance
                    return manager;
                }
            }

            /// <summary>
            /// Adds an listener to the inner list of listeners
            /// </summary>
            /// <param name="listener">The listener to add</param>
            internal static void AddListener(IWeakEventListener listener)
            {
                // add the listener to the inner list of listeners
                CurrentManager.listeners.Add(listener);

                // start / stop the listening process
                CurrentManager.StartStopListening();
            }

            /// <summary>
            /// Removes an listener from the inner list of listeners
            /// </summary>
            /// <param name="listener">The listener to remove</param>
            internal static void RemoveListener(IWeakEventListener listener)
            {
                // removes the listener from the inner list of listeners
                CurrentManager.listeners.Remove(listener);

                // start / stop the listening process
                CurrentManager.StartStopListening();
            }

            /// <summary>
            /// This method starts the listening process by attaching on the source event
            /// </summary>
            /// <param name="source">The source.</param>
            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override void StartListening(object source)
            {
                if (!this.isListening)
                {
                    Instance.OnCultureChanged += this.Instance_OnCultureChanged;
                    this.isListening = true;
                }
            }

            /// <summary>
            /// This method stops the listening process by detaching on the source event
            /// </summary>
            /// <param name="source">The source to stop listening on.</param>
            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override void StopListening(object source)
            {
                if (this.isListening)
                {
                    Instance.OnCultureChanged -= this.Instance_OnCultureChanged;
                    this.isListening = false;
                }
            }

            /// <summary>
            /// This method is called if the <see cref="LocalizeDictionary"/>.OnCultureChanged
            /// is called and the listening process is enabled
            /// </summary>
            private void Instance_OnCultureChanged()
            {
                // tells every listener in the list that the event is occurred
                this.DeliverEventToList(Instance, EventArgs.Empty, this.listeners);
            }

            /// <summary>
            /// This method starts and stops the listening process by attaching/detaching on the source event
            /// </summary>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void StartStopListening()
            {
                // check if listeners are available and the listening process is stopped, start it.
                // otherwise if no listeners are available and the listening process is started, stop it
                if (this.listeners.Count != 0)
                {
                    if (!this.isListening)
                    {
                        this.StartListening(null);
                    }
                }
                else
                {
                    if (this.isListening)
                    {
                        this.StopListening(null);
                    }
                }
            }
        } 
        #endregion
    }
}