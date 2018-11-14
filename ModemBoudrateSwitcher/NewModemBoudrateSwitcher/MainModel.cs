using NewModemBoudrateSwitcher.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NewModemBoudrateSwitcher
{
    public class MainModel : ViewModel
    {

        private static SerialPort port;
        private string last_command;
        private string modemanswer = "";

        private ObservableCollection<int> _baudrates = new ObservableCollection<int>() { 9600,19200, 115200 };
        public ObservableCollection<int> BaudRates
        {
            get { return _baudrates; }
            set { SetProperty(ref _baudrates, value); }
        }

        private int _selectedbaudrate = 115200;
        public int SelectedBaudRate
        {
            get { return _selectedbaudrate; }
            set { SetProperty(ref _selectedbaudrate, value); }
        }

        private int _selectednewbaudrate = 9600;
        public int SelectedNewBaudRate
        {
            get { return _selectednewbaudrate; }
            set { SetProperty(ref _selectednewbaudrate, value); TestCommand.RaiseCanExecuteChanged(); }
        }

        private ObservableCollection<string> _ports = new ObservableCollection<string>();
        public ObservableCollection<string> Ports
        {
            get { return _ports; }
            set { SetProperty(ref _ports, value); }
        }

        private string _selectedport;
        public string SelectedPort
        {
            get { return _selectedport; }
            set { SetProperty(ref _selectedport, value); }
        }

        private string _customcommand;
        public string CustomCommand
        {
            get { return _customcommand; }
            set { SetProperty(ref _customcommand, value);}
        }

        private string text = "";
        public string Text
        {
            get { return text; }
            set { SetProperty(ref text, value); }
        }

        private bool _enablecombos = true;
        public bool EnableCombos
        {
            get { return _enablecombos; }
            set { SetProperty(ref _enablecombos, value); }
        }

        private Visibility _showextraactions = Visibility.Collapsed;
        public Visibility ShowExtraActions
        {
            get { return _showextraactions; }
            set { SetProperty(ref _showextraactions, value); }
        }

        private Visibility _showcloseport = Visibility.Collapsed;
        public Visibility ShowClosePort
        {
            get { return _showcloseport; }
            set { SetProperty(ref _showcloseport, value); }
        }

        private Visibility _showsave = Visibility.Collapsed;
        public Visibility ShowSave
        {
            get { return _showsave; }
            set { SetProperty(ref _showsave, value); }
        }

        #region Commands
        private DelegateCommand _openCommand;
        public DelegateCommand OpenCommand
        {
            get
            {
                return _openCommand ?? (_openCommand = new DelegateCommand(OpenPort, (object obj) => true));
            }
        }

        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new DelegateCommand(ClosePort, (object obj) => true));
            }
        }

        private DelegateCommand _testCommand;
        public DelegateCommand TestCommand
        {
            get
            {
                return _testCommand ?? (_testCommand = new DelegateCommand(SendFunc, (object obj) => true));
            }
        }

        #endregion

        public MainModel()
        {
            ReadPorts();
        }

        private void ReadPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Ports.Clear();
            SelectedPort = null;
            foreach (var item in ports.OrderBy(x => x))
            {
                Ports.Add(item);
            }
            string lastport = Properties.Settings.Default.LastPort;
            SelectedPort = Ports.Where(x => x == lastport).FirstOrDefault() ?? Ports.FirstOrDefault();
        }

        private void ClosePort(object obj)
        {
            port.Close();
            Text += "Closing Port\n";
            Thread.Sleep(1000);
            Text += $"Closed\n";
            ShowClosePort = port.IsOpen ? Visibility.Visible : Visibility.Collapsed;
            ShowExtraActions = port.IsOpen ? Visibility.Visible : Visibility.Collapsed;
            EnableCombos = port.IsOpen ? false : true;
        }

        private void OpenPort(object obj)
        {
            Properties.Settings.Default.LastPort = SelectedPort;
            Properties.Settings.Default.Save();
            port = new SerialPort();
            port.PortName = SelectedPort;
            port.BaudRate = SelectedBaudRate;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.Handshake = Handshake.None;
            port.Parity = Parity.None;
            port.DtrEnable = true;
            port.RtsEnable = true;
            port.WriteTimeout = 1000;
            port.DataReceived += Port_DataReceived;
            Text += $"Openning Port {SelectedPort}, BoudRate {SelectedBaudRate}....\n";
            port.Open();
            Text += $"Opened\n";
            ShowClosePort = port.IsOpen ? Visibility.Visible : Visibility.Collapsed;
            EnableCombos = port.IsOpen ? false : true;
        }

        private void SendFunc(object obj)
        {
            if (obj is string command)
            {
                last_command = command;
                modemanswer = "";
                switch (command)
                {
                    case "AT":
                        ShowExtraActions = Visibility.Collapsed;
                        Text += $"Sending {command} Command....\n";
                        port.WriteLine(command + "\r");
                        break;
                    case "AT+IPR=":
                        Text += $"Sending {command} Command....\n";
                        port.WriteLine(command + SelectedNewBaudRate + "\r");
                        break;
                    case "ATW":
                        SendFunc("AT&W");
                        break;
                    case "AT&W":
                        Text += $"Sending {command} Command....\n";
                        port.WriteLine(command + "\r");
                        break;
                    case "AT^SCFG":
                        Text += $"Sending {command} Command....\n";
                        port.WriteLine(command + "=\"GPIO/mode/SYNC\", \"std\"" + "\r");
                        break;
                    case "AT^SLED":
                        Text += $"Sending {command} Command....\n";
                        port.WriteLine(command + "=1" + "\r");
                        break;
                    case "AT+CFUN":
                        Text += $"Sending {command} Command....\n";
                        port.WriteLine(command + "=1" + "\r");
                        break;
                    case "custum":
                    default:
                        if (string.IsNullOrEmpty(CustomCommand))
                            return;
                        Text += $"Sending {CustomCommand} Command....\n";
                        port.WriteLine(CustomCommand + "\r");
                        break;
                }
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            modemanswer += port.ReadExisting();
            if (!CheckAnswer(modemanswer, out string answer))
                return;
            Text += answer;
            modemanswer = "";
            switch (last_command)
            {
                case "AT":
                    ShowExtraActions = Visibility.Visible;
                    break;
                case "AT+IPR=":
                    ClosePort(null);
                    SelectedBaudRate = SelectedNewBaudRate;
                    OpenPort(null);
                    ShowSave = Visibility.Visible;
                    break;
                case "AT^SCFG":
                    SendFunc("AT+CFUN");
                    break;
                case "AT+CFUN":
                    SendFunc("AT^SLED");
                    break;
                case "AT^SLED":
                    ShowSave = Visibility.Visible;
                    break;
                case "AT&W":
                default:
                    break;
            }
        }

        private bool CheckAnswer(string modemanswer, out string answer)
        {
            answer = modemanswer.Replace("\r","");
            if (modemanswer.Contains("OK"))
            {
                return true;
            }
            else if (modemanswer.Contains("ERROR"))
            {
                Text += answer;
                modemanswer = "";
                return false;
            }
            return false;

        }
    }
}
