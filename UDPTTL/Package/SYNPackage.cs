namespace UDPTTL.Package
{

    /// <summary>
    /// 连接请求或是连接接受请求
    /// 暂时无用
    /// </summary>
    public class SYNPackage:BasePackage
    {
        public SYNPackage()
        {
            PackType = PackageControl.Syn; 
        }
        public SYNPackage(byte[] data):base(data)
        {
           
        }
    }
}
