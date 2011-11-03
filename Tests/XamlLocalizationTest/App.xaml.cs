using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace XamlLocalizationTest {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        protected override void OnStartup(StartupEventArgs e) {
            
            //System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("de-AT");
            //LocDictionary.Culture = System.Globalization.CultureInfo.GetCultureInfo("de-AT");
            //if (LocDictionary.Culture != null) { }
            base.OnStartup(e);
        }
    }
}
