using PCSC;
using PCSC.Iso7816;
using PCSCLib.Mifare1k;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PCSCLib
{
    public class PCSCLib : IDisposable
    {
        public event EventHandler<PCSCExceptionRaisedEventArgs> PCSCExceptionRaised;
        public event EventHandler<PCSCCardReadersReloadedEventArgs> PCSCCardReadersReloaded;
        public event EventHandler<PCSCCardDataReadedEventArgs> PCSCCardDataReaded;        

        protected virtual void onPCSCExceptionRaised(PCSCExceptionRaisedEventArgs args)
        {
            PCSCExceptionRaised?.Invoke(this, args);
        }

        protected virtual void onPCSCCardReadersReloaded(PCSCCardReadersReloadedEventArgs args)
        {
            PCSCCardReadersReloaded?.Invoke(this, args);
        }

        protected virtual void onPCSCCardDataReaded(PCSCCardDataReadedEventArgs args)
        {
            PCSCCardDataReaded?.Invoke(this, args);
        }

        private string[] Readers { get; set; } = new string[] { };
        protected static IContextFactory MyContextFactory { get; } = ContextFactory.Instance;
        SCardMonitor Monitor { get; set; }

        public PCSCLib()
        {
            //CheckReaders();
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    CheckReaders();
                    await Task.Delay(500);
                }
            }, TaskCreationOptions.LongRunning);
        }

        protected virtual void CheckReaders()
        {
            string[] newReaders = GetReaderNames();
            if ((newReaders.Except(Readers).Count() != 0) || (Readers.Except(newReaders).Count() != 0))
            {
                Monitor?.Dispose();                
                Readers = newReaders;
                Monitor = new SCardMonitor(MyContextFactory, SCardScope.System);
                Monitor.Start(Readers);
                AttachToAllEvents();
            }
        }

        private void AttachToAllEvents()
        {
            if (Monitor != null)
            {
                // Point the callback function(s) to the anonymous & static defined methods below.
                Monitor.CardInserted += Monitor_CardInserted;
                //Monitor.CardRemoved += (sender, args) => DisplayEvent("CardRemoved", args);
                //Monitor.Initialized += (sender, args) => DisplayEvent("Initialized", args);
                Monitor.StatusChanged += (o, e) => CheckReaders();
                Monitor.MonitorException += (o, e) => CheckReaders();
            }
        }

        private void DisplayEvent(string v, CardStatusEventArgs args)
        {
            Console.Out.WriteLine(v + " in reader " + args.ReaderName);
        }

        private static readonly byte[] DATA_TO_WRITE = {
            0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00
        };
        private const byte MSB = 0x00;
        private const byte LSB = 0x08;

        protected virtual void Monitor_CardInserted(object sender, CardStatusEventArgs e)
        {
            try
            {
                using (var context = MyContextFactory.Establish(SCardScope.System))
                {
                    using (var isoReader = new IsoReader(context, e.ReaderName, SCardShareMode.Shared, SCardProtocol.Any, false))
                    {
                        var card = new MifareCard(isoReader);
                        var loadKeySuccessful = card.LoadKey(
                            KeyStructure.NonVolatileMemory,
                            0x00, // first key slot
                            new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF } // key
                        );
                        if (!loadKeySuccessful)
                        {
                            //throw new Exception("LOAD KEY failed.");
                        }
                        var authSuccessful = card.Authenticate(MSB, LSB, KeyType.KeyA, 0x00);
                        if (!authSuccessful)
                        {
                            throw new Exception("AUTHENTICATE failed.");
                        }
                        var result = card.ReadBinary(MSB, LSB, 16);
                        Console.WriteLine("Result (before BINARY UPDATE): {0}", (result != null) ? BitConverter.ToString(result) : null);
                        var updateSuccessful = card.UpdateBinary(MSB, LSB, DATA_TO_WRITE);
                        if (!updateSuccessful)
                        {
                            throw new Exception("UPDATE BINARY failed.");
                        }
                        result = card.ReadBinary(MSB, LSB, 16);
                        Console.WriteLine("Result (after BINARY UPDATE): {0}", (result != null) ? BitConverter.ToString(result) : null);
                    }
                }
            }
            catch { }
        }
    
        protected string[] GetReaderNames()
        {
            using (var context = MyContextFactory.Establish(SCardScope.System))
            {
                return context.GetReaders();
            }
        }    

        protected void SafeExecute(Action action)
        {
            try
            {
                action();
            }
            catch { }
        }

        public void Dispose()
        {
            Monitor?.Dispose();
            Monitor = null;
        }    
    }
}
