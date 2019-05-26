#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UDPTTL
* 项目描述 ：
* 类 名 称 ：TTLReceiveSession
* 类 描 述 ：
* 命名空间 ：UDPTTL
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion


using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UDPTTL.Package;

namespace UDPTTL
{

    /* ============================================================================== 
* 功能描述：ReceiveQueue  完整的数据接收包
* 1.接收数据包，关闭报
* 2.主动发送丢包信息，确认包
* 3.自我监测，如果5秒没有接收到数据，自动关闭
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class ReceiveQueue
    {
        byte[] buf = null;

        private DATAPackage[] ds = null;

        private volatile int index = 0;
        private int WaitRecNum = 0;
        private int lastRecNum = 0;
        private int Len = 0;

        /// <summary>
        /// 丢失的数据
        /// </summary>
        private List<int> lstSeq = null;

        public string DestHost { get; set; }

        public int DestPort { get; set; }

        public int PackID { get; set; }

        public bool IsShut { get; set; }

        public int RecTimeOut { get; set; }

        public SocketStream Stream { get; set; }



        public ReceiveQueue(int len)
        {
            RecTimeOut = 5;
            Len = len;
            ds = new DATAPackage[len];
            lstSeq = new List<int>(len);
            for (int i = 0; i < len; i++)
            {
                lstSeq.Add(i);
            }
           
        }

        /// <summary>
        /// 发送一定时间丢包的
        /// 该实现和发送端实现不一样
        /// </summary>
        private void SendLost()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                //先发送启动确认包；
                SendAck();
                if (lstSeq.Count>0)
                {
                    for (int i = 0; i < lstSeq.Count; i++)
                    {
                        LostPackage lost = new LostPackage() { PackID = this.PackID, PackSeq = lstSeq[i],direction=0 };
                        Stream.Write(DestHost, DestPort, lost.GetBuffer());
                    }
                }
                WaitRecNum++;
                if(!IsShut)
                {
                    SendLost();
                }
                if(WaitRecNum-lastRecNum>RecTimeOut*10)
                {
                    //实际是WaitRecNum-lastRecNum>RecTimeOut*1000/100;
                    Stop();
                }
            });
        }

        private void SendAck()
        {
            Task.Factory.StartNew(() => {
                for (int i = 0; i < Len; i++)
                {
                    if (!lstSeq.Contains(i))
                    {
                        ACKPackage package = new ACKPackage();
                        package.PackID = this.PackID;
                        package.PackSeq = i;
                        Stream.Write(DestHost, DestPort, package.GetBuffer());
                    }
                }
            });
           
        }

        /// <summary>
        /// 验证包
        /// </summary>
        private void Check()
        {
            lock (this)
            {
                index = 0;
                for (int i = 0; i < ds.Length; i++)
                {
                    if(ds[i]!=null)
                    {
                        index++;
                    }
                }
            }
        }

        /// <summary>
        /// 添加包
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public bool Add(DATAPackage package)
        {
            ds[package.PackSeq] = package;
            lstSeq.Remove(package.PackSeq);
            lastRecNum = WaitRecNum;
            if (index==0)
            {
                SendLost();
            }
            index++;
            if (index >= ds.Length)
            {
                Check();
            }
            return index==ds.Length;
        }

        /// <summary>
        /// 发送方丢失包
        /// </summary>
        /// <param name="lost"></param>
        public void Add(LostPackage lost)
        {
            //如果丢失就从丢失包中移除，数据等待，没有就超时
            lstSeq.Remove(lost.PackSeq);
        }

        /// <summary>
        /// 关闭包
        /// </summary>
        /// <param name="package"></param>
        public void Add(ShutDownPackage package)
        {
            Stop();
        }

        public byte[] GetData()
        {
       
            byte[] buf = new byte[ds[0].Length];
            int offset = 0;
            for(int i=0;i<ds.Length;i++)
            {
               
                Array.Copy(ds[i].Data, 0, buf, offset, ds[i].PackLength);
                offset += offset + ds[i].PackLength;
            }
            return buf;
        }

        public void Stop()
        {
            IsShut = true;
            ds = null;
        }
    }
}
