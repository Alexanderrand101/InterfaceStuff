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
using System.Collections;

namespace InterfaceStuff
{
    public partial class Form1 : Form
    {
        private SerialPort comport = new SerialPort();
        private ManualResetEvent buffFull = new ManualResetEvent(false);
        private byte[] dataBuffer;
        private String[] baudRates = { "9600" };//хз надо ли другие. если попросит добавим
        private Dictionary<string, string> devAdr;
        private int defaultWaitTime = 50;

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
            buffFull.Reset();
            writeToPort(msg);
            if (!buffFull.WaitOne(defaultWaitTime * 10))
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

        private bool launch()
        {
            bool result = searchAndIdentify();
            if (!result) return result;
            result = configADC();
            if (!result) return result;
            result = configCDA();
            double[] arguments = new double[10];// а тут пусть сформируется массив сил тока.
            double[] values = new double[10];//запиши сюда свою функцию пусть она массив значений отдаст
            for(int i = 0; i < values.Length; i++)
            {
                result = setADCValue(values[i]);
                if (!result) return result;
                double value = 0;
                result = getCDAValue(6, out value);//на картинке у нас 6ой но хз. мб скажет конфигить
                //тыкай точку на график.
            }
            //когда перелезем за границу сделать уровень и зажечь лампочку
            return true;
        }

        private bool getCDAValue(int channel, out double value)
        {
            writeToPort(readСDAChannelCommand(devAdr["7018"], channel));
            if (buffFull.WaitOne(defaultWaitTime))
            {
                if ((char)dataBuffer[0] == '>')
                {
                    string sValue = "";
                    for (int j = 1; j < dataBuffer.Length; j++)
                        sValue += (char)dataBuffer[j];
                    value = Double.Parse(sValue);
                    AddInputHistoryMessage("C канала " + channel.ToString() + " ЦАП считано значение " 
                        + value.ToString() + "\n", outputEcho);
                    return true;
                }
            }
            AddInputHistoryMessage("Ошибка при чтении значения c канала " + channel.ToString() + " ЦАП\n", outputEcho);
            value = 0;
            return false;
        }

        private string readСDAChannelCommand(string adr, int channel)
        {
            return '#' + adr + channel.ToString();
        }

        private bool setADCValue(double value)
        {
            string val = String.Format("{0,6:F3}", value);
            writeToPort(setADCValueCommand(devAdr["7021"], val));
            if(buffFull.WaitOne(defaultWaitTime))
            {
                if ((char)dataBuffer[0] == '>')
                {
                    AddInputHistoryMessage("На АЦП установлено значение "+ val + "\n", outputEcho);
                    return true;
                }
                if ((char)dataBuffer[0] == '?')
                {
                    //тут надо тред на импульс запустить. но я тут еще не все продумал
                    AddInputHistoryMessage("На АЦП превышен допустимый диапазон " + val + "\n", outputEcho);
                    return true;
                }
            }
            AddInputHistoryMessage("Ошибка при установке значения" + val + "на АЦП\n", outputEcho);
            return false;
        }

        private string setADCValueCommand(string adr, string val)
        {
            return '#' + adr + val;
        }

        private bool configADC()
        {
            writeToPort(config10Command(devAdr["7021"]));
            if (buffFull.WaitOne(defaultWaitTime))
            {
                if((char)dataBuffer[0] == '!')
                {
                    AddInputHistoryMessage("АЦП установлен в режим 10В\n", outputEcho);
                    return true;
                }
            }
            AddInputHistoryMessage("Ошибка при установке АЦП в режим 10В\n", outputEcho);
            return false;
        }

        private bool configCDA()
        {
            //возможно надо как-то сконфигурировать 0 и диапазон. но вроде и так норм было
            writeToPort(turnChannelsOnOff(devAdr["7018"]));
            if (buffFull.WaitOne(defaultWaitTime))
            {
                if ((char)dataBuffer[0] == '!')
                {
                    AddInputHistoryMessage("Входные каналы ЦАП включены\n", outputEcho);
                    return true;
                }
            }
            AddInputHistoryMessage("Ошибка при включнении входных каналов ЦАП\n", outputEcho);
            return false;
        }

        private string turnChannelsOnOff(string adr)
        {
            //может понадобится выбор че включать. а пока включаем все
            return '$' + adr + '5' + "FF"; 
        }

        private string config10Command(string adr)
        {
            return '$' + adr + '7';
        }

        private bool searchAndIdentify(){
            for(int i = 0; i < 256; i++){
                string adr = intToHexString(i);
                buffFull.Reset();
                writeToPort(askNameCommand(adr));
                if (buffFull.WaitOne(defaultWaitTime))
                {
                    if((char)dataBuffer[0] == '!')
                    {
                        string name = "";
                        for (int j = 3; j < dataBuffer.Length; j++)
                            name += (char)dataBuffer[j];
                        devAdr.Add(name, adr);
                    }
                }
                if (devAdr.ContainsKey("7021") && devAdr.ContainsKey("7044") && devAdr.ContainsKey("7018"))
                {
                    AddInputHistoryMessage("Модули найдены\n", outputEcho);
                    return true;
                }
            }
            AddInputHistoryMessage("Модули не найдены\n", outputEcho);
            return false;
        }

        private string askNameCommand(string adr)
        {
            return "$" + adr + "M";
        }

        private string intToHexString(int i)
        {
            char left = bitsToHexChar((i & 0xF0) >> 4);
            char right = bitsToHexChar(i & 0x0F);
            return left.ToString() + right.ToString();
        }

        private char bitsToHexChar(int i)
        {
            return (i < 10) ? (char)(i + '0') : (char)(i - 10 + 'A');
        }
    }
}
