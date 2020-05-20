using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ThreadPerformance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void BtnOpenWindow_Click(object sender, RoutedEventArgs e)
        {
            var thread = new Thread(threadStart);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void threadStart()
        {
            var window = new MainView();
            window.Closed += Window_Closed;
            window.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            (sender as DispatcherObject).Dispatcher.InvokeShutdown();
        }

        private void BtnOpenWindow2_Click(object sender, RoutedEventArgs e)
        {
            var window = new MainView();
            window.Show();
        }

        private void BtnGC_Click(object sender, RoutedEventArgs e)
        {
            System.GC.Collect();
        }
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Tabs = new ObservableCollection<TabModel>();
            for(int i = 0; i < 2; i++)
            {
                Tabs.Add(new TabModel() { Name = Guid.NewGuid().ToString() });
            }

            SelectedTab = Tabs.FirstOrDefault();
        }

        private ObservableCollection<TabModel> _Tabs;

        public ObservableCollection<TabModel> Tabs
        {
            get { return _Tabs; }
            set { _Tabs = value; RaisePropertyChanged(nameof(Tabs)); }
        }

        private TabModel _SelectedTab;

        public TabModel SelectedTab
        {
            get { return _SelectedTab; }
            set { _SelectedTab = value; RaisePropertyChanged(nameof(SelectedTab)); }
        }


        public class TabModel : ViewModelBase
        {
            private string _Name;

            public string Name
            {
                get { return _Name; }
                set { _Name = value; RaisePropertyChanged(nameof(Name)); }
            }

            private ObservableCollection<ItemModel> _Items;

            public ObservableCollection<ItemModel> Items
            {
                get { return _Items; }
                set { _Items = value; RaisePropertyChanged(nameof(Items)); }
            }


            public TabModel()
            {
                Items = new ObservableCollection<ItemModel>();
                for(int i = 0; i < 15; i++)
                {
                    Items.Add(new ItemModel() { Name = Guid.NewGuid().ToString() });
                }
            }


            public class ItemModel : ViewModelBase
            {
                private string _Name;

                public string Name
                {
                    get { return _Name; }
                    set { _Name = value; RaisePropertyChanged(nameof(Name)); }
                }

                private ObservableCollection<EntrieModel> _Entries;

                public ObservableCollection<EntrieModel> Entries
                {
                    get { return _Entries; }
                    set { _Entries = value; RaisePropertyChanged(nameof(Entries)); }
                }

                public ItemModel()
                {
                    Entries = new ObservableCollection<EntrieModel>();
                    for(int i = 0; i < 15; i++)
                    {
                        Entries.Add(new EntrieModel() { Name = Guid.NewGuid().ToString() });
                    }
                }

                public class EntrieModel : ViewModelBase
                {
                    private string _Name;

                    public string Name
                    {
                        get { return _Name; }
                        set { _Name = value; RaisePropertyChanged(nameof(Name)); }
                    }
                }
            }
        }
    }
}
