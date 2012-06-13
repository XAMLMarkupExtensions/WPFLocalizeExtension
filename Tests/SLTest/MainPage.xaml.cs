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

namespace SLTest
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ButtonDE_Click(object sender, RoutedEventArgs e)
        {
            LocalizeDictionary.Instance.Culture = new CultureInfo("de");
        }

        private void ButtonEN_Click(object sender, RoutedEventArgs e)
        {
            LocalizeDictionary.Instance.Culture = new CultureInfo("en");
        }

        private void ButtonAssembly_Click(object sender, RoutedEventArgs e)
        {
            var dict = (string)this.GetValue(LocalizeDictionary.DefaultDictionaryProperty);
            if (dict == "Strings")
                this.SetValue(LocalizeDictionary.DefaultDictionaryProperty, "Strings2");
            else
                this.SetValue(LocalizeDictionary.DefaultDictionaryProperty, "Strings");
        }
    }
}
