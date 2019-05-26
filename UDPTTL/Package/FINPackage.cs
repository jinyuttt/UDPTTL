namespace UDPTTL.Package
{

    /// <summary>
    /// 接收完成包
    /// </summary>
    public class FINPackage:BasePackage
    {
        public FINPackage()
        {
            PackType = PackageControl.Fin;
        }

        public FINPackage(byte[] rec) : base(rec)
        {
        }
    }
}
