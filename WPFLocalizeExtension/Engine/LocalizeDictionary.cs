#region Copyright information
// <copyright file="LocalizeDictionary.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

#if SILVERLIGHT
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
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using XAMLMarkupExtensions.Base;
#if SILVERLIGHT
    using SLLocalizeExtension.Extensions;
    using SLLocalizeExtension.Providers;
#else
    using WPFLocalizeExtension.Extensions;
    using WPFLocalizeExtension.Providers;
#endif
    #endregion

    /// <summary>
    /// Represents the culture interface for localization
    /// </summary>
    public sealed class LocalizeDictionary : DependencyObject
    {
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
            LocalizeDictionary.CultureChangedEvent.Invoke(obj, EventArgs.Empty);

            var provider = args.NewValue as ILocalizationProvider;

            if (provider != null)
            {
                provider.ProviderChanged += (s, e) => { LocalizeDictionary.CultureChangedEvent.Invoke(e.Object, EventArgs.Empty); };
                provider.AvailableCultures.CollectionChanged += new NotifyCollectionChangedEventHandler(Instance.AvailableCulturesCollectionChanged);

                foreach (var c in provider.AvailableCultures)
                    if (!Instance.MergedAvailableCultures.Contains(c))
                        Instance.MergedAvailableCultures.Add(c);
            }
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
            string sep = null;

            if (args.NewValue is string)
                sep = (string)args.NewValue;

            if (sep != null)
                Instance.Separation = sep;
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
        /// Getter of <see cref="DependencyProperty"/> provider.
        /// </summary>
        /// <param name="obj">The dependency object to get the provider from.</param>
        /// <returns>The provider.</returns>
        public static ILocalizationProvider GetProvider(DependencyObject obj)
        {
            return (obj != null) ? (ILocalizationProvider)obj.GetValue(ProviderProperty) : null;
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
        /// Setter of <see cref="DependencyProperty"/> provider.
        /// </summary>
        /// <param name="obj">The dependency object to set the provider to.</param>
        /// <param name="value">The provider.</param>
        public static void SetProvider(DependencyObject obj, ILocalizationProvider value)
        {
            obj.SetValue(ProviderProperty, value);
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

        /// <summary>
        /// A dictionary for notification classes for changes of the individual target Parent changes.
        /// </summary>
        private Dictionary<DependencyObject, ParentChangedNotifier> parentNotifiers = new Dictionary<DependencyObject, ParentChangedNotifier>();
        #endregion

        #region Constructor
        /// <summary>
        /// Prevents a default instance of the <see cref="LocalizeDictionary"/> class from being created.
        /// Static Constructor
        /// </summary>
        private LocalizeDictionary()
        {
            this.DefaultProvider = ResxLocalizationProvider.Instance;
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

        #region Events and Actions
        ///// <summary>
        ///// Get raised if the <see cref="LocalizeDictionary"/>.Culture is changed.
        ///// </summary>
        //internal event Action<DependencyObject> OnLocChanged; 
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
        /// On set, <see cref="LocalizeDictionary.CultureChangedEvent"/> is raised.
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
                    throw new ArgumentNullException("value");

                // Set the CultureInfo
                this.culture = value;

                // Raise the OnLocChanged event
                LocalizeDictionary.CultureChangedEvent.Invoke(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the separation char/string for resource name patterns.
        /// On set, <see cref="LocalizeDictionary.CultureChangedEvent"/> is raised.
        /// </summary>
        public string Separation
        {
            get
            {
                if (separation == null)
                    separation = DefaultSeparation;

                return separation;
            }

            set
            {
                // the separation cannot contain a null reference
                if (value == null)
                    throw new ArgumentNullException("value");

                // Set the separation
                this.separation = value;

                // Raise the OnLocChanged event
                LocalizeDictionary.CultureChangedEvent.Invoke(null, EventArgs.Empty);
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
                        defaultProvider.ProviderChanged -= (s, e) => { LocalizeDictionary.CultureChangedEvent.Invoke(e.Object, EventArgs.Empty); };
                        defaultProvider.AvailableCultures.CollectionChanged -= new NotifyCollectionChangedEventHandler(AvailableCulturesCollectionChanged);
                    }

                    defaultProvider = value;

                    if (defaultProvider != null)
                    {
                        defaultProvider.ProviderChanged += (s, e) => { LocalizeDictionary.CultureChangedEvent.Invoke(e.Object, EventArgs.Empty); };
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
                }

                return mergedAvailableCultures;
            }
        }

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
            var provider = target != null ? target.GetValueOrRegisterParentNotifier(GetProvider, (obj) => { LocalizeDictionary.CultureChangedEvent.Invoke(obj, EventArgs.Empty); }, parentNotifiers) : null;

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
        internal static class CultureChangedEvent
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
            internal static void Invoke(DependencyObject sender, EventArgs args)
            {
                List<WeakReference> purgeList = new List<WeakReference>();

                for (int i = 0; i < listeners.Count; i++)
                {
                    WeakReference wr = listeners[i];

                    if (wr.IsAlive)
                        ((LocExtension)wr.Target).ResourceChanged(sender, args);
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
            internal static void AddListener(LocExtension listener)
            {
                if (listener == null)
                    return;

                listeners.Add(new WeakReference(listener));
            }

            /// <summary>
            /// Removes a listener from the inner list of listeners.
            /// </summary>
            /// <param name="listener">The listener to remove.</param>
            internal static void RemoveListener(LocExtension listener)
            {
                if (listener == null)
                    return;

                List<WeakReference> purgeList = new List<WeakReference>();

                foreach (WeakReference wr in listeners)
                {
                    if (!wr.IsAlive)
                        purgeList.Add(wr);
                    else if ((LocExtension)wr.Target == listener)
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
    }
}