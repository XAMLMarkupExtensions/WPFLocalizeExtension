using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        TestVM vm = new TestVM();

        public MainWindow()
        {
            InitializeComponent();

            vm.language = "de";
            vm.color = "Background";
            this.DataContext = vm;

            LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("de");
        }

        private void BindeTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (vm.language != "en")
                vm.language = "en";
            else
                vm.language = "de";

            if (vm.tenum == TestVM.TestEnum.Test1)
                vm.tenum = TestVM.TestEnum.Test2;
            else
                vm.tenum = TestVM.TestEnum.Test1;
        }
    }
}
