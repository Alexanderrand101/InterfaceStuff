using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace InterfaceStuff
{
    public partial class Form1 : Form
    {
        private SerialPort comport = new SerialPort();

        public Form1()
        {
            InitializeComponent();
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
            for (int i = 0; i < dataR.Length; i++)
                AddInputHistoryMessage(((char)dataR[i]).ToString(), outputEcho);
            AddInputHistoryMessage("\n", outputEcho);
        }

        private void AddInputHistoryMessage(string msg, RichTextBox richTextBox)
        {
            richTextBox.Invoke(
                    new EventHandler(delegate
                    {
                        richTextBox.AppendText(msg);
                        richTextBox.ScrollToCaret();
                    }
                )
            );
        }

        private void sendData(string command)
        {
            comport.WriteLine(command + (char)0x0D);
            AddInputHistoryMessage(command + "\n", inputEcho);
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
            //что нибудь про то что прот открыли
        }
    }
}
