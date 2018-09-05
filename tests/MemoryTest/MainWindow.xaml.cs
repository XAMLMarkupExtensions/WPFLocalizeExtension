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

namespace MemoryTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("en");
        }

        List<Window> windowList = new List<Window>();

        private void OpenWindow_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new TestWindow();
            windowList.Add(wnd);
            
            wnd.Show();
        }

        private void CloseWindows_Click(object sender, RoutedEventArgs e)
        {
            foreach (var wnd in windowList)
                wnd.Close();

            windowList.Clear();
        }

        private void OpenWindowUnlocalized_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new TestWindowUnlocalized();
            windowList.Add(wnd);

            wnd.Show();
        }

        private void GCCollect_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }
    }
}
