using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace HalloWeltWPF
{
    public class TestVM : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        /// <summary>
        /// Informiert über sich ändernde Eigenschaften.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify that a property has changed
        /// </summary>
        /// <param name="property">
        /// The property that changed
        /// </param>
        internal void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        public enum TestEnum
        {
            Test1,
            Test2
        }

        private TestEnum _tenum;
        public TestEnum tenum {
            get => _tenum;
            set {
                _tenum = value;
                RaisePropertyChanged(nameof(tenum));
            }
        }

        private int _hours;
        public int Hours
        {
            get => _hours;
            set
            {
                _hours = value;
                RaisePropertyChanged(nameof(Hours));
            }
        }

        private string _language;
        public string language
        {
            get => _language;
            set
            {
                _language = value;
                RaisePropertyChanged(nameof(language));
            }
        }

        private string _color;
        public string color {
            get => _color;
            set {
                _color = value;
                RaisePropertyChanged(nameof(color));
            }
        }
    }
}
