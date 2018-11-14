using NewModemBoudrateSwitcher.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewModemBoudrateSwitcher
{
    public class MainModel: ViewModel
    {
        private string text = "REE";
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

    }
}
