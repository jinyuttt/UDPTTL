using System.Collections.Generic;
using System.Timers;
using UDPTTL.Package;
namespace UDPTTL
{

    /// <summary>
    /// 检测数据发送
    /// </summary>
    public  class SendQueue
    {
        private readonly List<DATAPackage> SendBuffer;

        private readonly SocketStream stream;

        private const int interval = 100;//重复推送间隔（毫秒）

        private Timer  timerFulsh=null;

        private int ReSendNum = 0;

       /// <summary>
       /// 默认：5秒
       /// 单位：毫秒
       /// </summary>
        public  int SendTimeOut { get; set; }

        public  bool IsShut { get; set; }

        /// <summary>
        /// 最后一次接收数据
        /// </summary>
        private int LastFulsh = 0;

       
        public bool IsEmpty
        {
            get { return SendBuffer.Count == 0; }
        }
        public SendQueue(SocketStream stream,int len)
        {
            this.stream = stream;
            SendBuffer = new List<DATAPackage>(len);
            timerFulsh = new Timer();
            timerFulsh.Interval = interval;
            timerFulsh.Elapsed += TimerFulsh_Elapsed;
            timerFulsh.Start();
            SendTimeOut = 5000;
        }

        private void TimerFulsh_Elapsed(object sender, ElapsedEventArgs e)
        {
            for(int i=0;i< SendBuffer.Count;i++)
            {
                SendData(SendBuffer[i]);
            }
            ReSendNum++;
            if(ReSendNum>SendTimeOut/ interval+LastFulsh)
            {
                //按照最后一包数据发送超时计算
                Stop();//主动关闭
            }
        }

        /// <summary>
        /// 立即发送一个包
        /// </summary>
        /// <param name="pack"></param>
        private void SendData(BasePackage pack)
        {
            stream.Write(pack.DestHost, pack.DestPort, pack.GetBuffer());
        }

        /// <summary>
        /// 放置发送的数据
        /// </summary>
        /// <param name="package"></param>
        public void AddData(DATAPackage package)
        {
            SendBuffer.Add(package);
            SendData(package);
            LastFulsh = ReSendNum;
        }

        /// <summary>
        /// 接收确认包
        /// </summary>
        /// <param name="package"></param>
        public void AddAck(ACKPackage package)
        {
            var seqPack= SendBuffer.Find(x => x.PackSeq == package.PackSeq);
            if(seqPack!=null)
            {
                SendBuffer.Remove(seqPack);
                QueuePool.Roll(seqPack.Data);//回收数据
            }
           
        }

        /// <summary>
        /// 完成包
        /// </summary>
        /// <param name="package"></param>
        internal void AddFin(FINPackage package)
        {
            Stop();
        }

        /// <summary>
        /// 丢失
        /// </summary>
        /// <param name="package"></param>
        public void AddLost(LostPackage package)
        {
            var pack=SendBuffer.Find(x => x.PackSeq == package.PackSeq);
            if(pack==null)
            {
                //发送一个丢失包
                LostPackage lost = new LostPackage() { PackID = package.PackID, PackSeq = package.PackSeq,direction=1 };
                SendData(lost);
            }
            else
            {
                SendData(pack);
            }
        }

        /// <summary>
        /// 接收关闭信息
        /// </summary>
        /// <param name="package"></param>
        public void AddShutDown(ShutDownPackage package)
        {
            Stop();
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public void Clear()
        {
            //回收所以数据
            for(int i=0;i<SendBuffer.Count;i++)
            {
                QueuePool.Roll(SendBuffer[i].Data);
            }
            SendBuffer.Clear();
        }

        /// <summary>
        /// 停止使用
        /// </summary>
        public void Stop()
        {
            timerFulsh.Stop();
            timerFulsh = null;
            IsShut = true;
            Clear();
           
        }
    }
}
