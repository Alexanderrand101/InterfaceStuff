using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceStuff
{
    class Device7044 : Device
    {
        bool lampOn = false;
        bool pulseOn = false;

        public Device7044() { }

        public string ResponseTo(string msg)
        {
            if (msg[0] == '$' && msg[3] == 'M')
            {
                return "!" + msg.Substring(1, 2) + "7044" + (char)0x0D;
            }
            else if (msg[0] == '$') return ">" + (char)0x0D;
            else return "CNI" + msg.Substring(1, 2) + (char)0x0D;
        }
    }
}
