namespace PCSCLib
{
    public class PCSCCardUIDLoadedEventArgs
    {
        public string ReaderName { get; private set; }
        public string SW1 { get; private set; }
        public string SW2 { get; private set; }
        public string UID { get; private set; }

        public PCSCCardUIDLoadedEventArgs(string ReaderName, string SW1, string SW2, string UID)
        {
            this.ReaderName = ReaderName;
            this.SW1 = SW1;
            this.SW2 = SW2;
            this.UID = UID;
        }

    }
}
