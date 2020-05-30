using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using WPFLocalizeExtension.Providers;

namespace WPFLocalizeExtension.Engine
{
    /// <summary>
    /// A class to hold all global settings
    /// </summary>
    public sealed class LocalizeSettings
    {
        #region Attached Dependency Properties
        /// <summary>
        /// A flag indicating to use singleton or per thread based instance
        /// </summary>
        public static readonly DependencyProperty UseThreadInstancesProperty =
            DependencyProperty.RegisterAttached("UseThreadInstances", typeof(bool), typeof(LocalizeSettings), new PropertyMetadata(Settings.DefaultUseThreadInstances, UseThreadInstancesChangedCallback));

        /// <summary>
        /// <see cref="DependencyProperty"/> to set the localization Culture
        /// </summary>
        public static readonly DependencyProperty CultureProperty =
            DependencyProperty.RegisterAttached("Culture", typeof(CultureInfo), typeof(LocalizeSettings), new PropertyMetadata(null, CultureChangedCallback));

        /// <summary>
        /// A flag indicating that the cache is disabled.
        /// </summary>
        public static readonly DependencyProperty DisableCacheProperty =
            DependencyProperty.RegisterAttached("DisableCache", typeof(bool), typeof(LocalizeSettings), new PropertyMetadata(Settings.DefaultDisableCache, DisableCacheChangedCallback));

        /// <summary>
        /// A flag indicating that the invariant culture should be included.
        /// </summary>
        public static readonly DependencyProperty IncludeInvariantCultureProperty =
            DependencyProperty.RegisterAttached("IncludeInvariantCulture", typeof(bool), typeof(LocalizeSettings), new PropertyMetadata(Settings.DefaultIncludeInvariantCulture, IncludeInvariantCultureChangedCallback));

        /// <summary>
        /// A flag indicating that missing keys should be output.
        /// </summary>
        public static readonly DependencyProperty OutputMissingKeysProperty =
            DependencyProperty.RegisterAttached("OutputMissingKeys", typeof(bool), typeof(LocalizeSettings), new PropertyMetadata(Settings.DefaultOutputMissingKeys, OutputMissingKeysChangedCallback));

        /// <summary>
        /// <see cref="DependencyProperty"/> to set the separation character/string for resource name patterns.
        /// </summary>
        public static readonly DependencyProperty SeparationProperty =
            DependencyProperty.RegisterAttached("Separation", typeof(string), typeof(LocalizeSettings), new PropertyMetadata(Settings.DefaultSeparation, SeparationChangedCallback));

        /// <summary>
        /// <see cref="DependencyProperty"/> to set the default <see cref="ILocalizationProvider"/> type.
        /// </summary>
        public static readonly DependencyProperty LocalizationProviderTypeProperty =
            DependencyProperty.RegisterAttached("LocalizationProviderType", typeof(Type), typeof(LocalizeSettings), new PropertyMetadata(Settings.DefaultLocalizatinProviderType, LocalizationProviderTypeChangedCallback));

        #endregion

        #region Attached Dependency Properties Management
        /// <summary>
        /// Get <see cref="Settings.UseThreadInstances"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="Settings.UseThreadInstances"/></returns>
        public static bool GetUseThreadInstances(DependencyObject obj)
        {
            return Instance.UseThreadInstances;
        }

        /// <summary>
        /// Set <see cref="Settings.UseThreadInstances"/>
        /// </summary>
        /// <param name="obj">not used</param>
        /// <param name="value"></param>
        public static void SetUseThreadInstances(DependencyObject obj, bool value)
        {
            Instance.UseThreadInstances = value;
        }

        /// <summary>
        /// Get <see cref="Settings.Culture"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="Settings.Culture"/></returns>
        public static CultureInfo GetCulture(DependencyObject obj)
        {
            return Instance.Culture;
        }

        /// <summary>
        /// Set <see cref="Settings.Culture"/>
        /// </summary>
        /// <param name="obj">not used</param>
        /// <param name="value"></param>
        public static void SetCulture(DependencyObject obj, CultureInfo value)
        {
            Instance.Culture = value;
        }

        /// <summary>
        /// Get <see cref="Settings.DisableCache"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="Settings.DisableCache"/></returns>
        public static bool GetDisableCache(DependencyObject obj)
        {
            return Instance.DisableCache;
        }

        /// <summary>
        /// Set <see cref="Settings.DisableCache"/>
        /// </summary>
        /// <param name="obj">not used</param>
        /// <param name="value"></param>
        public static void SetDisableCache(DependencyObject obj, bool value)
        {
            Instance.DisableCache = value;
        }

        /// <summary>
        /// Get <see cref="Settings.IncludeInvariantCulture"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="Settings.IncludeInvariantCulture"/></returns>
        public static bool GetIncludeInvariantCulture(DependencyObject obj)
        {
            return (bool)obj.GetValue(IncludeInvariantCultureProperty);
        }

        /// <summary>
        /// Set <see cref="Settings.IncludeInvariantCulture"/>
        /// </summary>
        /// <param name="obj">not used</param>
        /// <param name="value"></param>
        public static void SetIncludeInvariantCulture(DependencyObject obj, bool value)
        {
            obj.SetValue(IncludeInvariantCultureProperty, value);
        }

        /// <summary>
        /// Get <see cref="Settings.OutputMissingKeys"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="Settings.OutputMissingKeys"/></returns>
        public static bool GetOutputMissingKeys(DependencyObject obj)
        {
            return Instance.OutputMissingKeys;
        }

        /// <summary>
        /// Set <see cref="Settings.OutputMissingKeys"/>
        /// </summary>
        /// <param name="obj">not used</param>
        /// <param name="value"></param>
        public static void SetOutputMissingKeys(DependencyObject obj, bool value)
        {
            Instance.OutputMissingKeys = value;
        }

        /// <summary>
        /// Get <see cref="Settings.Separation"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="Settings.Separation"/></returns>
        public static string GetSeparation(DependencyObject obj)
        {
            return Instance.Separation;
        }

        /// <summary>
        /// Set <see cref="Settings.Separation"/>
        /// </summary>
        /// <param name="obj">not used</param>
        /// <param name="value"></param>
        public static void SetSeparation(DependencyObject obj, string value)
        {
            Instance.Separation = value;
        }

        /// <summary>
        /// Get <see cref="Settings.LocalizationProviderType"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="Settings.LocalizationProviderType"/></returns>
        public static Type GetLocalizationProviderType(DependencyObject obj)
        {
            return Instance.LocalizationProviderType;
        }

        /// <summary>
        /// Set <see cref="Settings.LocalizationProviderType"/>
        /// </summary>
        /// <param name="obj">not used</param>
        /// <param name="value"></param>
        public static void SetLocalizationProviderType(DependencyObject obj, Type value)
        {
            Instance.LocalizationProviderType = value;
        }
        #endregion

        #region Attached Dependency Properties Callback
        private static void UseThreadInstancesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool b)
                Instance.UseThreadInstances = b;
        }

        private static void CultureChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is CultureInfo c)
                Instance.Culture = c;
        }

        private static void DisableCacheChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool b)
                Instance.DisableCache = b;
        }

        private static void IncludeInvariantCultureChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool b)
                Instance.IncludeInvariantCulture = b;
        }

        private static void OutputMissingKeysChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool b)
                Instance.OutputMissingKeys = b;
        }

        private static void SeparationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string s)
                Instance.Separation = s;
        }

        private static void LocalizationProviderTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Type t)
                Instance.LocalizationProviderType = t;
        }
        #endregion

        #region Static Properties

        private static Settings _instance;

        /// <summary>
        /// Singleton settings instance
        /// </summary>
        public static Settings Instance
        {
            get
            {
                return _instance ?? (_instance = new Settings());
            }
        }
        #endregion

        #region Settings Class
        /// <summary>
        /// Global settings class
        /// </summary>
        public class Settings
        {
            /// <summary>
            /// protected constructor
            /// </summary>
            internal Settings() { }

            #region Default values
            /// <summary>
            /// The default separation key for <see cref="Separation"/>
            /// </summary>
            public const string DefaultSeparation = "_";

            /// <summary>
            /// Default value for <see cref="UseThreadInstances"/>
            /// </summary>
            public const bool DefaultUseThreadInstances = false;

            /// <summary>
            /// Default value for <see cref="DisableCache"/>
            /// </summary>
            public const bool DefaultDisableCache = false;

            /// <summary>
            /// Default value for <see cref="IncludeInvariantCulture"/>
            /// </summary>
            public const bool DefaultIncludeInvariantCulture = true;

            /// <summary>
            /// Default value for <see cref="OutputMissingKeys"/>
            /// </summary>
            public const bool DefaultOutputMissingKeys = true;

            /// <summary>
            /// Default type for <see cref="LocalizationProviderType"/>
            /// </summary>
            public static readonly Type DefaultLocalizatinProviderType = typeof(ResxLocalizationProvider);
            #endregion

            #region Private Properties
            private CultureInfo _culture;
            private bool _disableCache = DefaultDisableCache;
            private bool _includeInvariantCulture = DefaultIncludeInvariantCulture;
            private bool _outputMissingKeys = DefaultOutputMissingKeys;
            private string _separation = DefaultSeparation;
            private Type _LocalizationProviderType = DefaultLocalizatinProviderType;
            private bool _setCurrentThreadCulture = true;

            #endregion

            #region Public Properties
            /// <summary>
            /// A flag to determine the behaivor of this application on multithreaded applications.
            /// When set to false a singleton instances will be used
            /// otherwise every thread get's his own instance.
            /// </summary>
            public bool UseThreadInstances { get; set; } = DefaultUseThreadInstances;

            /// <summary>
            /// Global culture
            /// </summary>
            public CultureInfo Culture
            {
                get
                {
                    return _culture;
                }
                set
                {
                    if(_culture != value)
                    {
                        _culture = value;

                        foreach (var instance in GetDictionaries())
                            instance.Dispatcher.BeginInvoke(new Action(() => instance.Culture = _culture));
                    }
                }
            }

            /// <summary>
            /// A flag to disable the caching logic
            /// </summary>
            public bool DisableCache
            {
                get
                {
                    return _disableCache;
                }
                set
                {
                    if(_disableCache != value)
                    {
                        _disableCache = value;

                        foreach (var instance in GetDictionaries())
                            instance.Dispatcher.BeginInvoke(new Action(() => instance.DisableCache = _disableCache));
                    }
                }
            }

            /// <summary>
            /// Determines, if the <see cref="LocalizeDictionary.MergedAvailableCultures"/> contains the invariant culture.
            /// </summary>
            public bool IncludeInvariantCulture
            {
                get
                {
                    return _includeInvariantCulture;
                }
                set
                {
                    if (_includeInvariantCulture != value)
                    {
                        _includeInvariantCulture = value;

                        foreach (var instance in GetDictionaries())
                            instance.Dispatcher.BeginInvoke(new Action(() => instance.IncludeInvariantCulture = _includeInvariantCulture));
                    }
                }
            }

            /// <summary>
            /// Determines, if missing keys should be output.
            /// </summary>
            public bool OutputMissingKeys
            {
                get
                {
                    return _outputMissingKeys;
                }
                set
                {
                    if(_outputMissingKeys != value)
                    {
                        _outputMissingKeys = value;

                        foreach (var instance in GetDictionaries())
                            instance.Dispatcher.BeginInvoke(new Action(() => instance.OutputMissingKeys = _outputMissingKeys));
                    }
                }
            }

            /// <summary>
            /// Holds the separation char/string.
            /// </summary>
            public string Separation
            {
                get
                {
                    return _separation;
                }
                set
                {
                    if(_separation != value)
                    {
                        _separation = value;

                        foreach (var instance in GetDictionaries())
                            instance.Dispatcher.BeginInvoke(new Action(() => instance.Separation = _separation));
                    }
                }
            }

            /// <summary>
            /// Holds the default localization provider type
            /// This type has to implementd <see cref="ILocalizationProvider"/>
            /// </summary>
            public Type LocalizationProviderType
            {
                get
                {
                    return _LocalizationProviderType;
                }
                set
                {
                    if (value != null)
                    {
                        if (typeof(ILocalizationProvider).IsAssignableFrom(value))
                            _LocalizationProviderType = value;
                        else
                            throw new NotSupportedException($"The type {value} can't be used as {nameof(LocalizationProviderType)}, it has to implement the interface {nameof(ILocalizationProvider)}!");
                    }
                    else
                        _LocalizationProviderType = DefaultLocalizatinProviderType;
                }
            }

            /// <summary>
            /// Gets or sets a flag that determines, if the CurrentThread culture should be changed along with the Culture property.
            /// </summary>
            public bool SetCurrentThreadCulture
            {
                get
                {
                    return _setCurrentThreadCulture;
                }
                set
                {
                    if(value != _setCurrentThreadCulture)
                    {
                        _setCurrentThreadCulture = value;

                        foreach (var instance in GetDictionaries())
                            instance.Dispatcher.BeginInvoke(new Action(() => instance.SetCurrentThreadCulture = _setCurrentThreadCulture));
                    }
                }
            }
            #endregion

            #region Helper
            private List<LocalizeDictionary> GetDictionaries()
            {
                return InstanceLocator.GetTypeInstances<LocalizeDictionary>().Select(x => x.Value as LocalizeDictionary).ToList();
            }
            #endregion
        }
        #endregion
    }
}
