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
using System.Windows.Data;
using System.Globalization;

namespace HalloWeltSL
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            this.DataContext = LocalizeDictionary.Instance;
            InitializeComponent();

            LocalizeDictionary.Instance.Culture = new CultureInfo("de");
        }
    }
}
