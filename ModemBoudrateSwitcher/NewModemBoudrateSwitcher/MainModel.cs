using NewModemBoudrateSwitcher.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewModemBoudrateSwitcher
{
    public class MainModel: ViewModel
    {
        private ObservableCollection<string> _ports  = new ObservableCollection<string>();
        public ObservableCollection<string> Ports
        {
            get { return _ports; }
            set { SetProperty(ref _ports, value); }
        }

        private string text = "REE";
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        public MainModel()
        {
            string[] ports = SerialPort.GetPortNames();
        }

    }
}
