namespace UDPTTL.Package
{
    public class PackageControl
    {
        public const byte Data = 0;
        public const byte Ack = 1;
        public const byte Fin = 3;
        public const byte Lost = 4;
        public const byte ShutDown = 5;
        public const byte Syn = 2;

        public const short PackSize = 1472;//548
    }
}
