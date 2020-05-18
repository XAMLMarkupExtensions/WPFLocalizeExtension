using Caliburn.Micro;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WPFLocalizeExtension;
using WPFLocalizeExtension.Extensions;
using WPFLocalizeExtension.Providers;

namespace WPFLocalizationExtensionDemoApplication.ViewModels.Examples
{
    public class TextLocalizationExampleViewModel : Screen, INotifyPropertyChanged
    {
        private string _resourceKey;

        private string _resourceText;

        public TextLocalizationExampleViewModel()
        {
            base.DisplayName = ResourceKey = "TextLocalizationExample";

            BindPropertyToResource(nameof(ResourceText), "TextLocalizationExample2");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ResourceKey
        {
            get { return _resourceKey; }
            set
            {
                if (value == _resourceKey)
                    return;
                _resourceKey = value;
                OnPropertyChanged();
            }
        }

        public ResourceKeyViewModel ResourceKeyVM { get; set; } = new ResourceKeyViewModel("TextLocalizationExample");

        public string ResourceText
        {
            get { return _resourceText; }
            set
            {
                if (value == _resourceText) return;
                _resourceText = value;
                OnPropertyChanged();
            }
        }

        protected void BindPropertyToResource(string propertyName, string resourceKey)
        {
            var resxLocalizationProvider = ResxLocalizationProvider.Instance;

            var targetProperty = GetType().GetProperty(propertyName);
            var locBinding = new LocTextExtension(resourceKey);

            locBinding.SetBinding(this, targetProperty);
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}