using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSCLib.ARC122U
{
    public class ARC122ULedsBuzzerCommand : ARC122UCommand
    {
        protected override byte[] Command { get; set; }

        public enum LedBlink
        {
            Blink,
            NoBlink
        }

        public enum LedState
        {
            On,
            Off
        }

        public enum LedStateMask
        {
            Update,
            NoChange
        }

        public enum BuzzerStatus : byte
        {
            Off = 0x00,
            OnT1 = 0x01,
            OnT2 = 0x02,
            OnT1T2 = 0x03
        }

        public ARC122ULedsBuzzerCommand(
            LedState redFinalLedState,
            LedState greenFinalLedState,
            LedStateMask redStateMask,
            LedStateMask greenStateMask,           
            LedState redBlinkLedState,
            LedState greenBlinkLedState,
            LedStateMask redBlinkStateMask,
            LedStateMask greenBlinkStateMask,
            BuzzerStatus buzzerStatus, byte numberOfRepetitions, byte hundredMillisecondsT1, byte hundredMillisecondsT2
            )
        {
            Class = 0xFF;
            Instruction = 0x00;
            P1 = 0x40;
            LcLe = 0x04;
            P2 = 0x00;
            /*
            P2 |= (redInitialLedStatus == LedStatus.On ? (byte)0x10 : (byte)0x00);
            P2 |= (greenInitialLedStatus == LedStatus.On ? (byte)0x20 : (byte)0x00);
            P2 |= (redLedBlink == LedBlink.Blink ? (byte)0x40 : (byte)0x00);
            P2 |= (greenLedBlink == LedBlink.Blink ? (byte)0x80 : (byte)0x00);
            //initial state for leds set to 1
            P2 |= 0x10;
            P2 |= 0x20;
            */
            P2 |= (redFinalLedState == LedState.On ? (byte)0x01 : (byte)0x00);
            P2 |= (greenFinalLedState == LedState.On ? (byte)0x02 : (byte)0x00);
            P2 |= (redStateMask == LedStateMask.Update ? (byte)0x04 : (byte)0x00);
            P2 |= (greenStateMask == LedStateMask.Update ? (byte)0x08 : (byte)0x00);
            P2 |= (redBlinkLedState == LedState.On ? (byte)0x10 : (byte)0x00);
            P2 |= (greenBlinkLedState  == LedState.On ? (byte)0x20 : (byte)0x00);
            P2 |= (redBlinkStateMask == LedStateMask.Update ? (byte)0x40 : (byte)0x00);
            P2 |= (greenBlinkStateMask == LedStateMask.Update ? (byte)0x80 : (byte)0x00);

            Command = new byte[]
            {
                Class,
                Instruction,
                P1,
                P2,
                LcLe,
                hundredMillisecondsT1,
                hundredMillisecondsT2,
                numberOfRepetitions,
                (byte)buzzerStatus
            };
        }
    }
}
