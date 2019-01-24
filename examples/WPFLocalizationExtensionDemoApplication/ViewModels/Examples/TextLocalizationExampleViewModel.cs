using Caliburn.Micro;
using WPFLocalizeExtension;

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

        public ResourceKeyViewModel ResourceKeyVM { get; set; } = new ResourceKeyViewModel("TextLocalizationExample");
    }
}