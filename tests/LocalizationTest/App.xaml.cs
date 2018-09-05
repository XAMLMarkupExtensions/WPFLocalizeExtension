using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using LocalizationTest.Properties;
using WPFLocalizeExtension.Engine;

namespace LocalizationTest
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			InitialiseCultures();
		}

		private static void InitialiseCultures()
		{
			if (!string.IsNullOrEmpty(Settings.Default.Culture))
			{
				LocalizeDictionary.Instance.Culture
					= Thread.CurrentThread.CurrentCulture
					= new CultureInfo(Settings.Default.Culture);
			}

			if (!string.IsNullOrEmpty(Settings.Default.UICulture))
			{
				LocalizeDictionary.Instance.Culture
					= Thread.CurrentThread.CurrentUICulture
					= new CultureInfo(Settings.Default.UICulture);
			}

			FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));
		}
	}
}
