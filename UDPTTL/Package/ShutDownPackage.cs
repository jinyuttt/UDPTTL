namespace UDPTTL.Package
{
    public class ShutDownPackage:BasePackage
    {
        /// <summary>
        /// 方向
        /// 0是接收端发送的;
        /// 1是发送端发送
        /// 默认：0
        /// </summary>
        public byte direction = 0;
        public ShutDownPackage(byte[]data):base(data)
        {
            direction = data[BasePackage.PackSum];
        }
       public ShutDownPackage()
        {
            PackType = PackageControl.ShutDown;
        }
    }
}
