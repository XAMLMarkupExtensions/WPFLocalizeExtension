using System.Diagnostics;
using System.Windows;

namespace XamlLocalizationTest
{
    /// <summary>
    /// Implements the ResTextsKeyValueWindow.
    /// </summary>
    public partial class ResTextsKeyValueWindow : Window
    {
        /// <summary>
      /// Initializes a new instance of the <see cref="ResTextsKeyValueWindow"/> class.
        /// </summary>
        public ResTextsKeyValueWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ResTextsKeyValueWindow"/> is reclaimed by garbage collection.
        /// </summary>
        ~ResTextsKeyValueWindow()
        {
            Debug.WriteLine("WINDOW destructed!");
        }
    }
}