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
using System.Windows.Threading;

namespace NewModemBoudrateSwitcher
{
    public class MainModel : ViewModel
    {
        private readonly string[] ChangeBaudrateCommand = new string[] { "AT+IPR=", "AT" };
        private readonly string[] ChangeConfugurationCommand = new string[] { "AT^SCFG", "AT^SCFG-SYNC", "AT^SLED", "AT" };
        private readonly string[] UpgradeCommand = new string[] { "AT^SCFG", "AT+CFUN-DTR0", "AT" };

        DispatcherTimer timer = new DispatcherTimer();

        private void timer_Tick(object sender, EventArgs e)
        {
            Text += "\nNo Answer From Modem\n";
            index = 0;
            EnableCommands = true;
            timer.Stop();
        }

        List<string> Commands = new List<string>();
        int index = 0;

        #region Properties
        private static SerialPort port;
        private string last_command;
        private string modemanswer = "";

        private ObservableCollection<int> _baudrates = new ObservableCollection<int>() { 9600, 19200, 115200 };
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
            set { SetProperty(ref _customcommand, value); }
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

        private Visibility _showcloseport = Visibility.Collapsed;
        public Visibility ShowClosePort
        {
            get { return _showcloseport; }
            set { SetProperty(ref _showcloseport, value); }
        }

        private bool _enablecommand = true;
        public bool EnableCommands
        {
            get { return _enablecommand; }
            set { SetProperty(ref _enablecommand, value); }
        }

        #endregion

        #region Commands
             private DelegateCommand _clearCommand;
        public DelegateCommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new DelegateCommand((object obj)=> Text = string.Empty, (object obj) => true));
            }
        }

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
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += timer_Tick;
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
                Commands.Clear();
                switch (command)
                {
                    case "custum":
                        Commands.Add(command);
                        break;
                    case "BoudRate Only":
                        Commands.Add("AT");
                        Commands.AddRange(ChangeBaudrateCommand);
                        Commands.Add("AT&W");
                        break;
                    case "BoudRate And Configuration":
                        Commands.Add("AT");
                        Commands.AddRange(ChangeConfugurationCommand);
                        Commands.AddRange(ChangeBaudrateCommand);
                        Commands.Add("AT&W");
                        break;
                    case "Configuration Only":
                        Commands.Add("AT");
                        Commands.AddRange(ChangeConfugurationCommand);
                        Commands.Add("AT&W");
                        break;
                    case "Upgrade":
                        Commands.Add("AT");
                        Commands.AddRange(UpgradeCommand);
                        break;
                }
                if (Commands.Count > 0)
                {
                    index = 0;
                    EnableCommands = false;
                    SendCommand();
                }

            }
        }

        private void SendCommand()
        {
            string command = Commands[index];
            last_command = command;
            modemanswer = "";
            switch (last_command)
            {
                case "AT":
                    Text += $"Sending {command} Command....\n";
                    timer.Start();
                    port.WriteLine(command + "\r");
                    break;
                case "AT+IPR=":
                    Text += $"Sending {command} Command....\n";
                    timer.Start();
                    port.WriteLine(command + SelectedNewBaudRate + "\r");
                    break;
                case "AT&W":
                    Text += $"Sending {command} Command....\n";
                    timer.Start();
                    port.WriteLine(command + "\r");
                    break;
                case "AT^SCFG-SYNC":
                    Text += $"Sending {command} Command....\n";
                    timer.Start();
                    port.WriteLine(command + "=\"GPIO/mode/SYNC\", \"std\"" + "\r");
                    break;
                case "AT^SCFG-DTR0":
                    Text += $"Sending {command} Command....\n";
                    timer.Start();
                    port.WriteLine(command + "=\"GPIO/mode/DTR0\", \"std\"" + "\r");
                    break;
                case "AT^SLED":
                    Thread.Sleep(1000);
                    Text += $"Sending {command} Command....\n";
                    timer.Start();
                    port.WriteLine(command + "=1" + "\r");
                    break;
                case "AT+CFUN":
                    Text += $"Sending {command} Command....\n";
                    timer.Start();
                    port.WriteLine(command + "=1,1" + "\r");
                    break;
                case "custum":
                default:
                    if (string.IsNullOrEmpty(CustomCommand))
                        return;
                    Text += $"Sending {CustomCommand} Command....\n";
                    timer.Start();
                    port.WriteLine(CustomCommand + "\r");
                    break;
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
                    break;
                case "AT+IPR=":
                    ClosePort(null);
                    SelectedBaudRate = SelectedNewBaudRate;
                    OpenPort(null);
                    break;
                case "AT+CFUN":
                    Thread.Sleep(1000);
                    break;
                case "AT^SCFG":
                case "AT^SLED":
                case "AT&W":
                default:
                    break;
            }
            index++;
            if (index < Commands.Count)
            {
                SendCommand();
            }
            else
            {
                index = 0;
                EnableCommands = true;
            }
        }

        private bool CheckAnswer(string modemanswer, out string answer)
        {
            answer = modemanswer.Replace("\r", "");
            if (modemanswer.Contains("OK"))
            {
                timer.Stop();
                return true;
            }
            else if (modemanswer.Contains("ERROR"))
            {
                Text += answer;
                modemanswer = "";
                timer.Stop();
                return false;
            }
            return false;

        }
    }
}
