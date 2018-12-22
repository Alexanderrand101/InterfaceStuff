using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceStuff
{
    class Device7018 : Device
    {
        private double measuredValue;

        public double MeasuredValue { get => measuredValue; set => measuredValue = value; }

        public string ResponseTo(string msg)
        {
            if (msg[0] == '$' && msg[3] == 'M')
            {
                return "!" + msg.Substring(1, 2) + "7018" + (char)0x0D;
            }
            else if (msg[0] == '$' && msg.Length > 2 && msg[3] == '5')
            {
                return "!" + msg.Substring(1, 2) + (char)0x0D;
            }
            else if (msg[0] == '#' && msg.Length > 2)
            {
                return ">+" + String.Format("{0,6:00.000}", measuredValue) + (char)0x0D;
            }
            else
            {
                return "CNI" + msg.Substring(1, 2) + (char)0x0D;
            }
        }
    }
}
