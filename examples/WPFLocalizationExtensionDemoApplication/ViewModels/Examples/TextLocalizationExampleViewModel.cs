using Caliburn.Micro;

namespace WPFLocalizationExtensionDemoApplication.ViewModels.Examples
{
    public class TextLocalizationExampleViewModel : Screen
    {
        private string _resourceKey;

        public TextLocalizationExampleViewModel()
        {
            base.DisplayName = ResourceKey = "TextLocalizationExample";
        }

        public string ResourceKey
        {
            get { return _resourceKey; }
            set
            {
                if (value == _resourceKey) return;
                _resourceKey = value;
                NotifyOfPropertyChange(() => ResourceKey);
            }
        }
    }
}