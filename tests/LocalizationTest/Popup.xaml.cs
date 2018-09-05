using System.Diagnostics;
using System.Windows;

namespace LocalizationTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class Popup : Window
	{
		private static int nextInstanceNumber = 0;

		private int myInstanceNumber;

		public Popup()
		{
			InitializeComponent();

			myInstanceNumber = nextInstanceNumber;
			nextInstanceNumber++;

			Debug.WriteLine(string.Format("Creating instance {0}", myInstanceNumber));
		}

		~Popup()
		{
			Debug.WriteLine(string.Format("Finalizing instance {0}", myInstanceNumber));
		}
	}
}
