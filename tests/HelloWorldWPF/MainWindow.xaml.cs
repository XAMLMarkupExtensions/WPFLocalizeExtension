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
            vm.Hours = 0;
            this.DataContext = vm;
            
            LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("de");

            LocalizeDictionary.Instance.OutputMissingKeys = true;
            LocalizeDictionary.Instance.MissingKeyEvent += Instance_MissingKeyEvent;
        }

        private void Instance_MissingKeyEvent(object sender, MissingKeyEventArgs e)
        {
       
            e.MissingKeyResult = "Hello World";
        }

        private void BindeTestButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Hours = vm.Hours + 1;

            switch (vm.language)
            {
                case "en": vm.language = "de";
                    break;
                case "de":
                    vm.language = "error";
                    break;
                default:
                    vm.language = "en";
                    break;
            }

            if (vm.tenum == TestVM.TestEnum.Test1)
                vm.tenum = TestVM.TestEnum.Test2;
            else
                vm.tenum = TestVM.TestEnum.Test1;
        }
    }
}
