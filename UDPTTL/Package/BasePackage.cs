using System;

namespace UDPTTL.Package
{

    /// <summary>
    /// 包结构基类
    /// 相当于包头
    /// </summary>
    public class BasePackage
    {
        public const int PackSum = 23;

        /// <summary>
        /// 包类型
        /// </summary>
        public byte PackType;

        /// <summary>
        /// 数据包
        /// </summary>
        public int PackID;

        /// <summary>
        /// 包总长
        /// </summary>
        public long Length;

        /// <summary>
        /// 包序号
        /// </summary>
        public int PackSeq;

        /// <summary>
        /// 包目的地址
        /// 本地使用，不参与传输
        /// </summary>
        public string DestHost;

        /// <summary>
        /// 包目的端口
        /// 本地使用，不参与传输
        /// </summary>
        public int DestPort;

        /// <summary>
        /// 包版本
        /// </summary>
        public short Version=1;

        /// <summary>
        /// 本包真实长度
        /// </summary>
        public short PackLength;

        /// <summary>
        /// 分包大小
        /// </summary>
        public short PackSize;

        public long Ticks = DateTime.Now.Ticks;
        public BasePackage()
        {

        }
        public BasePackage(byte[]rec)
        { 
            Version =BitConverter.ToInt16(rec,0);
            PackType = rec[2];
            PackID = BitConverter.ToInt32(rec, 3);
            PackLength = BitConverter.ToInt16(rec, 7);
            PackSeq = BitConverter.ToInt32(rec, 9);
            Length = BitConverter.ToInt64(rec, 13);
            PackSize = BitConverter.ToInt16(rec, 21);
            //23
        }
        public virtual byte[] GetBuffer()
        {
            byte[] buf = new byte[PackSum];
            Array.Copy(BitConverter.GetBytes(Version), 0, buf, 0, 2); ;
            buf[2] = PackType;
            Array.Copy(BitConverter.GetBytes(PackID), 0, buf, 3, 4);
            Array.Copy(BitConverter.GetBytes(PackLength), 0, buf, 7, 2);
            Array.Copy(BitConverter.GetBytes(PackSeq), 0, buf, 9, 4);
            Array.Copy(BitConverter.GetBytes(Length), 0, buf, 13, 8);
            Array.Copy(BitConverter.GetBytes(PackSize), 0, buf, 21, 2);
            return buf;
        }
    }
}
