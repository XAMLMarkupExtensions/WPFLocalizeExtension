using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

namespace MemoryTest
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        private static int nextInstanceNumber = 0;

		private int myInstanceNumber;

		public TestWindow()
        {
			InitializeComponent();

			myInstanceNumber = nextInstanceNumber;
			nextInstanceNumber++;

			Debug.WriteLine(string.Format("Creating localized instance {0}", myInstanceNumber));
		}

        ~TestWindow()
		{
            Debug.WriteLine(string.Format("Finalizing localized instance {0}", myInstanceNumber));
		}
    }
}
