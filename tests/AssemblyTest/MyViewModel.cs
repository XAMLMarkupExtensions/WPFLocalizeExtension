using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using WPFLocalizeExtension.Providers;

namespace AssemblyTest
{
    public class MyViewModel : ViewModelBase
    {
        private ObservableCollection<Item> _items;

        public ObservableCollection<Item> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<Item>();
                    _items.Add(new Item() { TranslationKey = "AssemblyTest:Strings:Tab1" });
                    _items.Add(new Item() { TranslationKey = "AssemblyTest:Strings:Tab2" });
                    _items.Add(new Item() { TranslationKey = "AssemblyTest:Strings:Tab3" });
                }
                return _items;
            }
        }

        private TestEnum enumValue = TestEnum.Processing;
        /// <summary>
        /// Gets or sets the enumValue.
        /// </summary>
        public TestEnum EnumValue
        {
            get { return enumValue; }
            set
            {
                if (enumValue != value)
                {
                    enumValue = value;
                    RaisePropertyChanged("EnumValue");
                }
            }
        }

        public MyViewModel()
        {
            ResxLocalizationProvider.Instance.UpdateCultureList("AssemblyTest", "Strings");
        }
    }

    public class Item : ViewModelBase
    {
        public string DisplayName
        {
            get { return "HOW TO GET TRANSLATED VALUE ?!"; }
        }

        public string TranslationKey { get; set; }
    }
}
