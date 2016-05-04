#region Copyright information
// <copyright file="LocalizeDictionary.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

using System.Windows.Markup;

// Register this namespace one with prefix
[assembly: XmlnsDefinition("http://wpflocalizeextension.codeplex.com", "WPFLocalizeExtension.Engine")]
[assembly: XmlnsDefinition("http://wpflocalizeextension.codeplex.com", "WPFLocalizeExtension.Extensions")]
[assembly: XmlnsDefinition("http://wpflocalizeextension.codeplex.com", "WPFLocalizeExtension.Providers")]
[assembly: XmlnsDefinition("http://wpflocalizeextension.codeplex.com", "WPFLocalizeExtension.TypeConverters")]

// Assign a default namespace prefix for the schema
[assembly: XmlnsPrefix("http://wpflocalizeextension.codeplex.com", "lex")]

namespace WPFLocalizeExtension.Engine
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using Extensions;
    using Providers;
    using XAMLMarkupExtensions.Base;
    using System.Windows.Threading;
    #endregion

    /// <summary>
    /// Represents the culture interface for localization
    /// </summary>
    public sealed class LocalizeDictionary : DependencyObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
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
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
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
        [DesignOnly(true)]
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
        /// A flag indicating that the invariant culture should be included.
        /// </summary>
        public static readonly DependencyProperty IncludeInvariantCultureProperty =
            DependencyProperty.RegisterAttached(
                "IncludeInvariantCulture",
                typeof(bool),
                typeof(LocalizeDictionary),
                new PropertyMetadata(true, SetIncludeInvariantCultureFromDependencyProperty));

        /// <summary>
        /// A flag indicating that the cache is disabled.
        /// </summary>
        public static readonly DependencyProperty DisableCacheProperty =
            DependencyProperty.RegisterAttached(
                "DisableCache",
                typeof(bool),
                typeof(LocalizeDictionary),
                new PropertyMetadata(false, SetDisableCacheFromDependencyProperty));

        /// <summary>
        /// A flag indicating that missing keys should be output.
        /// </summary>
        public static readonly DependencyProperty OutputMissingKeysProperty =
            DependencyProperty.RegisterAttached(
                "OutputMissingKeys",
                typeof(bool),
                typeof(LocalizeDictionary),
                new PropertyMetadata(false, SetOutputMissingKeysFromDependencyProperty));
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

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.DisableCache if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetDisableCacheFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool)
                Instance.DisableCache = (bool)args.NewValue;
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.OutputMissingKeys if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetOutputMissingKeysFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool)
                Instance.OutputMissingKeys = (bool)args.NewValue;
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
            return obj.GetValueSync<ILocalizationProvider>(ProviderProperty);
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
        /// Tries to get the flag from the given target object or of one of its parents.
        /// </summary>
        /// <param name="target">The target object for context.</param>
        /// <returns>The flag.</returns>
        public static bool GetDisableCache(DependencyObject target)
        {
            return Instance.DisableCache;
        }

        /// <summary>
        /// Tries to get the flag from the given target object or of one of its parents.
        /// </summary>
        /// <param name="target">The target object for context.</param>
        /// <returns>The flag.</returns>
        public static bool GetOutputMissingKeys(DependencyObject target)
        {
            return Instance.OutputMissingKeys;
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> DesignCulture.
        /// Only supported at DesignTime.
        /// If its in Runtime, <see cref="LocalizeDictionary"/>.Culture will be returned.
        /// </summary>
        /// <param name="obj">The dependency object to get the design culture from.</param>
        /// <returns>The design culture at design time or the current culture at runtime.</returns>
        [DesignOnly(true)]
        public static string GetDesignCulture(DependencyObject obj)
        {
            if (Instance.GetIsInDesignMode())
                return obj.GetValueSync<string>(DesignCultureProperty);
            else
                return Instance.Culture.ToString();
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
            obj.SetValueSync(ProviderProperty, value);
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
        /// Setter of <see cref="DependencyProperty"/> DisableCache.
        /// </summary>
        /// <param name="obj">The dependency object to set the separation to.</param>
        /// <param name="value">The flag.</param>
        public static void SetDisableCache(DependencyObject obj, bool value)
        {
            Instance.DisableCache = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> OutputMissingKeys.
        /// </summary>
        /// <param name="obj">The dependency object to set the separation to.</param>
        /// <param name="value">The flag.</param>
        public static void SetOutputMissingKeys(DependencyObject obj, bool value)
        {
            Instance.OutputMissingKeys = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> DesignCulture.
        /// Only supported at DesignTime.
        /// </summary>
        /// <param name="obj">The dependency object to set the culture to.</param>
        /// <param name="value">The value.</param>
        [DesignOnly(true)]
        public static void SetDesignCulture(DependencyObject obj, string value)
        {
            if (Instance.GetIsInDesignMode())
                obj.SetValueSync(DesignCultureProperty, value);
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
        /// Determines, if the cache is disabled.
        /// </summary>
        private bool disableCache = true;

        /// <summary>
        /// Determines, if missing keys should be output.
        /// </summary>
        private bool outputMissingKeys = false;

        /// <summary>
        /// A default provider.
        /// </summary>
        private ILocalizationProvider defaultProvider;

        /// <summary>
        /// Determines, if the CurrentThread culture is set along with the Culture property.
        /// </summary>
        private bool setCurrentThreadCulture = true;

        /// <summary>
        /// Determines if the code is run in DesignMode or not.
        /// </summary>
        private bool? _isInDesignMode = null;

        /// <summary>
        /// A dictionary for notification classes for changes of the individual target Parent changes.
        /// </summary>
        private ParentNotifiers parentNotifiers = new ParentNotifiers();
        #endregion

        #region Constructor
        /// <summary>
        /// Prevents a default instance of the <see cref="LocalizeDictionary"/> class from being created.
        /// Static Constructor
        /// </summary>
        private LocalizeDictionary()
        {
            this.DefaultProvider = ResxLocalizationProvider.Instance;
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

                if (!includeInvariantCulture && this.MergedAvailableCultures.Count > 1 && this.MergedAvailableCultures.Contains(CultureInfo.InvariantCulture))
                    this.MergedAvailableCultures.Remove(CultureInfo.InvariantCulture);
            }), e);
        }

        /// <summary>
        /// Destructor code.
        /// </summary>
        ~LocalizeDictionary()
        {
            LocExtension.ClearResourceBuffer();
            FELoc.ClearResourceBuffer();
            BLoc.ClearResourceBuffer();
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

        /// <summary>
        /// Gets the culture of the singleton instance.
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get { return Instance.Culture; }
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
                        if (c == CultureInfo.InvariantCulture && !this.IncludeInvariantCulture)
                            continue;
                        else if (c.Name == value.Name)
                        {
                            newCulture = c;
                            break;
                        }
                        else if (c.Parent.Name == value.Name)
                        {
                            // We found a parent culture, but continue - maybe there is a specific one available too.
                            newCulture = c;
                        }
                        else if (value.Parent.Name == c.Name)
                        {
                            // We found a parent culture, but continue - maybe there is a specific one available too.
                            newCulture = value;
                        }
                }

                if (culture != newCulture)
                {
                    if (newCulture != null && !this.MergedAvailableCultures.Contains(newCulture))
                        this.MergedAvailableCultures.Add(newCulture);

                    culture = newCulture;

                    // Change the CurrentThread culture if needed.
                    if (setCurrentThreadCulture && !this.GetIsInDesignMode())
                    {
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;
                    }
                    
                    // Raise the OnLocChanged event
                    LocalizeDictionary.DictionaryEvent.Invoke(null, new DictionaryEventArgs(DictionaryEventType.CultureChanged, value));

                    RaisePropertyChanged("Culture");
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines, if the CurrentThread culture should be changed along with the Culture property.
        /// </summary>
        public bool SetCurrentThreadCulture
        {
            get { return setCurrentThreadCulture; }
            set
            {
                if (setCurrentThreadCulture != value)
                {
                    setCurrentThreadCulture = value;
                    RaisePropertyChanged("SetCurrentThreadCulture");
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
                    else if (!includeInvariantCulture && existing && this.MergedAvailableCultures.Count > 1)
                        this.MergedAvailableCultures.Remove(c);
                }
            }
        }

        /// <summary>
        /// Gets or sets the flag that disables the cache.
        /// </summary>
        public bool DisableCache
        {
            get { return disableCache; }
            set
            {
                if (disableCache != value)
                {
                    disableCache = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the flag that controls the output of missing keys.
        /// </summary>
        public bool OutputMissingKeys
        {
            get { return outputMissingKeys; }
            set
            {
                if (outputMissingKeys != value)
                {
                    outputMissingKeys = value;
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
                        defaultProvider.ValueChanged -= new ValueChangedEventHandler(ValueChanged);
                        defaultProvider.AvailableCultures.CollectionChanged -= new NotifyCollectionChangedEventHandler(AvailableCulturesCollectionChanged);
                    }

                    defaultProvider = value;

                    if (defaultProvider != null)
                    {
                        defaultProvider.ProviderChanged += new ProviderChangedEventHandler(ProviderUpdated);
                        defaultProvider.ValueChanged += new ValueChangedEventHandler(ValueChanged);
                        defaultProvider.AvailableCultures.CollectionChanged += new NotifyCollectionChangedEventHandler(AvailableCulturesCollectionChanged);

                        foreach (CultureInfo c in defaultProvider.AvailableCultures)
                        {
                            if (!this.MergedAvailableCultures.Contains(c))
                                this.MergedAvailableCultures.Add(c);
                        }
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
            if (this.DefaultProvider is InheritingResxLocalizationProvider)
                return GetLocalizedObject(key, target, culture, this.DefaultProvider);
                
            var provider = target != null ? target.GetValue(GetProvider) : null;

            if (provider == null)
                provider = this.DefaultProvider;

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
        /// Uses the key and target to build a fully qualified resource key (Assembly, Dictionary, Key)
        /// </summary>
        /// <param name="key">Key used as a base to find the full key</param>
        /// <param name="target">Target used to help determine key information</param>
        /// <returns>Returns an object with all possible pieces of the given key (Assembly, Dictionary, Key)</returns>
        public FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(string key, DependencyObject target)
        {
            if (this.DefaultProvider is InheritingResxLocalizationProvider)
                return GetFullyQualifiedResourceKey(key, target, this.DefaultProvider);

            var provider = target != null ? target.GetValue(GetProvider) : null;

            if (provider == null)
                provider = this.DefaultProvider;

            return GetFullyQualifiedResourceKey(key, target, provider);
        }

        /// <summary>
        /// Uses the key and target to build a fully qualified resource key (Assembly, Dictionary, Key)
        /// </summary>
        /// <param name="key">Key used as a base to find the full key</param>
        /// <param name="target">Target used to help determine key information</param>
        /// <param name="provider">Provider to use</param>
        /// <returns>Returns an object with all possible pieces of the given key (Assembly, Dictionary, Key)</returns>
        public FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(String key, DependencyObject target, ILocalizationProvider provider)
        {
            if (provider == null)
                throw new InvalidOperationException("No provider found and no default provider given.");

            return provider.GetFullyQualifiedResourceKey(key, target);
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
            var provider = ResxLocalizationProvider.Instance;

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
            lock (SyncRoot)
            {
                if (_isInDesignMode.HasValue)
                    return _isInDesignMode.Value;

                if (this.Dispatcher == null || this.Dispatcher.Thread == null || !this.Dispatcher.Thread.IsAlive)
                {
                    _isInDesignMode = false;
                    return _isInDesignMode.Value;
                }

                if (!this.Dispatcher.CheckAccess())
                {
                    try
                    {
                        _isInDesignMode = (bool)this.Dispatcher.Invoke(DispatcherPriority.Normal, TimeSpan.FromMilliseconds(100), new Func<bool>(this.GetIsInDesignMode));
                    }
                    catch (Exception)
                    {
                        _isInDesignMode = default(bool);
                    }
                    
                    return _isInDesignMode.Value;
                }
                _isInDesignMode = DesignerProperties.GetIsInDesignMode(this);
                return _isInDesignMode.Value;
            }
        }
        #endregion

        #region MissingKeyEvent (standard event)
        /// <summary>
        /// An event for missing keys.
        /// </summary>
        public event EventHandler<MissingKeyEventArgs> MissingKeyEvent;

        /// <summary>
        /// Triggers a MissingKeyEvent.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="key">The missing key.</param>
        /// <returns>True, if a reload should be performed.</returns>
        internal bool OnNewMissingKeyEvent(object sender, string key)
        {
            var args = new MissingKeyEventArgs(key);

            if (MissingKeyEvent != null)
                MissingKeyEvent(sender, args);

            return args.Reload;
        }
        #endregion

        #region DictionaryEvent (using weak references)
        internal static class DictionaryEvent
        {
            /// <summary>
            /// The list of listeners
            /// </summary>
            private static List<WeakReference> listeners = new List<WeakReference>();
            private static object listenersLock = new object();

            /// <summary>
            /// Fire the event.
            /// </summary>
            /// <param name="sender">The sender of the event.</param>
            /// <param name="args">The event arguments.</param>
            internal static void Invoke(DependencyObject sender, DictionaryEventArgs args)
            {
                var list = new List<IDictionaryEventListener>();

                lock (listenersLock)
                {
                    foreach (var wr in listeners.ToList())
                    {
                        var targetReference = wr.Target;
                        if (targetReference != null)
                            list.Add((IDictionaryEventListener)targetReference);
                        else
                            listeners.Remove(wr);
                    }
                }

                foreach (var item in list)
                    item.ResourceChanged(sender, args);
            }

            /// <summary>
            /// Adds a listener to the inner list of listeners.
            /// </summary>
            /// <param name="listener">The listener to add.</param>
            internal static void AddListener(IDictionaryEventListener listener)
            {
                if (listener == null)
                    return;

                // Check, if this listener already was added.
                bool listenerExists = false;

                lock (listenersLock)
                {
                    foreach (var wr in listeners.ToList())
                    {
                        var targetReference = wr.Target;
                        if (targetReference == null)
                            listeners.Remove(wr);
                        else if (targetReference == listener)
                            listenerExists = true;
                    }

                    // Add it now.
                    if (!listenerExists)
                        listeners.Add(new WeakReference(listener));
                }
            }

            /// <summary>
            /// Removes a listener from the inner list of listeners.
            /// </summary>
            /// <param name="listener">The listener to remove.</param>
            internal static void RemoveListener(IDictionaryEventListener listener)
            {
                if (listener == null)
                    return;

                lock (listenersLock)
                {
                    foreach (var wr in listeners.ToList())
                    {
                        var targetReference = wr.Target;
                        if (targetReference == null)
                            listeners.Remove(wr);
                        else if ((IDictionaryEventListener)targetReference == listener)
                            listeners.Remove(wr);
                    }
                }
            }

            /// <summary>
            /// Enumerates all listeners of type T.
            /// </summary>
            /// <typeparam name="T">The listener type.</typeparam>
            /// <returns>An enumeration of listeners.</returns>
            internal static IEnumerable<T> EnumerateListeners<T>()
            {
                lock (listenersLock)
                {
                    foreach (var wr in listeners.ToList())
                    {
                        var targetReference = wr.Target;
                        if (targetReference == null)
                            listeners.Remove(wr);
                        else if (targetReference is T)
                            yield return (T)targetReference;
                    }
                }
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
