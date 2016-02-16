namespace GapTextWpfTest
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> WeekDays { get; set; }

        public DateTime OpeningTime { get; set; }

        public DateTime ClosingTime { get; set; }

        public string City { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();

            this.City = "Paderborn";
            this.OpeningTime = new DateTime(1, 1, 1, 09, 30, 00);
            this.ClosingTime = new DateTime(1, 1, 1, 16, 0, 0);
        }
    }
}
