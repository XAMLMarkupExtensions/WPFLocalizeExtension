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
using SLLocalizeExtension.Engine;
using System.Globalization;
using SLLocalizeExtension.Providers;

namespace SLTest
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            this.DataContext = LocalizeDictionary.Instance;
            InitializeComponent();
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var dict = (string)this.GetValue(ResxLocalizationProvider.DefaultDictionaryProperty);
            if (dict == "Strings")
                this.SetValue(ResxLocalizationProvider.DefaultDictionaryProperty, "Strings2");
            else
                this.SetValue(ResxLocalizationProvider.DefaultDictionaryProperty, "Strings");
        }
    }
}
