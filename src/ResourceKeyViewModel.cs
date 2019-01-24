using System.ComponentModel;
using System.Windows;
using WPFLocalizeExtension.Engine;

namespace WPFLocalizeExtension
{
    /// <summary>
    /// Wrapper around a string that raises <see cref="INotifyPropertyChanged"/>
    /// when the language changes or when the <see cref="ResourceKey"/> changes.
    /// </summary>
    public class ResourceKeyViewModel : IDictionaryEventListener, INotifyPropertyChanged
    {
        private static PropertyChangedEventArgs ResourceKeyEventArgs = new PropertyChangedEventArgs(nameof(ResourceKey));

        public ResourceKeyViewModel(string resourceKey)
        {
            ResourceKey = resourceKey;

            // do not need to remove listener, for LocalizeDictionary keeps weak references
            LocalizeDictionary.DictionaryEvent.AddListener(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _resourceKey;

        public string ResourceKey
        {
            get { return _resourceKey; }
            set
            {
                if (_resourceKey != value)
                {
                    _resourceKey = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void ResourceChanged(DependencyObject sender, DictionaryEventArgs e)
        {
            RaisePropertyChanged();
        }

        /// <summary>
        /// Notify that a property has changed
        /// </summary>
        /// <param name="property">The property that changed</param>
        private void RaisePropertyChanged()
        {
            PropertyChanged?.Invoke(this, ResourceKeyEventArgs);
        }
    }
}