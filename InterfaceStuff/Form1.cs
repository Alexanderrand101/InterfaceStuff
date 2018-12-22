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
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;

namespace InterfaceStuff
{
    public partial class Form1 : Form
    {
        private PortInterface comport = new VirtualPort();
        private ManualResetEvent buffFull = new ManualResetEvent(false);
        private volatile String dataBuffer;
        private String[] baudRates = { "9600" };//хз надо ли другие. если попросит добавим
        private Dictionary<string, string> devAdr = new Dictionary<string, string>();
        private int defaultWaitTime = 700;
        private bool signalOn = false;
        private double bound = 2.5;
        private int amountOfPoints;
        private double b;
        private double a;
        private Stopwatch stpwtch = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
            portName.Items.AddRange(SerialPort.GetPortNames());
            portName.SelectedIndex = 0;//ну если ничего нету то вылетаем нафиг.вообще. потом поправлю кароч
            baudRate.Items.AddRange(baudRates);
            baudRate.SelectedIndex = 0;
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataRecived);
            exponGraph.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            button1.VisibleChanged += Button1_EnabledChanged;
            checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
            
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!comport.IsOpen)
            {
                button1.Visible = false;
                rawInput.ReadOnly = true;
                checkBox1.Checked = false;
            }
        }

        private void Button1_EnabledChanged(object sender, EventArgs e)
        {
            if (!comport.IsOpen) button1.Visible = false;
        }

        private void port_DataRecived(object sender, SerialDataReceivedEventArgs e)
        {
            dataBuffer = "";
            char curSymb = (char)comport.ReadChar();
            while (curSymb != (char)0x0D)
            {
                dataBuffer += curSymb;
                curSymb = (char)comport.ReadChar();
            }
            comport.DiscardOutBuffer();
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
            //AddInputHistoryMessage(msg + '\n', outputEcho);
            comport.DiscardInBuffer();
        }

        private void initPort()
        {
            if (comport.IsOpen)
            {
                AddInputHistoryMessage("Порт уже открыт\n", outputEcho);
            }
            else
            {
                comport.PortName = portName.SelectedItem.ToString();
                comport.BaudRate = int.Parse(baudRate.SelectedItem.ToString());
                comport.DataBits = 8;
                comport.StopBits = StopBits.One;
                comport.Parity = Parity.None;
                comport.Handshake = Handshake.None;
                comport.ReceivedBytesThreshold = 1;
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
            listBox1.Items.Insert(0, msg);
            if (buffFull.WaitOne(defaultWaitTime))
            {
                string recived = "";
                for (int j = 0; j < dataBuffer.Length; j++)
                    recived += (char)dataBuffer[j];
                AddInputHistoryMessage(recived + '\n', outputEcho);                
            }
            else
            {
                AddInputHistoryMessage("ничего не пришло\n", outputEcho);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            initPort();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            closePort();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            lockForm();
            Thread thread = new Thread(new ThreadStart(launch));
            thread.Start();
        }

        private void lockForm()
        {
            button4.Enabled = false;
            button1.Enabled = false;
            button3.Enabled = false;
            button2.Enabled = false;
            checkBox1.Enabled = false;
            amountOfPoints = (int)numericUpDown4.Value;
            a = (double)numericUpDown1.Value;
            b = (double)numericUpDown2.Value;
            exponGraph.Series[0].Points.Clear();
            impuls.Series[0].Points.Clear();
            pictureBox1.BackColor = Color.Red;
            stpwtch.Reset();
            stpwtch.Start();
            timer2.Interval = (int)numericUpDown3.Value;
            timer1.Start();
        }

        private void unlockForm()
        {
            button4.Enabled = true;
            button1.Enabled = true;
            button3.Enabled = true;
            button2.Enabled = true;
            checkBox1.Enabled = true;
        }

        private void launch()
        {
            bool result = searchAndIdentify();
            if (!result) return;
            result = configCDA();
            double[ ,] arr = new double[amountOfPoints + 1, 2];
            double step = (Math.Log(bound/a)/b) / (amountOfPoints - 1);
            for(int i = 0; i < amountOfPoints; i++)
            {
                arr[i, 0] = step * i;
                arr[i, 1] = a * Math.Exp(step * i * b);
            }
            for(int i = 0; i < amountOfPoints; i++)
            {
                result = setADCValue(arr[i, 1]);
                if (!result) return;
                System.Threading.Thread.Sleep(40);
                double value = 0;
                result = getCDAValue(6, out value);
                this.Invoke(new EventHandler(delegate { exponGraph.Series[0].Points.AddXY(arr[i, 0], value); }));
            }
            this.Invoke(new EventHandler(delegate { unlockForm(); }));
        }

        private bool getCDAValue(int channel, out double value)
        {
            buffFull.Reset();
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
            string val = String.Format("{0,6:00.000}", value);
            buffFull.Reset();
            writeToPort(setADCValueCommand(devAdr["7021"], val));
            if(buffFull.WaitOne(defaultWaitTime))
            {
                if ((char)dataBuffer[0] == '>')
                {
                    AddInputHistoryMessage("На АЦП установлено значение "+ val + "\n", outputEcho);
                    if (value >= 2.5 - 0.00005){
                        buffFull.Reset();
                        writeToPort(turnOnChannel(devAdr["7044"], 7));
                        if (buffFull.WaitOne(defaultWaitTime))
                        {
                            if ((char)dataBuffer[0] == '>')
                            {
                                AddInputHistoryMessage("Лампочка включена\n", outputEcho);
                                this.Invoke(new EventHandler(delegate { pictureBox1.BackColor = Color.Green; }));
                            }
                            else
                            {
                                AddInputHistoryMessage("Лампочка не включена\n", outputEcho);
                            }
                        }
                        else
                        {
                            AddInputHistoryMessage("Лампочка не включена\n", outputEcho);
                        }
                        buffFull.Reset();
                        writeToPort(turnOnChannel(devAdr["7044"], 6));
                        if (buffFull.WaitOne(defaultWaitTime))
                        {
                            if ((char)dataBuffer[0] == '>')
                            {
                                AddInputHistoryMessage("Уровень установлен\n", outputEcho);
                                this.Invoke(new EventHandler(delegate {
                                    impuls.Series[0].Points.AddXY(stpwtch.ElapsedMilliseconds, 0);
                                    impuls.Series[0].Points.AddXY(stpwtch.ElapsedMilliseconds, 1);
                                    signalOn = true;
                                    timer2.Start();
                                }));
                            }
                            else
                            {
                                AddInputHistoryMessage("Уровень не установлен\n", outputEcho);
                            }
                        }
                        else
                        {
                            AddInputHistoryMessage("Уровень не установлен\n", outputEcho);
                        }
                        timer2.Start();
                    }
                    return true;
                }
                //if ((char)dataBuffer[0] == '?')
                //{
                //    if (!signalOn)
                //    {
                //        signalOn = true;
                //        Thread thread = new Thread(new ThreadStart(formSignal));
                //        thread.Start();
                //    }
                //    AddInputHistoryMessage("На АЦП превышен допустимый диапазон " + val + "\n", outputEcho);
                //    return true;
                //}
            }
            AddInputHistoryMessage("Ошибка при установке значения" + val + "на АЦП\n", outputEcho);
            return false;
        }

        private string setADCValueCommand(string adr, string val)
        {
            return '#' + adr + val;
        }

        //private bool configADC()
        //{
        //    semaphore.WaitOne();
        //    buffFull.Reset();
        //    writeToPort(config10Command(devAdr["7021"]));
        //    if (buffFull.WaitOne(defaultWaitTime))
        //    {
        //        if((char)dataBuffer[0] == '!')
        //        {
        //            AddInputHistoryMessage("АЦП установлен в режим 10В\n", outputEcho);
        //            semaphore.Release();
        //            return true;
        //        }
        //    }
        //    semaphore.Release();
        //    AddInputHistoryMessage("Ошибка при установке АЦП в режим 10В\n", outputEcho);
        //    return false;
        //}

        private bool configCDA()
        {
            //возможно надо как-то сконфигурировать 0 и диапазон. но вроде и так норм было
            buffFull.Reset();
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

        private void formSignal()
        {
            buffFull.Reset();
            writeToPort(turnOnChannel(devAdr["7044"], 7));
            if (buffFull.WaitOne(defaultWaitTime))
            {
                if ((char)dataBuffer[0] == '>')
                {
                    AddInputHistoryMessage("Лампочка включена\n", outputEcho);
                }
                else
                {
                    AddInputHistoryMessage("Лампочка не включена\n", outputEcho);
                }
            }
            else
            {
                AddInputHistoryMessage("Лампочка не включена\n", outputEcho);
            }

            buffFull.Reset();
            writeToPort(turnOnChannel(devAdr["7044"], 6));
            if (buffFull.WaitOne(defaultWaitTime))
            {
                if ((char)dataBuffer[0] == '>')
                {
                    AddInputHistoryMessage("Уровень установлен\n", outputEcho);
                }
                else
                {
                    AddInputHistoryMessage("Уровень не установлен\n", outputEcho);
                }
            }
            else
            {
                AddInputHistoryMessage("Уровень не установлен\n", outputEcho);
            }
            System.Threading.Thread.Sleep(5000);
            buffFull.Reset();
            writeToPort(turnOffChannel(devAdr["7044"], 6));
            if (buffFull.WaitOne(defaultWaitTime))
            {
                if ((char)dataBuffer[0] == '>')
                {
                    AddInputHistoryMessage("Уровень сброшен\n", outputEcho);
                }
                else
                {
                    AddInputHistoryMessage("Уровень не сброшен\n", outputEcho);
                }
            }
            else
            {
                AddInputHistoryMessage("Уровень не сброшен\n", outputEcho);
            }
            signalOn = false;
        }

        private string config10Command(string adr)
        {
            return '$' + adr + '7';
        }

        private string turnOnChannel(string adr, int channel)
        {
            return '$' + adr + '0' + channel.ToString() + "01";
        }

        private string turnOffChannel(string adr, int channel)
        {
            return '$' + adr + '0' + channel.ToString() + "00";
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
                        string recived = "";
                        for (int j = 3; j < dataBuffer.Length; j++)
                            name += (char)dataBuffer[j];
                        devAdr.Add(name, adr);
                        AddInputHistoryMessage("found device " + name + " with adr " + adr +  '\n', outputEcho);
                        for (int j = 0; j < dataBuffer.Length; j++)
                            recived += (char)dataBuffer[j];
                        AddInputHistoryMessage(recived + '\n', outputEcho);

                    }
                }
                System.Threading.Thread.Sleep(100);
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

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                button1.Visible = true;
                rawInput.ReadOnly = false;
            }
            else
            {
                button1.Visible = false;
                rawInput.ReadOnly = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            impuls.Series[0].Points.AddXY(stpwtch.ElapsedMilliseconds, signalOn ? 1 : 0);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            buffFull.Reset();
            writeToPort(turnOffChannel(devAdr["7044"], 6));
            if (buffFull.WaitOne(defaultWaitTime))
            {
                if ((char)dataBuffer[0] == '>')
                {
                    AddInputHistoryMessage("Уровень сброшен\n", outputEcho);
                    impuls.Series[0].Points.AddXY(stpwtch.ElapsedMilliseconds, 1);
                    impuls.Series[0].Points.AddXY(stpwtch.ElapsedMilliseconds, 0);
                    signalOn = false;
                }
                else
                {
                    AddInputHistoryMessage("Уровень не сброшен\n", outputEcho);
                }
            }
            else
            {
                AddInputHistoryMessage("Уровень не сброшен\n", outputEcho);
            }
            
            timer2.Stop();
            timer1.Stop();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                rawInput.Text = listBox1.Text;
        }
    }
}
