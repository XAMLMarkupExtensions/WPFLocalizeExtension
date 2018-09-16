#region Copyright and License Information

// Fluent Ribbon Control Suite
// http://fluent.codeplex.com/
// Copyright © Degtyarev Daniel, Rikker Serg., Weegen Patrick 2009-2013.  All rights reserved.
// 
// Distributed under the terms of the Microsoft Public License (Ms-PL). 
// The license is available online http://fluent.codeplex.com/license

#endregion

using System.Globalization;
using WPFLocalizeExtension.Engine;
namespace Fluent.Sample.Foundation
{
    // ATTENTION: You need use Fluent.RibbonWindow. 
    // RibbonWindow designed to provide proper office-like glass style.
    // RibbonWindow automatically will use special non-DWM style in case of
    // Windows XP or basic Windows 7/Vista theme. 
    // You still can use usual System.Windows.Window

    /// <summary>
    /// Represents the main window of the application
    /// </summary>
    public partial class Window : RibbonWindow
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Window()
        {
            InitializeComponent();
        }

        private void buttonLanguage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CultureInfo newCulture;
            if (LocalizeDictionary.Instance.Culture.Name.Contains("de"))
            {
                newCulture = CultureInfo.GetCultureInfo("en-US");
            }
            else
            {
                newCulture = CultureInfo.GetCultureInfo("de");
            }

            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;            
            LocalizeDictionary.Instance.Culture = newCulture;
        }
    }
}
