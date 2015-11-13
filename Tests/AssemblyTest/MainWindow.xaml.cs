namespace AssemblyTest
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Documents;
    using WPFLocalizeExtension.Extensions;
    using WPFLocalizeExtension.Providers;
    using XAMLMarkupExtensions.Base;
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ResxLocalizationProvider.Instance.ProviderError += (s, e) =>
            {
                Debug.WriteLine("Missing Key: " + e.Key);
            };

            this.DataContext = new MyViewModel();
            InitializeComponent();

            string localisedValue = string.Empty;

            //ILocalizationProvider cvsProvider = new CSVLocalizationProvider() { FileName = "Example", HasHeader = true };
            //LocalizeDictionary.Instance.DefaultProvider = cvsProvider;

            //LocExtension locExtension = new LocExtension();
            //locExtension.Key = "TestText";

            //locExtension.ResolveLocalizedValue<string>(out localisedValue, new CultureInfo("de"));

            //localisedValue = (string)LocalizeDictionary.Instance.GetLocalizedObject("TestText", null, new CultureInfo("de"), cvsProvider);

            localisedValue = LocExtension.GetLocalizedValue<string>("AssemblyTestResourceLib:Strings:TestText");

            Console.WriteLine(localisedValue);
        }

        private void ButtonAssembly_Click(object sender, RoutedEventArgs e)
        {
            this.SetValue(ResxLocalizationProvider.DefaultAssemblyProperty, "AssemblyTest");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var ext = LocExtension.GetBoundExtension(TestObject, "Text", -1);
            //MessageBox.Show(ext.ToString());
        }

        private static int threadCount = 0;

        private void ButtonMultiThreading_Click(object sender, RoutedEventArgs e)
        {
            Thread newWindowThread = new Thread(new ThreadStart(ThreadStartingPoint));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.Name = "Thread #" + (++threadCount);
            //newWindowThread.IsBackground = true;
            newWindowThread.Start();
            //newWindowThread.Join();
        }

        private void ThreadStartingPoint()
        {
            MainWindow tempWindow = new MainWindow();
            tempWindow.Closed += tempWindow_Closed;
            tempWindow.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

        private void tempWindow_Closed(object sender, EventArgs e)
        {
            (sender as Window).Dispatcher.InvokeShutdown();
        }
    }
}
