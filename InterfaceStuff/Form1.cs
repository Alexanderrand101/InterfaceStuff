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
using System.Threading;

namespace InterfaceStuff
{
    public partial class Form1 : Form
    {
        private SerialPort comport = new SerialPort();
        private ManualResetEvent buffFull = new ManualResetEvent(false);
        private byte[] dataBuffer;
        private String[] baudRates = { "9600" };//хз надо ли другие. если попросит добавим

        public Form1()
        {
            InitializeComponent();
            portName.Items.AddRange(SerialPort.GetPortNames());
            portName.SelectedIndex = 0;//ну если ничего нету то вылетаем нафиг.вообще. потом поправлю кароч
            baudRate.Items.AddRange(baudRates);
            baudRate.SelectedIndex = 0;
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataRecived);
        }

        private void port_DataRecived(object sender, SerialDataReceivedEventArgs e)
        {
            dataBuffer = new byte[comport.BytesToRead];
            comport.Read(dataBuffer, 0, dataBuffer.Length);
            for (int i = 0; i < dataBuffer.Length; i++)
                AddInputHistoryMessage(((char)dataBuffer[i]).ToString(), outputEcho);
            comport.DiscardInBuffer();
            buffFull.Set();//должно быть норм.хз чебудет если set на уже открытый эвент вызвать. так то у нас всегда есть while
        }

        //private void AddRecive(object sender, EventArgs e)
        //{
        //    System.Threading.Thread.Sleep(100);
            
            
        //    AddInputHistoryMessage("\n", outputEcho);
        //}

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

        private void writeToPort(String msg)
        {
            comport.WriteLine(msg + (char)0x0D);
            AddInputHistoryMessage(msg + '\n', outputEcho);
            comport.DiscardOutBuffer();
        }

        private void initPort()
        {
            if (comport.IsOpen)
            {
                AddInputHistoryMessage("Порт уже открыт\n", outputEcho);
            }
            else
            {
                comport.PortName = portName.SelectedText;
                comport.BaudRate = int.Parse(baudRate.SelectedText);
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
                AddInputHistoryMessage("Порт открыт\n", outputEcho);
            }
        }

        private void closePort()
        {
            if (comport.IsOpen)
            {
                comport.DiscardOutBuffer();
                comport.DiscardInBuffer();
                comport.Close();
                System.Threading.Thread.Sleep(100);//пишут что сон полезен для здорового общения с портами
                AddInputHistoryMessage("Порт закрыт\n", outputEcho);
            }
            else
            {
                AddInputHistoryMessage("Порт не открыт\n", outputEcho);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string msg = rawInput.Text;
            writeToPort(msg);
            if (!buffFull.WaitOne(500))
            {
                AddInputHistoryMessage("ничего не пришло", outputEcho);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            initPort();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            closePort();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            launch();
        }

        private void launch()
        {
            //найти модули
            //настроить АЦП и ЦАП
            //посчитать параметры
            //высылать и читать че придет анализируя че придет
            //когда перелезем за границу сделать уровень и зажечь лампочку
        }
    }
}
