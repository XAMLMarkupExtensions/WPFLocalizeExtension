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
    /// Interaction logic for TestWindowUnlocalized.xaml
    /// </summary>
    public partial class TestWindowUnlocalized : Window
    {
        private static int nextInstanceNumber = 0;

		private int myInstanceNumber;

		public TestWindowUnlocalized()
        {
			InitializeComponent();

			myInstanceNumber = nextInstanceNumber;
			nextInstanceNumber++;

            Debug.WriteLine(string.Format("Creating unlocalized instance {0}", myInstanceNumber));
		}

        ~TestWindowUnlocalized()
		{
            Debug.WriteLine(string.Format("Finalizing unlocalized instance {0}", myInstanceNumber));
		}
    }
}
