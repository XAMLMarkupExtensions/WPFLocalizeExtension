using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace HalloWeltWP
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            this.DataContext = WP7LocalizeExtension.Engine.LocalizeDictionary.Instance;
            InitializeComponent();

            WP7LocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("de");

            WP7LocalizeExtension.Providers.StaticResxLocalizationProvider.Instance.ProviderError += new WP7LocalizeExtension.Providers.ProviderErrorEventHandler(Instance_ProviderError);
        }

        void Instance_ProviderError(object sender, WP7LocalizeExtension.Providers.ProviderErrorEventArgs args)
        {
            Console.WriteLine(args.Message);
        }
    }
}