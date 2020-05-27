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
using WPFLocalizeExtension.Engine;

namespace HalloWeltWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int Hours
        {
            get
            {
                return 1;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("de");
        }
    }
}
