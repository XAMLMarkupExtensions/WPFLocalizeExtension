using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Providers;
using ProviderExample;
using WPFLocalizeExtension.Extensions;

namespace AssemblyTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = new MyViewModel();
            InitializeComponent();

            //string localisedValue = string.Empty;

            //ILocalizationProvider cvsProvider = new CSVLocalizationProvider() { FileName = "Example", HasHeader = true };
            //LocalizeDictionary.Instance.DefaultProvider = cvsProvider;

            //LocExtension locExtension = new LocExtension();
            //locExtension.Key = "TestText";

            //locExtension.ResolveLocalizedValue<string>(out localisedValue, new CultureInfo("de"));

            //localisedValue = (string)LocalizeDictionary.Instance.GetLocalizedObject("TestText", null, new CultureInfo("de"), cvsProvider);

            //Console.WriteLine(localisedValue);
        }

        private void ButtonAssembly_Click(object sender, RoutedEventArgs e)
        {
            this.SetValue(ResxLocalizationProvider.DefaultAssemblyProperty, "AssemblyTest");
        }
    }
}
