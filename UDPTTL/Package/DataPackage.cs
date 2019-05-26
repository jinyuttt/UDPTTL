using System;

namespace UDPTTL.Package
{

    /// <summary>
    /// 数据包
    /// </summary>
    public  class DATAPackage:BasePackage
    {
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }

        public DATAPackage(byte[] data):base(data)
        {
            Data = new byte[this.PackLength];
            Array.Copy(data, BasePackage.PackSum, Data,0, Data.Length);
          
        }
        public DATAPackage()
        {
            PackType = PackageControl.Data;
        }

        public override byte[] GetBuffer()
        {
            //
            byte[] bytes = new byte[BasePackage.PackSum + Data.Length];
            byte[] head = base.GetBuffer();
            Array.Copy(base.GetBuffer(), bytes, BasePackage.PackSum);
            Array.Copy(Data,0, bytes, BasePackage.PackSum,Data.Length);
            return bytes;
        }

        
       
    }
}
