using System;
using System.Windows;

namespace LeakSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnReload(object sender, RoutedEventArgs e)
        {
            ItemsControl.ItemsSource = new object[new Random().Next(10, 30)];
        }
    }
}
