namespace UDPTTL.Package
{
    public class LostPackage : BasePackage
    {
        /// <summary>
        /// 方向
        /// 0是接收端发送的;
        /// 1是发送端发送
        /// 默认：0
        /// </summary>
        public byte direction = 0;
        public LostPackage(byte[] bytes):base(bytes)
        {
            direction = bytes[BasePackage.PackSum];
        }
        public LostPackage()
        {
            PackType = PackageControl.Lost;
        }
    }
}
