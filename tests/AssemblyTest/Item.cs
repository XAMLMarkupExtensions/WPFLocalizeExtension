using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyTest
{
    public class Item : ViewModelBase
    {
        public string DisplayName
        {
            get { return "HOW TO GET TRANSLATED VALUE ?!"; }
        }

        public string TranslationKey { get; set; }
    }
}
