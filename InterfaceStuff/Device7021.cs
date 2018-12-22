using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceStuff
{
    class Device7021 : Device
    {
        private Device7018 device7018;

        public Device7021(Device7018 device7018){
            this.device7018 = device7018;
        }

        public string ResponseTo(string msg)
        {
            if(msg[0] == '$' && msg[3] == 'M')
            {
                return "!" + msg.Substring(1, 2) + "7021" + (char)0x0D; 
            }
            else if (msg[0] == '#' && msg.Length > 3)
            {
                double value = Double.Parse(msg.Remove(0, 3));
                if (value > 10)
                {
                    device7018.MeasuredValue = 10;
                    return "?" + msg.Substring(1, 2) + (char)0x0D;
                }
                device7018.MeasuredValue = value;
                return ">" + (char)0x0D;
            }
            else
            {
                return "CNI" + msg.Substring(1, 2) + (char)0x0D;
            }        
        }
    }
}
