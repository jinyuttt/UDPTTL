namespace UDPTTL.Package
{

    /// <summary>
    /// 确认包
    /// </summary>
    public class ACKPackage:BasePackage
    {
        public ACKPackage()
        {
            PackType = PackageControl.Ack;

        }

        public ACKPackage(byte[] data):base(data)
        {
           
        }
    }
}
