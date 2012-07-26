using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

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
