using Caliburn.Micro;
using WPFLocalizationExtensionDemoApplication.ViewModels.Examples;

namespace WPFLocalizationExtensionDemoApplication.ViewModels
{
    public class MainViewModel : Conductor<Screen>.Collection.OneActive
    {
        public MainViewModel()
        {
            this.Items.Add(new GapTextWpfExampleViewModel());
            this.Items.Add(new TextLocalizationExampleViewModel());
        }
    }
}