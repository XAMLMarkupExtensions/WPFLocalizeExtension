#region Copyright information
// <copyright file="LocalizeDictionary.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.Engine
#elif SILVERLIGHT
namespace SLLocalizeExtension.Engine
#else
namespace WPFLocalizeExtension.Engine
#endif
{
    #region Uses
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Markup;
    using System.Windows.Input;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
#if WINDOWS_PHONE
    using WP7LocalizeExtension.Providers;
#else
    using XAMLMarkupExtensions.Base;
#if SILVERLIGHT
    using SLLocalizeExtension.Providers;
#else
    using WPFLocalizeExtension.Providers;
#endif
#endif
    #endregion

    /// <summary>
    /// Represents the culture interface for localization
    /// </summary>
    public sealed class LocalizeDictionary : DependencyObject, INotifyPropertyChanged
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

        #region Dependency Properties
        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultProvider to set the default ILocalizationProvider.
        /// </summary>
        public static readonly DependencyProperty DefaultProviderProperty =
            DependencyProperty.RegisterAttached(
                "DefaultProvider",
                typeof(ILocalizationProvider),
                typeof(LocalizeDictionary),
                new PropertyMetadata(null, SetDefaultProviderFromDependencyProperty));

        /// <summary>
        /// <see cref="DependencyProperty"/> Provider to set the ILocalizationProvider.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.RegisterAttached(
                "Provider",
                typeof(ILocalizationProvider),
                typeof(LocalizeDictionary),
                new PropertyMetadata(null, SetProviderFromDependencyProperty));

        /// <summary>
        /// <see cref="DependencyProperty"/> DesignCulture to set the Culture.
        /// Only supported at DesignTime.
        /// </summary>
#if SILVERLIGHT
#else
        [DesignOnly(true)]
#endif
        public static readonly DependencyProperty DesignCultureProperty =
            DependencyProperty.RegisterAttached(
                "DesignCulture",
                typeof(string),
                typeof(LocalizeDictionary),
                new PropertyMetadata(SetCultureFromDependencyProperty));

        /// <summary>
        /// <see cref="DependencyProperty"/> Separation to set the separation character/string for resource name patterns.
        /// </summary>
        public static readonly DependencyProperty SeparationProperty =
            DependencyProperty.RegisterAttached(
                "Separation",
                typeof(string),
                typeof(LocalizeDictionary),
                new PropertyMetadata(LocalizeDictionary.DefaultSeparation, SetSeparationFromDependencyProperty));

        /// <summary>
        /// <see cref="DependencyProperty"/> Separation to set the separation character/string for resource name patterns.
        /// </summary>
        public static readonly DependencyProperty IncludeInvariantCultureProperty =
            DependencyProperty.RegisterAttached(
                "IncludeInvariantCulture",
                typeof(bool),
                typeof(LocalizeDictionary),
                new PropertyMetadata(true, SetIncludeInvariantCultureFromDependencyProperty));
        #endregion

        #region Dependency Property Callbacks
        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.Culture if set in Xaml.
        /// Only supported at DesignTime.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
#if SILVERLIGHT
#else
        [DesignOnly(true)]
#endif
        private static void SetCultureFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!Instance.GetIsInDesignMode())
                return;

            CultureInfo culture;

            try
            {
                culture = new CultureInfo((string)args.NewValue);
            }
            catch
            {
                if (Instance.GetIsInDesignMode())
                    culture = DefaultCultureInfo;
                else
                    throw;
            }

            if (culture != null)
                Instance.Culture = culture;
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.Provider if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetProviderFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            LocalizeDictionary.DictionaryEvent.Invoke(obj, new DictionaryEventArgs(DictionaryEventType.ProviderChanged, args.NewValue));

            var oldProvider = args.OldValue as ILocalizationProvider;

            if (oldProvider != null)
            {
                oldProvider.ProviderChanged -= new ProviderChangedEventHandler(ProviderUpdated);
                oldProvider.ValueChanged -= new ValueChangedEventHandler(ValueChanged);
                oldProvider.AvailableCultures.CollectionChanged -= new NotifyCollectionChangedEventHandler(Instance.AvailableCulturesCollectionChanged);
            }

            var provider = args.NewValue as ILocalizationProvider;

            if (provider != null)
            {
                provider.ProviderChanged += new ProviderChangedEventHandler(ProviderUpdated);
                provider.ValueChanged += new ValueChangedEventHandler(ValueChanged);
                provider.AvailableCultures.CollectionChanged += new NotifyCollectionChangedEventHandler(Instance.AvailableCulturesCollectionChanged);

                foreach (var c in provider.AvailableCultures)
                    if (!Instance.MergedAvailableCultures.Contains(c))
                        Instance.MergedAvailableCultures.Add(c);
            }
        }

        private static void ProviderUpdated(object sender, ProviderChangedEventArgs args)
        {
            LocalizeDictionary.DictionaryEvent.Invoke(args.Object, new DictionaryEventArgs(DictionaryEventType.ProviderUpdated, sender));
        }

        private static void ValueChanged(object sender, ValueChangedEventArgs args)
        {
            LocalizeDictionary.DictionaryEvent.Invoke(null, new DictionaryEventArgs(DictionaryEventType.ValueChanged, args));
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.DefaultProvider if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetDefaultProviderFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is ILocalizationProvider)
                Instance.DefaultProvider = (ILocalizationProvider)args.NewValue;
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.Separation if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetSeparationFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.IncludeInvariantCulture if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetIncludeInvariantCultureFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool)
                Instance.IncludeInvariantCulture = (bool)args.NewValue;
        }
        #endregion

        #region Dependency Property Management
        #region Get
        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> Provider.
        /// </summary>
        /// <param name="obj">The dependency object to get the provider from.</param>
        /// <returns>The provider.</returns>
        public static ILocalizationProvider GetProvider(DependencyObject obj)
        {
            return (obj != null) ? (ILocalizationProvider)obj.GetValue(ProviderProperty) : null;
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> DefaultProvider.
        /// </summary>
        /// <param name="obj">The dependency object to get the default provider from.</param>
        /// <returns>The default provider.</returns>
        public static ILocalizationProvider GetDefaultProvider(DependencyObject obj)
        {
            return Instance.DefaultProvider;
        }

        /// <summary>
        /// Tries to get the separation from the given target object or of one of its parents.
        /// </summary>
        /// <param name="target">The target object for context.</param>
        /// <returns>The separation of the given context or the default.</returns>
        public static string GetSeparation(DependencyObject target)
        {
            return Instance.Separation;
        }

        /// <summary>
        /// Tries to get the flag from the given target object or of one of its parents.
        /// </summary>
        /// <param name="target">The target object for context.</param>
        /// <returns>The flag.</returns>
        public static bool GetIncludeInvariantCulture(DependencyObject target)
        {
            return Instance.IncludeInvariantCulture;
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> Culture.
        /// Only supported at DesignTime.
        /// If its in Runtime, <see cref="LocalizeDictionary"/>.Culture will be returned.
        /// </summary>
        /// <param name="obj">The dependency object to get the design culture from.</param>
        /// <returns>The design culture at design time or the current culture at runtime.</returns>
#if SILVERLIGHT
#else
        [DesignOnly(true)]
#endif
        public static string GetDesignCulture(DependencyObject obj)
        {
            if (Instance.GetIsInDesignMode())
            {
                return (string)obj.GetValue(DesignCultureProperty);
            }
            else
            {
                return Instance.Culture.ToString();
            }
        }
        #endregion

        #region Set
        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> Provider.
        /// </summary>
        /// <param name="obj">The dependency object to set the provider to.</param>
        /// <param name="value">The provider.</param>
        public static void SetProvider(DependencyObject obj, ILocalizationProvider value)
        {
            obj.SetValue(ProviderProperty, value);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> DefaultProvider.
        /// </summary>
        /// <param name="obj">The dependency object to set the default provider to.</param>
        /// <param name="value">The default provider.</param>
        public static void SetDefaultProvider(DependencyObject obj, ILocalizationProvider value)
        {
            Instance.DefaultProvider = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> Separation.
        /// </summary>
        /// <param name="obj">The dependency object to set the separation to.</param>
        /// <param name="value">The separation.</param>
        public static void SetSeparation(DependencyObject obj, string value)
        {
            Instance.Separation = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> IncludeInvariantCulture.
        /// </summary>
        /// <param name="obj">The dependency object to set the separation to.</param>
        /// <param name="value">The flag.</param>
        public static void SetIncludeInvariantCulture(DependencyObject obj, bool value)
        {
            Instance.IncludeInvariantCulture = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> Culture.
        /// Only supported at DesignTime.
        /// </summary>
        /// <param name="obj">The dependency object to set the culture to.</param>
        /// <param name="value">The odds format.</param>
#if SILVERLIGHT
#else
        [DesignOnly(true)]
#endif
        public static void SetDesignCulture(DependencyObject obj, string value)
        {
            if (Instance.GetIsInDesignMode())
            {
                obj.SetValue(DesignCultureProperty, value);
            }
        }
        #endregion
        #endregion

        #region Variables
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
        /// Holds the separation char/string.
        /// </summary>
        private string separation = LocalizeDictionary.DefaultSeparation;

        /// <summary>
        /// Determines, if the <see cref="LocalizeDictionary.MergedAvailableCultures"/> contains the invariant culture.
        /// </summary>
        private bool includeInvariantCulture = true;

        /// <summary>
        /// A default provider.
        /// </summary>
        private ILocalizationProvider defaultProvider;

#if !WINDOWS_PHONE
        /// <summary>
        /// A dictionary for notification classes for changes of the individual target Parent changes.
        /// </summary>
        private Dictionary<DependencyObject, ParentChangedNotifier> parentNotifiers = new Dictionary<DependencyObject, ParentChangedNotifier>();
#endif
        #endregion

        #region Constructor
        /// <summary>
        /// Prevents a default instance of the <see cref="LocalizeDictionary"/> class from being created.
        /// Static Constructor
        /// </summary>
        private LocalizeDictionary()
        {
#if WINDOWS_PHONE
            this.DefaultProvider = StaticResxLocalizationProvider.Instance;
#else
            this.DefaultProvider = ResxLocalizationProvider.Instance;
#endif
            this.SetCultureCommand = new CultureInfoDelegateCommand(SetCulture);
        }

        private void AvailableCulturesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action<NotifyCollectionChangedEventArgs>((args) =>
            {
                if (args.NewItems != null)
                {
                    foreach (CultureInfo c in args.NewItems)
                    {
                        if (!this.MergedAvailableCultures.Contains(c))
                            this.MergedAvailableCultures.Add(c);
                    }
                }

                if (args.OldItems != null)
                {
                    foreach (CultureInfo c in args.OldItems)
                    {
                        if (this.MergedAvailableCultures.Contains(c))
                            this.MergedAvailableCultures.Remove(c);
                    }
                }
            }), e);
        } 
        #endregion

        #region Static Properties
        /// <summary>
        /// Gets the default <see cref="CultureInfo"/> to initialize the <see cref="LocalizeDictionary"/>.<see cref="CultureInfo"/>
        /// </summary>
        public static CultureInfo DefaultCultureInfo
        {
            get { return CultureInfo.InvariantCulture; }
        }

        /// <summary>
        /// Gets the default separation char/string.
        /// </summary>
        public static string DefaultSeparation
        {
            get { return "_"; }
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
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> for localization.
        /// On set, <see cref="LocalizeDictionary.DictionaryEvent"/> is raised.
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
                    this.culture = DefaultCultureInfo;

                return this.culture;
            }

            set
            {
                // the cultureinfo cannot contain a null reference
                if (value == null)
                    value = DefaultCultureInfo;

                // Let's see if we already got this culture
                var newCulture = value;

                if (!GetIsInDesignMode())
                {
                    foreach (var c in this.MergedAvailableCultures)
                        if (c.Name == value.Name)
                        {
                            newCulture = c;
                            break;
                        }
                        else if (c.Parent.Name == value.Name || value.Parent.Name == c.Name)
                        {
                            // We found a parent culture, but continue - maybe there is a specific one available too.
                            newCulture = c;
                        }
                }

                if (culture != newCulture)
                {
                    if (GetIsInDesignMode() && newCulture != null && !this.MergedAvailableCultures.Contains(newCulture))
                        this.MergedAvailableCultures.Add(newCulture);

                    culture = newCulture;

                    // Raise the OnLocChanged event
                    LocalizeDictionary.DictionaryEvent.Invoke(null, new DictionaryEventArgs(DictionaryEventType.CultureChanged, value));

                    RaisePropertyChanged("Culture");
                }
            }
        }

        /// <summary>
        /// Gets or sets the flag indicating if the invariant culture is included in the <see cref="LocalizeDictionary.MergedAvailableCultures"/> list.
        /// </summary>
        public bool IncludeInvariantCulture
        {
            get { return includeInvariantCulture; }
            set
            {
                if (includeInvariantCulture != value)
                {
                    includeInvariantCulture = value;

                    var c = CultureInfo.InvariantCulture;
                    var existing = this.MergedAvailableCultures.Contains(c);
                    
                    if (includeInvariantCulture && !existing)
                        this.MergedAvailableCultures.Insert(0, c);
                    else if (!includeInvariantCulture && existing)
                        this.MergedAvailableCultures.Remove(c);
                }
            }
        }

        /// <summary>
        /// The separation char for automatic key retrieval.
        /// </summary>
        public string Separation
        {
            get { return separation; }
            set
            {
                separation = value;
                LocalizeDictionary.DictionaryEvent.Invoke(null, new DictionaryEventArgs(DictionaryEventType.SeparationChanged, value));
            }
        }

        /// <summary>
        /// Gets or sets the default <see cref="ILocalizationProvider"/>.
        /// </summary>
        public ILocalizationProvider DefaultProvider
        {
            get { return defaultProvider; }
            set
            {
                if (defaultProvider != value)
                {
                    if (defaultProvider != null)
                    {
                        defaultProvider.ProviderChanged -= new ProviderChangedEventHandler(ProviderUpdated);
                        defaultProvider.ValueChanged += new ValueChangedEventHandler(ValueChanged);
                        defaultProvider.AvailableCultures.CollectionChanged -= new NotifyCollectionChangedEventHandler(AvailableCulturesCollectionChanged);
                    }

                    defaultProvider = value;

                    if (defaultProvider != null)
                    {
                        defaultProvider.ProviderChanged += new ProviderChangedEventHandler(ProviderUpdated);
                        defaultProvider.ValueChanged += new ValueChangedEventHandler(ValueChanged);
                        defaultProvider.AvailableCultures.CollectionChanged += new NotifyCollectionChangedEventHandler(AvailableCulturesCollectionChanged);
                    }
                }
            }
        }

        private ObservableCollection<CultureInfo> mergedAvailableCultures = null;
        /// <summary>
        /// Gets the merged list of all available cultures.
        /// </summary>
        public ObservableCollection<CultureInfo> MergedAvailableCultures
        {
            get
            {
                if (mergedAvailableCultures == null)
                {
                    mergedAvailableCultures = new ObservableCollection<CultureInfo>();
                    mergedAvailableCultures.Add(CultureInfo.InvariantCulture);
                    mergedAvailableCultures.CollectionChanged += (s, e) => { this.Culture = this.Culture; };
                }

                return mergedAvailableCultures;
            }
        }

        /// <summary>
        /// A command for culture changes.
        /// </summary>
        public ICommand SetCultureCommand { get; private set; }

#if SILVERLIGHT
#else
        /// <summary>
        /// Gets the specific <see cref="CultureInfo"/> of the current culture.
        /// This can be used for format manners.
        /// If the Culture is an invariant <see cref="CultureInfo"/>, 
        /// SpecificCulture will also return an invariant <see cref="CultureInfo"/>.
        /// </summary>
        public CultureInfo SpecificCulture
        {
            get
            {
                return CultureInfo.CreateSpecificCulture(this.Culture.ToString());
            }
        } 
#endif
        #endregion

        #region Localization Core
        /// <summary>
        /// Get the localized object using the built-in ResxLocalizationProvider.
        /// </summary>
        /// <param name="source">The source of the dictionary.</param>
        /// <param name="dictionary">The dictionary with key/value pairs.</param>
        /// <param name="key">The key to the value.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public object GetLocalizedObject(string source, string dictionary, string key, CultureInfo culture)
        {
            return GetLocalizedObject(source + ":" + dictionary + ":" + key, null, culture, this.DefaultProvider);
        }
        
        /// <summary>
        /// Get the localized object using the given target for context information.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
#if WINDOWS_PHONE
            var provider = this.DefaultProvider;
#else
#if !SILVERLIGHT
            if (this.DefaultProvider is InheritingResxLocalizationProvider)
                return GetLocalizedObject(key, target, culture, this.DefaultProvider);
#endif
                
            var provider = target != null ? target.GetValueOrRegisterParentNotifier(GetProvider, (obj) => { LocalizeDictionary.DictionaryEvent.Invoke(obj, new DictionaryEventArgs(DictionaryEventType.ProviderChanged, null)); }, parentNotifiers) : null;

            if (provider == null)
                provider = this.DefaultProvider;
#endif

            return GetLocalizedObject(key, target, culture, provider);
        }

        /// <summary>
        /// Get the localized object using the given target and provider.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="culture">The culture to use.</param>
        /// <param name="provider">The provider to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture, ILocalizationProvider provider)
        {
            if (provider == null)
                throw new InvalidOperationException("No provider found and no default provider given.");

            return provider.GetLocalizedObject(key, target, culture);
        }

        /// <summary>
        /// Looks up the ResourceManagers for the searched <paramref name="resourceKey"/> 
        /// in the <paramref name="resourceDictionary"/> in the <paramref name="resourceAssembly"/>
        /// with an Invariant Culture.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly</param>
        /// <param name="resourceDictionary">The dictionary to look up</param>
        /// <param name="resourceKey">The key of the searched entry</param>
        /// <returns>
        /// TRUE if the searched one is found, otherwise FALSE
        /// </returns>
        public bool ResourceKeyExists(string resourceAssembly, string resourceDictionary, string resourceKey)
        {
            return ResourceKeyExists(resourceAssembly, resourceDictionary, resourceKey, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Looks up the ResourceManagers for the searched <paramref name="resourceKey"/>
        /// in the <paramref name="resourceDictionary"/> in the <paramref name="resourceAssembly"/>
        /// with the passed culture. If the searched one does not exists with the passed culture, is will searched
        /// until the invariant culture is used.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly</param>
        /// <param name="resourceDictionary">The dictionary to look up</param>
        /// <param name="resourceKey">The key of the searched entry</param>
        /// <param name="cultureToUse">The culture to use.</param>
        /// <returns>
        /// TRUE if the searched one is found, otherwise FALSE
        /// </returns>
        public bool ResourceKeyExists(string resourceAssembly, string resourceDictionary, string resourceKey, CultureInfo cultureToUse)
        {
#if WINDOWS_PHONE
            var provider = StaticResxLocalizationProvider.Instance;
#else
            var provider = ResxLocalizationProvider.Instance;
#endif

            return ResourceKeyExists(resourceAssembly + ":" + resourceDictionary + ":" + resourceKey, cultureToUse, provider);
        }

        /// <summary>
        /// Looks up the ResourceManagers for the searched <paramref name="key"/>
        /// with the passed culture. If the searched one does not exists with the passed culture, is will searched
        /// until the invariant culture is used.
        /// </summary>
        /// <param name="key">The key of the searched entry</param>
        /// <param name="cultureToUse">The culture to use.</param>
        /// <param name="provider">The localization provider.</param>
        /// <returns>
        /// TRUE if the searched one is found, otherwise FALSE
        /// </returns>
        public bool ResourceKeyExists(string key, CultureInfo cultureToUse, ILocalizationProvider provider)
        {
            return provider.GetLocalizedObject(key, null, cultureToUse) != null;
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Gets the status of the design mode
        /// </summary>
        /// <returns>TRUE if in design mode, else FALSE</returns>
        public bool GetIsInDesignMode()
        {
#if SILVERLIGHT
#else
            if (!this.Dispatcher.Thread.IsAlive)
            {
                return false;
            }

            if (!this.Dispatcher.CheckAccess())
            {
                return (bool)this.Dispatcher.Invoke(new Func<bool>(this.GetIsInDesignMode));
            }
#endif
            return DesignerProperties.GetIsInDesignMode(this);
        }
        #endregion

        #region CultureChangedEvent
        internal static class DictionaryEvent
        {
            /// <summary>
            /// The list of listeners
            /// </summary>
            private static List<WeakReference> listeners = new List<WeakReference>();

            /// <summary>
            /// Fire the event.
            /// </summary>
            /// <param name="sender">The sender of the event.</param>
            /// <param name="args">The event arguments.</param>
            internal static void Invoke(DependencyObject sender, DictionaryEventArgs args)
            {
                List<WeakReference> purgeList = new List<WeakReference>();

                for (int i = 0; i < listeners.Count; i++)
                {
                    WeakReference wr = listeners[i];

                    if (wr.IsAlive)
                        ((IDictionaryEventListener)wr.Target).ResourceChanged(sender, args);
                    else
                        purgeList.Add(wr);
                }

                foreach (WeakReference wr in purgeList)
                    listeners.Remove(wr);

                purgeList.Clear();
            }

            /// <summary>
            /// Adds a listener to the inner list of listeners.
            /// </summary>
            /// <param name="listener">The listener to add.</param>
            internal static void AddListener(IDictionaryEventListener listener)
            {
                if (listener == null)
                    return;

                listeners.Add(new WeakReference(listener));
            }

            /// <summary>
            /// Removes a listener from the inner list of listeners.
            /// </summary>
            /// <param name="listener">The listener to remove.</param>
            internal static void RemoveListener(IDictionaryEventListener listener)
            {
                if (listener == null)
                    return;

                List<WeakReference> purgeList = new List<WeakReference>();

                foreach (WeakReference wr in listeners)
                {
                    if (!wr.IsAlive)
                        purgeList.Add(wr);
                    else if ((IDictionaryEventListener)wr.Target == listener)
                        purgeList.Add(wr);
                }
            }

            /// <summary>
            /// Remove a list of weak references from the list.
            /// </summary>
            /// <param name="purgeList">The list of references to remove.</param>
            private static void Purge(List<WeakReference> purgeList)
            {
                foreach (WeakReference wr in purgeList)
                    listeners.Remove(wr);

                purgeList.Clear();
            }
        }
        #endregion

        #region CultureInfoDelegateCommand
        private void SetCulture(CultureInfo c)
        {
            this.Culture = c;
        }

        /// <summary>
        /// A class for culture commands.
        /// </summary>
        internal class CultureInfoDelegateCommand : ICommand
        {
            #region Functions for execution and evaluation
            /// <summary>
            /// Predicate that determines if an object can execute
            /// </summary>
            private readonly Predicate<CultureInfo> canExecute;

            /// <summary>
            /// The action to execute when the command is invoked
            /// </summary>
            private readonly Action<CultureInfo> execute;
            #endregion

            #region Constructor
            /// <summary>
            /// Initializes a new instance of the <see cref="CultureInfoDelegateCommand"/> class. 
            /// Creates a new command that can always execute.
            /// </summary>
            /// <param name="execute">
            /// The execution logic.
            /// </param>
            public CultureInfoDelegateCommand(Action<CultureInfo> execute)
                : this(execute, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CultureInfoDelegateCommand"/> class. 
            /// Creates a new command.
            /// </summary>
            /// <param name="execute">
            /// The execution logic.
            /// </param>
            /// <param name="canExecute">
            /// The execution status logic.
            /// </param>
            public CultureInfoDelegateCommand(Action<CultureInfo> execute, Predicate<CultureInfo> canExecute)
            {
                if (execute == null)
                {
                    throw new ArgumentNullException("execute");
                }

                this.execute = execute;
                this.canExecute = canExecute;
            }
            #endregion

            #region ICommand interface
            /// <summary>
            /// Occurs when changes occur that affect whether or not the command should execute. 
            /// </summary>
#if SILVERLIGHT
            public event EventHandler CanExecuteChanged;
#else
            public event EventHandler CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
#endif

            /// <summary>
            /// Determines whether the command can execute in its current state.
            /// </summary>
            /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
            /// <returns>true if this command can be executed; otherwise, false.</returns>
            public bool CanExecute(object parameter)
            {
                return this.canExecute == null || this.canExecute((CultureInfo)parameter);
            }

            /// <summary>
            /// Is called when the command is invoked.
            /// </summary>
            /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
            public void Execute(object parameter)
            {
                var c = new CultureInfo((string)parameter);
                this.execute(c);
            }
            #endregion
        } 
        #endregion
    }
}