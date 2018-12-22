using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceStuff
{
    interface PortInterface
    {
        event SerialDataReceivedEventHandler DataReceived;
        void DiscardOutBuffer();
        void DiscardInBuffer();
        int ReadChar();
        void WriteLine(string msg);
        string PortName { get; set; }
        int BaudRate { get; set; }
        int DataBits { get; set; }
        StopBits StopBits { get; set; }
        Parity Parity { get; set; }
        Handshake Handshake { get; set; }
        int ReceivedBytesThreshold { get; set; }
        int WriteBufferSize { get; set; }
        int ReadBufferSize { get; set; }
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }
        bool IsOpen { get;}
        bool DtrEnable { get; set; }
        void Open();
        bool RtsEnable { get; set; }
        void Close();
    }
}
