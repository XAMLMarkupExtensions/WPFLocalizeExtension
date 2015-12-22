using System.Windows;
using Caliburn.Micro;
using WPFLocalizationExtensionDemoApplication.ViewModels;

namespace WPFLocalizationExtensionDemoApplication.Infrastructure
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }
    }
}
