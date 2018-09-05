using System.Windows;

namespace LocalizationTest
{
	/// <summary>
	/// Interaction logic for Loader.xaml
	/// </summary>
	public partial class Loader : Window
	{
		public Loader()
		{
			InitializeComponent();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			var form = new Popup();
			form.ShowDialog();
		}
	}
}
