using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceStuff
{
    class ComPortWrap : PortInterface
    {
        private SerialPort port;

        public ComPortWrap()
        {
            port = new SerialPort();
            port.DataReceived += new SerialDataReceivedEventHandler(rethrow_DataRecived);
        }

        public string PortName { get => port.PortName; set => port.PortName = value; }
        public int BaudRate { get => port.BaudRate; set => port.BaudRate = value; }
        public int DataBits { get => port.DataBits; set => port.DataBits = value; }
        public StopBits StopBits { get => port.StopBits; set => port.StopBits = value; }
        public Parity Parity { get => port.Parity; set => port.Parity = value; }
        public Handshake Handshake { get => port.Handshake; set => port.Handshake = value; }
        public int ReceivedBytesThreshold { get => port.ReceivedBytesThreshold; set => port.ReceivedBytesThreshold = value; }
        public int WriteBufferSize { get => port.WriteBufferSize; set => port.WriteBufferSize = value; }
        public int ReadBufferSize { get => port.ReadBufferSize; set => port.ReadBufferSize = value; }
        public int ReadTimeout { get => port.ReadTimeout; set => port.ReadTimeout = value; }
        public bool DtrEnable { get => port.DtrEnable; set => port.DtrEnable = value; }
        public bool RtsEnable { get => port.RtsEnable; set => port.RtsEnable = value; }
        public int WriteTimeout { get => port.WriteTimeout; set => port.WriteTimeout = value; }
        public bool IsOpen { get => port.IsOpen; }

        public event SerialDataReceivedEventHandler DataReceived;

        private void rethrow_DataRecived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceived(sender, e);
        }

        public void Close()
        {
            port.Close();
        }

        public void DiscardInBuffer()
        {
            port.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            port.DiscardOutBuffer();
        }

        public void Open()
        {
            port.Open();
        }

        public int ReadChar()
        {
            return port.ReadChar();
        }

        public void WriteLine(string msg)
        {
            port.WriteLine(msg);
        }
    }
}
