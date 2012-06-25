#region Copyright information
// <copyright file="ProviderEventArgs.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     http://wpflocalizeextension.codeplex.com/license
// </copyright>
// <author>Uwe Mayer</author>
#endregion

#if WINDOWS_PHONE
namespace WP7LocalizeExtension.Providers
#elif SILVERLIGHT
namespace SLLocalizeExtension.Providers
#else
namespace WPFLocalizeExtension.Providers
#endif
{
    using System;
    using System.Windows;

    /// <summary>
    /// Events arguments for a ProviderChangedEventHandler.
    /// </summary>
    public class ProviderChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The target object.
        /// </summary>
        public DependencyObject Object { get; private set; }

        /// <summary>
        /// Creates a new <see cref="ProviderChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="obj">The target object.</param>
        public ProviderChangedEventArgs(DependencyObject obj)
        {
            this.Object = obj;
        }
    }

    /// <summary>
    /// An event handler for notification of provider changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void ProviderChangedEventHandler(object sender, ProviderChangedEventArgs args);

    /// <summary>
    /// Events arguments for a ProviderErrorEventHandler.
    /// </summary>
    public class ProviderErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The target object.
        /// </summary>
        public DependencyObject Object { get; private set; }

        /// <summary>
        /// The key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// The message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Creates a new <see cref="ProviderErrorEventArgs"/> instance.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="key">The key that caused the error.</param>
        /// <param name="message">The error message.</param>
        public ProviderErrorEventArgs(DependencyObject obj, string key, string message)
        {
            this.Object = obj;
            this.Key = key;
            this.Message = message;
        }
    }

    /// <summary>
    /// An event handler for notification of provider erorrs.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void ProviderErrorEventHandler(object sender, ProviderErrorEventArgs args);

    /// <summary>
    /// Events arguments for a ValueChangedEventHandler.
    /// </summary>
    public class ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// A custom tag.
        /// </summary>
        public object Tag { get; private set; }

        /// <summary>
        /// The new value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// The key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Creates a new <see cref="ValueChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="key">The key where the value was changed.</param>
        /// <param name="value">The new value.</param>
        /// <param name="tag">A custom tag.</param>
        public ValueChangedEventArgs(string key, object value, object tag)
        {
            this.Key = key;
            this.Value = value;
            this.Tag = tag;
        }
    }

    /// <summary>
    /// An event handler for notification of changes of localized values.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs args);
}
