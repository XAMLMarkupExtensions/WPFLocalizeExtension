namespace AssemblyTest
{
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
            this.DataContext = new MyViewModel();
            InitializeComponent();

            //string localisedValue = string.Empty;

            //ILocalizationProvider cvsProvider = new CSVLocalizationProvider() { FileName = "Example", HasHeader = true };
            //LocalizeDictionary.Instance.DefaultProvider = cvsProvider;

            //LocExtension locExtension = new LocExtension();
            //locExtension.Key = "TestText";

            //locExtension.ResolveLocalizedValue<string>(out localisedValue, new CultureInfo("de"));

            //localisedValue = (string)LocalizeDictionary.Instance.GetLocalizedObject("TestText", null, new CultureInfo("de"), cvsProvider);

            //Console.WriteLine(localisedValue);
        }

        private void ButtonAssembly_Click(object sender, RoutedEventArgs e)
        {
            this.SetValue(ResxLocalizationProvider.DefaultAssemblyProperty, "AssemblyTest");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ext = LocExtension.GetBoundExtension(TestObject, "Text", -1);
            MessageBox.Show(ext.ToString());
        }
    }
}
