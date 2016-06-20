using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModemBoudrateSwitcher
{
    class Program
    {
        private static bool allDone;
        private static SerialPort port;
        static string selectedport = "";

        const int boudfrom = 115200;
        const int boudTo = 9600;

        static void Main(string[] args)
        {
            string[] ports = SerialPort.GetPortNames();
            while (ports.Length == 0)
            {
                Console.WriteLine("Please Connect the modem and press ENTER");
                ConsoleKeyInfo info = Console.ReadKey();
                if (info.Key == ConsoleKey.Enter)
                {
                    ports = SerialPort.GetPortNames();
                }
                else if (info.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine();
                }
            }
            if (ports.Length > 1)
            {
                Console.WriteLine("Please Select a Port:");
                for (int i = 0; i < ports.Length; i++)
                {
                    Console.WriteLine("{1} {0}", ports[i], i + 1);
                }
                Console.WriteLine("---------------------------------");
                Console.ReadLine();

            }
            else
                selectedport = ports[0];

            port = new SerialPort();
            port.PortName = selectedport;
            port.BaudRate = boudfrom;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.Handshake = Handshake.None;
            port.Parity = Parity.None;
            port.DataReceived += Port_DataReceived;
            Console.WriteLine("Openning Port {0}, BoudRate {1}....", selectedport, boudfrom);
            port.Open();
            port.WriteLine("AT" + "\r");
            Thread.Sleep(1000);
            while (!allDone)
            {

            }
            Thread.Sleep(5000);
        }

        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string modemanswer = port.ReadExisting();
            Console.WriteLine(modemanswer);
            if (modemanswer.Contains("OK") && modemanswer.Contains("AT+IPR"))
            {
                CloseAndReSend();
            }
            else if (modemanswer.Contains("OK") && modemanswer.Contains("AT"))
            {
                SentBoudrate();
            }
        }

        private static void CloseAndReSend()
        {
            port.Close();
            Console.WriteLine("Closing the port");
            Thread.Sleep(1000);
            port = new SerialPort();
            port.PortName = selectedport;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.Handshake = Handshake.None;
            port.Parity = Parity.None;
            port.DataReceived += Port_DataReceived1;
            port.BaudRate = boudTo;
            Console.WriteLine("Openning Port {0}, BoudRate {1}....", selectedport, boudTo);
            port.Open();
            Thread.Sleep(1000);
            port.WriteLine("AT&W" + "\r");
            Thread.Sleep(1000);
        }

        private static void Port_DataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            string modemanswer = port.ReadExisting();
            Console.WriteLine(modemanswer);
            if (modemanswer.Contains("OK"))
            {
                allDone = true;
            }
        }

        private static void SentBoudrate()
        {
            port.WriteLine("AT+IPR=9600" + "\r");
            Thread.Sleep(1000);
        }
    }
}
