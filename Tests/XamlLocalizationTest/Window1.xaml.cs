using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace XamlLocalizationTest
{
    /// <summary>
    /// Implements the Window1.
    /// </summary>
    public partial class Window1 : Window
    {
        private bool toggle = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Window1"/> class.
        /// </summary>
        public Window1()
        {
            this.InitializeComponent();
            this.btnChange.Click += new RoutedEventHandler(this.BtnChange_Click);
            this.btnNewWindow.Click += new RoutedEventHandler(this.BtnNewWindow_Click);
            this.btnCloseChilds.Click += new RoutedEventHandler(this.BtnCloseChilds_Click);
            this.btnCallGC.Click += new RoutedEventHandler(this.BtnCallGC_Click);

            this.Loaded += new RoutedEventHandler(this.Window1_Loaded);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            for (int i = Application.Current.MainWindow.OwnedWindows.Count - 1; i >= 0; i--)
            {
                Application.Current.MainWindow.OwnedWindows[i].Close();
            }

            base.OnClosing(e);
        }

        /// <summary>
        /// Does the binding test.
        /// </summary>
        private void DoBindingTest()
        {
            TextBlock tb = new TextBlock();

            LocTextExtension loc = new LocTextExtension("XamlLocalizationTest:ResTexts:resBack");

            // should be true
            bool success = loc.SetBinding(tb, TextBlock.TextProperty);

            // should be false
            success = loc.SetBinding(tb, TextBlock.TextProperty);

            tb = null;
            tb = new TextBlock();

            // should be true
            success = loc.SetBinding(tb, TextBlock.TextProperty);

            tb = null;
            tb = new TextBlock();

            // should be true
            success = loc.SetBinding(tb, TextBlock.TextProperty);

            // should be false
            success = loc.SetBinding(tb, TextBlock.TextProperty);
        }

        /// <summary>
        /// Handles the Click event of the BtnCallGC control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnCallGC_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            ObjectDependencyManager.CleanUp();
            GC.Collect();
        }

        /// <summary>
        /// Handles the Click event of the BtnChange control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            if (!this.toggle)
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("de-DE");
            }
            else
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("de-AT");
            }

            this.toggle = !this.toggle;
        }

        /// <summary>
        /// Handles the Click event of the BtnCloseChilds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnCloseChilds_Click(object sender, RoutedEventArgs e)
        {
            for (int i = Application.Current.MainWindow.OwnedWindows.Count - 1; i >= 0; i--)
            {
                Application.Current.MainWindow.OwnedWindows[i].Close();
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnNewWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnNewWindow_Click(object sender, RoutedEventArgs e)
        {
            Window2 wnd2 = new Window2();
            wnd2.Owner = this;
            wnd2.Show();

            // wnd2.Close();

            // wnd2 = null;

            // GC.Collect();

            // this.RemoveLogicalChild(uc1);

            // uc1 = new UserControl1();

            // uc1 = null;

            // for (int i = 0; i < 10; i++) {
            // Window2 wnd2 = new Window2();
            // wnd2.Owner = this;
            // wnd2.Show();
            // wnd2.Close();

            // //wnd2 = null;
            // }
        }

        /// <summary>
        /// Handles the Loaded event of the Window1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            // <TextBlock x:Name="txtSizeNormal" Grid.Column="3" Grid.Row="1" Text="{LocText XamlLocalizationTest:ResTexts:resBack, Prefix=AbC, Suffix=AbC, PrefixSpace=true, SuffixSpace=true}" />
            // <TextBlock x:Name="txtSizeUpper" Grid.Column="3" Grid.Row="2" Text="{LocTextUpper XamlLocalizationTest:ResTexts:resBack, Prefix=AbC, Suffix=AbC, PrefixSpace=true, SuffixSpace=true}" />
            // <TextBlock x:Name="txtSizeLower" Grid.Column="3" Grid.Row="3" Text="{LocTextLower XamlLocalizationTest:ResTexts:resBack, Prefix=AbC, Suffix=AbC, PrefixSpace=true, SuffixSpace=true}" />
            bool txtNormalBinding = new LocTextExtension("XamlLocalizationTest:ResTexts:resBack")
                                    {
                                        Prefix = "AbC ", Suffix = " AbC" 
                                    }

                .SetBinding(this.txtSizeNormal, TextBlock.TextProperty);
            bool txtUpperBinding = new LocTextUpperExtension("XamlLocalizationTest:ResTexts:resBack")
                                   {
                                       Prefix = "AbC ", Suffix = " AbC" 
                                   }

                .SetBinding(this.txtSizeUpper, TextBlock.TextProperty);
            bool txtLowerBinding = new LocTextLowerExtension("XamlLocalizationTest:ResTexts:resBack")
                                   {
                                       Prefix = "AbC ", Suffix = " AbC" 
                                   }

                .SetBinding(this.txtSizeLower, TextBlock.TextProperty);

            this.DoBindingTest();

            this.CheckBitmapResolving();
        }

        private void CheckBitmapResolving()
        {
            BitmapSource bmp;
            bool success = new LocImageExtension { Key = "ImageTest", Dict = "ResTexts", Assembly = "XamlLocalizationTest" }.ResolveLocalizedValue(out bmp);
        }
    }
}