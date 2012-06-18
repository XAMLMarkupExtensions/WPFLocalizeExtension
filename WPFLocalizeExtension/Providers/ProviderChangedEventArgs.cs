#if SILVERLIGHT
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

        public ProviderChangedEventArgs(DependencyObject obj)
        {
            this.Object = obj;
        }
    }

    public delegate void ProviderChangedEventHandler(object sender, ProviderChangedEventArgs args);
}
