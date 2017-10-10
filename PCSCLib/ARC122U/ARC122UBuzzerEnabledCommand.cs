using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSCLib.ARC122U
{
    class ARC122UBuzzerEnabledCommand : ARC122UCommand
    {
        protected override byte[] Command { get; set; }

        public ARC122UBuzzerEnabledCommand(bool buzzerEnabled)
        {
            Class = 0xFF;
            Instruction = 0x00;
            P1 = 0x52;
            P2 = (buzzerEnabled ? (byte)0xFF : (byte)0x00);
            LcLe = 0x00;

            Command = new byte[]
            {
                Class,
                Instruction,
                P1,
                P2,
                LcLe
            };
        }
    }
}
