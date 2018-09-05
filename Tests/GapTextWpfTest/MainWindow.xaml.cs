namespace GapTextWpfTest
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;

    public enum WeekDay
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday,
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises a new <see cref="E:INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<string> weekDays = null;
        /// <summary>
        /// Gets a list of week days.
        /// </summary>
        public ObservableCollection<string> WeekDays
        {
            get
            {
                if (weekDays == null)
                    weekDays = new ObservableCollection<string>() { "Mo", "Di", "Mi", "Do", "Fr", "Sa", "So" };
                return weekDays;
            }
        }

        private DateTime openingTime = new DateTime(1, 1, 1, 9, 30, 0); 
        /// <summary>
        /// Gets or sets the opening time.
        /// </summary>
        public DateTime OpeningTime
        {
            get { return openingTime; }
            set
            {
                if (openingTime != value)
                {
                    openingTime = value;
                    RaisePropertyChanged(nameof(OpeningTime));
                }
            }
        }

        private DateTime closingTime = new DateTime(1, 1, 1, 16, 0, 0);
        /// <summary>
        /// Gets or sets the closing time.
        /// </summary>
        public DateTime ClosingTime
        {
            get { return closingTime; }
            set
            {
                if (closingTime != value)
                {
                    closingTime = value;
                    RaisePropertyChanged("ClosingTime");
                }
            }
        }

        private string city = "Paderborn";
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City
        {
            get { return city; }
            set
            {
                if (city != value)
                {
                    city = value;
                    RaisePropertyChanged("City");
                }
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TestTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Debug.WriteLine("Test");
        }
    }
}
