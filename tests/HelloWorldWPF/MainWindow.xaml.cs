using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string language { get; set; }

        public string color { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            language = "de";
            color = "Background";
            this.DataContext = this;

            LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("de");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BindeTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (language != "en")
                language = "en";
            else
                language = "de";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(language)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(color)));
        }
    }
}
