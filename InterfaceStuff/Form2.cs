using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InterfaceStuff
{
    public partial class Form2 : Form
    {
        private SerialPort comport = new SerialPort();
        private Semaphore pool;

        public Form2()
        {
            InitializeComponent();
            pool = new Semaphore(0, 1);
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataRecived);
        }

        private void port_DataRecived(object sender, SerialDataReceivedEventArgs e)
        {
            this.Invoke(new EventHandler(AddRecive));
        }

        private void AddRecive(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            byte[] dataR = new byte[comport.BytesToRead];
            comport.Read(dataR, 0, dataR.Length);
        }

        private void initPort()
        {
            if (comport.IsOpen)
            {
                comport.Close();
                System.Threading.Thread.Sleep(100);
            }
            //comport.PortName = portNameTb.Text;
            int baudRate = 9600;
            //надо скорость контрлить?
            comport.BaudRate = baudRate;
            comport.DataBits = 8;
            comport.StopBits = StopBits.One;
            comport.Parity = Parity.None;
            comport.Handshake = Handshake.None;
            comport.ReceivedBytesThreshold = 8;
            comport.WriteBufferSize = 20;
            comport.ReadBufferSize = 20;
            comport.ReadTimeout = -1;
            comport.WriteTimeout = -1;
            comport.DtrEnable = false;
            comport.Open();
            comport.RtsEnable = true;
            System.Threading.Thread.Sleep(100);
            //что нибудь про то что порт открыли
        }

        private void sendData(string command)
        {
            comport.WriteLine(command + (char)0x0D);
        }

        private void DoGraph()
        {
            for(int i = 0; i < 10; i++)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(this.DoGraph);//implicit delegate
            t.Start();
        }
    }
}
