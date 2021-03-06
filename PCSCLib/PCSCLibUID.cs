using PCSC;
using PCSC.Iso7816;
using PCSCLib.ARC122U;
using System;

namespace PCSCLib
{
    public class PCSCLibUID : PCSCLib
    {
        public event EventHandler<PCSCCardUIDLoadedEventArgs> PCSCCardUIDLoaded;

        public bool ConsoleOutEnabled { get; set; } = false;

        protected virtual void onPCSCCardUIDLoadedRaised(PCSCCardUIDLoadedEventArgs args)
        {
            PCSCCardUIDLoaded?.Invoke(this, args);
        }

        protected override void Monitor_CardInserted(object sender, CardStatusEventArgs e)
        {
            //base.Monitor_CardInserted(sender, e);
            var readerName = e.ReaderName;
            using (var context = MyContextFactory.Establish(SCardScope.System))
            {
                using (var rfidReader = new SCardReader(context))
                {
                    var sc = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                    if (sc != SCardError.Success)
                    {
                        if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("Could not connect to reader {0}:\n{1}",
                                readerName,
                                SCardHelper.StringifyError(sc));
                        }
                        this.PlayStatus(readerName, false);
                        return;
                    }

                    var apdu = new CommandApdu(IsoCase.Case2Short, rfidReader.ActiveProtocol)
                    {
                        CLA = 0xFF,
                        Instruction = InstructionCode.GetData,
                        P1 = 0x00,
                        P2 = 0x00,
                        Le = 0 // We don't know the ID tag size
                    };

                    sc = rfidReader.BeginTransaction();
                    if (sc != SCardError.Success)
                    {
                        if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("Could not begin transaction.");
                        }
                        this.PlayStatus(readerName, false);
                        return;
                    }

                    Console.WriteLine("Retrieving the UID .... ");

                    //var receivePci = new SCardPCI(); // IO returned protocol control information.
                    using (var receivePci = new SCardPCI())
                    {
                        var sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);

                        var receiveBuffer = new byte[256];
                        var command = apdu.ToArray();

                        sc = rfidReader.Transmit(
                            sendPci, // Protocol Control Information (T0, T1 or Raw)
                            command, // command APDU
                            receivePci, // returning Protocol Control Information
                            ref receiveBuffer); // data buffer

                        if (sc != SCardError.Success)
                        {
                            if (ConsoleOutEnabled)
                            {
                                Console.WriteLine("Error: " + SCardHelper.StringifyError(sc));
                            }
                        }

                        var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);
                        if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("SW1: {0:X2}, SW2: {1:X2}\nUid: {2}",
                                responseApdu.SW1,
                                responseApdu.SW2,
                                responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received");
                        }
                        rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                        rfidReader.Disconnect(SCardReaderDisposition.Reset);


                        string SW1 = string.Format("{0:X2}", responseApdu.SW1);
                        string SW2 = string.Format("{0:X2}", responseApdu.SW2);

                        byte[] _data = responseApdu.GetData();
                        string UID = responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "";

                        onPCSCCardUIDLoadedRaised(new PCSCCardUIDLoadedEventArgs(readerName, SW1, SW2, UID));


                        //PlayWithLedsAndBuzzer(readerName);
                    }
                }
            }
        }


        public void PlayWithLedsAndBuzzer(string readerName)
        {
            if (!readerName.ToUpper().Contains("ACR122"))
            {
                return;
            }
            using (var context = MyContextFactory.Establish(SCardScope.System))
            {
                using (var rfidReader = new SCardReader(context))
                {
                    var sc = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                    if (sc != SCardError.Success)
                    {
                        if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("Could not connect to reader {0}:\n{1}",
                                readerName,
                                SCardHelper.StringifyError(sc));
                        }
                        return;
                    }

                    sc = rfidReader.BeginTransaction();
                    if (sc != SCardError.Success)
                    {
                        if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("Could not begin transaction.");
                        }
                        return;
                    }

                    ARC122ULedsBuzzerCommand command = new ARC122ULedsBuzzerCommand(
                        ARC122ULedsBuzzerCommand.LedState.Off,
                        ARC122ULedsBuzzerCommand.LedState.On,
                        ARC122ULedsBuzzerCommand.LedStateMask.Update,
                        ARC122ULedsBuzzerCommand.LedStateMask.Update,
                        ARC122ULedsBuzzerCommand.LedState.Off,
                        ARC122ULedsBuzzerCommand.LedState.On,
                        ARC122ULedsBuzzerCommand.LedStateMask.NoChange,
                        ARC122ULedsBuzzerCommand.LedStateMask.NoChange,
                        ARC122ULedsBuzzerCommand.BuzzerStatus.OnT1,
                        1,
                        1,
                        4
                    );


                    //var receivePci = new SCardPCI(); // IO returned protocol control information.
                    using (var receivePci = new SCardPCI())
                    {
                        var sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);

                        var receiveBuffer = new byte[256];

                        sc = rfidReader.Transmit(
                            sendPci, // Protocol Control Information (T0, T1 or Raw)
                            command.GetCommand(), // command APDU
                            receivePci, // returning Protocol Control Information
                            ref receiveBuffer); // data buffer

                        if (sc != SCardError.Success)
                        {
                            if (ConsoleOutEnabled)
                            {
                                Console.WriteLine("Error: " + SCardHelper.StringifyError(sc));
                            }
                        }

                        var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);
                        //if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("SW1: {0:X2}, SW2: {1:X2}\nUid: {2}",
                                responseApdu.SW1,
                                responseApdu.SW2,
                                responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received");
                        }
                        rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                        rfidReader.Disconnect(SCardReaderDisposition.Reset);


                        string SW1 = string.Format("{0:X2}", responseApdu.SW1);
                        string SW2 = string.Format("{0:X2}", responseApdu.SW2);

                        byte[] _data = responseApdu.GetData();
                    }
                }
            }
        }

        public bool PlayStatus(string readerName, bool status)
        {
            if (!readerName.ToUpper().Contains("ACR122"))
            {
                return false;
            }
            using (var context = MyContextFactory.Establish(SCardScope.System))
            {
                using (var rfidReader = new SCardReader(context))
                {
                    var sc = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                    if (sc != SCardError.Success)
                    {
                        if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("Could not connect to reader {0}:\n{1}",
                                readerName,
                                SCardHelper.StringifyError(sc));
                        }
                        return false;
                    }

                    sc = rfidReader.BeginTransaction();
                    if (sc != SCardError.Success)
                    {
                        if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("Could not begin transaction.");
                        }
                        return false;
                    }

                    ARC122ULedsBuzzerCommand command;

                    if (status)
                    {
                        command = new ARC122ULedsBuzzerCommand(
                            ARC122ULedsBuzzerCommand.LedState.Off,
                            ARC122ULedsBuzzerCommand.LedState.On,
                            ARC122ULedsBuzzerCommand.LedStateMask.Update,
                            ARC122ULedsBuzzerCommand.LedStateMask.Update,
                            ARC122ULedsBuzzerCommand.LedState.Off,
                            ARC122ULedsBuzzerCommand.LedState.On,
                            ARC122ULedsBuzzerCommand.LedStateMask.NoChange,
                            ARC122ULedsBuzzerCommand.LedStateMask.NoChange,
                            ARC122ULedsBuzzerCommand.BuzzerStatus.OnT1,
                            1,
                            1,
                            9
                        );
                    }
                    else
                    {
                        command = new ARC122ULedsBuzzerCommand(
                            ARC122ULedsBuzzerCommand.LedState.On,
                            ARC122ULedsBuzzerCommand.LedState.On,
                            ARC122ULedsBuzzerCommand.LedStateMask.Update,
                            ARC122ULedsBuzzerCommand.LedStateMask.Update,
                            ARC122ULedsBuzzerCommand.LedState.On,
                            ARC122ULedsBuzzerCommand.LedState.On,
                            ARC122ULedsBuzzerCommand.LedStateMask.NoChange,
                            ARC122ULedsBuzzerCommand.LedStateMask.NoChange,
                            ARC122ULedsBuzzerCommand.BuzzerStatus.OnT1,
                            1,
                            4,
                            6
                        );
                    }

                    //var receivePci = new SCardPCI(); // IO returned protocol control information.
                    using (var receivePci = new SCardPCI())
                    {
                        var sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);

                        var receiveBuffer = new byte[256];

                        sc = rfidReader.Transmit(
                            sendPci, // Protocol Control Information (T0, T1 or Raw)
                            command.GetCommand(), // command APDU
                            receivePci, // returning Protocol Control Information
                            ref receiveBuffer); // data buffer

                        if (sc != SCardError.Success)
                        {
                            if (ConsoleOutEnabled)
                            {
                                Console.WriteLine("Error: " + SCardHelper.StringifyError(sc));
                            }
                            return false;
                        }

                        var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);
                        //if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("SW1: {0:X2}, SW2: {1:X2}\nUid: {2}",
                                responseApdu.SW1,
                                responseApdu.SW2,
                                responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received");
                        }
                        rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                        rfidReader.Disconnect(SCardReaderDisposition.Reset);


                        string SW1 = string.Format("{0:X2}", responseApdu.SW1);
                        string SW2 = string.Format("{0:X2}", responseApdu.SW2);

                        byte[] _data = responseApdu.GetData();
                    }
                }
                return true;
            }
        }


        public bool DisableBuzzerOnCardDetected(string readerName)
        {
            if (!readerName.ToUpper().Contains("ACR122"))
            {
                return false;
            }
            using (var context = MyContextFactory.Establish(SCardScope.System))
            {
                using (var rfidReader = new SCardReader(context))
                {
                    var sc = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                    if (sc != SCardError.Success)
                    {
                        if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("Could not connect to reader {0}:\n{1}",
                                readerName,
                                SCardHelper.StringifyError(sc));
                        }
                        return false;
                    }

                    sc = rfidReader.BeginTransaction();
                    if (sc != SCardError.Success)
                    {
                        if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("Could not begin transaction.");
                        }
                        return false;
                    }

                    ARC122UBuzzerOffCardDetectedCommand command = new ARC122UBuzzerOffCardDetectedCommand();

                    //var receivePci = new SCardPCI(); // IO returned protocol control information.
                    using (var receivePci = new SCardPCI())
                    {
                        var sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);

                        var receiveBuffer = new byte[256];

                        sc = rfidReader.Transmit(
                            sendPci, // Protocol Control Information (T0, T1 or Raw)
                            command.GetCommand(), // command APDU
                            receivePci, // returning Protocol Control Information
                            ref receiveBuffer); // data buffer

                        if (sc != SCardError.Success)
                        {
                            if (ConsoleOutEnabled)
                            {
                                Console.WriteLine("Error: " + SCardHelper.StringifyError(sc));
                            }
                            return false;
                        }

                        var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);
                        //if (ConsoleOutEnabled)
                        {
                            Console.WriteLine("SW1: {0:X2}, SW2: {1:X2}\nUid: {2}",
                                responseApdu.SW1,
                                responseApdu.SW2,
                                responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received");
                        }
                        rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                        rfidReader.Disconnect(SCardReaderDisposition.Reset);


                        string SW1 = string.Format("{0:X2}", responseApdu.SW1);
                        string SW2 = string.Format("{0:X2}", responseApdu.SW2);

                        byte[] _data = responseApdu.GetData();
                    }
                }
                return true;
            }
        }

    }
}
