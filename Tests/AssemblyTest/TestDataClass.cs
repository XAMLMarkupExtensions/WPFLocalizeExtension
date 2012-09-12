namespace AssemblyTest
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    #endregion

    public class Countries : INotifyPropertyChanged
    {
        #region PropertyChanged Logic
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
        internal void OnNotifyPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion

        #region Properties
        private string country = "";
        public string Country
        {
            get { return country; }
            set { country = value; OnNotifyPropertyChanged("Country"); OnNotifyPropertyChanged("FlagSource"); }
        }

        private string countryDE = "";
        public string CountryDE
        {
            get { return countryDE; }
            set { countryDE = value; OnNotifyPropertyChanged("CountryDE"); }
        }
        
        private double area = 0;
        public double Area
        {
            get { return area; }
            set { area = value; OnNotifyPropertyChanged("Area"); }
        }

        public string FlagSource
        {
            get { return this.country.Replace(' ', '_'); }
            set { }
        }
        #endregion
    }
}
