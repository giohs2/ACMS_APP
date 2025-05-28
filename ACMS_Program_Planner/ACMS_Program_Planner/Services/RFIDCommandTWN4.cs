using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACMS_Program_Planner.Services
{
    public class RFIDCommandTWN4
    {
        public string SearchTag()
        {
            return "050020";
        }

        public string InitializeLEDs()
        {
            return "041007";
        }

        public string TurnOnGreenLED()
        {
            return "041102";
        }

        public string TurnOffGreenLED()
        {
            return "041202";
        }

        public string TurnOnRedLED()
        {
            return "041101";
        }

        public string TurnOffRedLED()
        {
            return "041201";
        }

        public string ISO15693_WriteSingleBlock(ushort block, string data)
        {
            string blockHex = ((byte)block).ToString("X2") + ((byte)(block >> 8)).ToString("X2");
            // Ensure data is 8 hex digits (4 bytes)
            data = data.PadRight(8, '0');
            return "0D07" + blockHex + "04" + data;
        }

        public string BeepShort()
        {
            return "040750600950005000";
        }

        public string BeepLong()
        {
            return "04075A0908E803F401";
        }

    }
}
