using System.Diagnostics;
using System.Windows;

namespace XamlLocalizationTest
{
    /// <summary>
    /// Implements the Window2.
    /// </summary>
    public partial class Window2 : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Window2"/> class.
        /// </summary>
        public Window2()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Window2"/> is reclaimed by garbage collection.
        /// </summary>
        ~Window2()
        {
            Debug.WriteLine("WINDOW destructed!");
        }
    }
}