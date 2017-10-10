using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSCLib.ARC122U
{
    public abstract class ARC122UCommand
    {
        public byte Class { get; protected set; }
        public byte Instruction { get; protected set; }
        public byte P1 { get; protected set; }
        public byte P2 { get; protected set; }
        public byte LcLe { get; protected set; }

        public abstract byte[] GetCommand();
    }
}
