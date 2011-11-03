using System.Diagnostics;
using System.Windows.Controls;

namespace XamlLocalizationTest
{
    /// <summary>
    /// Implements the UserControl1.
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserControl1"/> class.
        /// </summary>
        public UserControl1()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="UserControl1"/> is reclaimed by garbage collection.
        /// </summary>
        ~UserControl1()
        {
            Debug.WriteLine("UC destructed!");
        }
    }
}