using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSCLib.ARC122U
{
    public class ARC122UBuzzerOffCardDetectedCommand : ARC122UCommand
    {
        public ARC122UBuzzerOffCardDetectedCommand()
        {
            this.Command = new byte[]
            {
                0xFF,
                0x00,
                0x52,
                0x00,
                0x00
            };
        }
    }
}
