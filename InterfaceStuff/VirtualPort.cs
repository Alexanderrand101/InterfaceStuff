using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceStuff
{
    class VirtualPort : PortInterface
    {
        private bool isOpen;

        private string portName;
        private int baudRate;
        private int dataBits;
        private StopBits stopBits;
        private Parity parity;
        private Handshake handshake;
        private int recivedBytesThreshold;
        private int writeBufferSize;
        private int readBufferSize;
        private int readTimeout;
        private int writeTimeout;
        private bool dtrEnable;
        private bool rtsEnable;

        private Dictionary<string, Device> devAdr = new Dictionary<string, Device>();

        public VirtualPort()
        {
            Device7018 dev = new Device7018();
            devAdr.Add("01", new Device7021(dev));
            devAdr.Add("02", dev);
            devAdr.Add("03", new Device7044());
        }

        private MemoryStream buffer;

        public string PortName { get => portName; set => portName = value; }
        public int BaudRate { get => baudRate; set => baudRate = value; }
        public int DataBits { get => dataBits; set => dataBits = value; }
        public StopBits StopBits { get => stopBits; set => stopBits = value; }
        public Parity Parity { get => parity; set => parity = value; }
        public Handshake Handshake { get => handshake; set => handshake = value; }
        public int ReceivedBytesThreshold { get => recivedBytesThreshold; set => recivedBytesThreshold = value; }
        public int WriteBufferSize { get => writeBufferSize; set => writeBufferSize = value; }
        public int ReadBufferSize { get => readBufferSize; set => readBufferSize = value; }
        public int ReadTimeout { get => readTimeout; set => readTimeout = value; }
        public bool DtrEnable { get => dtrEnable; set => dtrEnable = value; }
        public bool RtsEnable { get => rtsEnable; set => rtsEnable = value; }
        public int WriteTimeout { get => writeTimeout; set => writeTimeout = value; }
        public bool IsOpen { get => isOpen; set => isOpen = value; }

        public event SerialDataReceivedEventHandler DataReceived;

        public void Close()
        {
            if (!isOpen) throw new Exception("Not Opened");
            isOpen = false;
        }

        public void DiscardInBuffer()
        {
            //do nothing
        }

        public void DiscardOutBuffer()
        {
            //do nothing
        }

        public void Open()
        {
            if (isOpen) throw new Exception("Already Open");
            isOpen = true;
        }

        public int ReadChar()
        {
            if (!isOpen) throw new Exception("Not Opened");
            return buffer.ReadByte();
        }

        public void WriteLine(string msg)
        {
            if(devAdr.ContainsKey(msg.Substring(1, 2)))
            {
                Device dev = devAdr[msg.Substring(1, 2)];
                buffer = new MemoryStream(Encoding.ASCII.GetBytes(dev.ResponseTo(msg)));
                DataReceived(this, null);
            }
        }
    }
}
