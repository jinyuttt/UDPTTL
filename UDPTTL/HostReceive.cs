#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UDPTTL
* 项目描述 ：
* 类 名 称 ：HostReceive
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
using System.Collections.Concurrent;
using UDPTTL.Package;

namespace UDPTTL
{

    /* ============================================================================== 
* 功能描述：HostReceive 每个IP+端口的数据
* 1.从session接收数据放入ReceiveQueue
* 2.ReceiveQueue接收完成发送完成包FINPackage
* 3.触发接收数据的数据回调方法
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class HostReceive
    {

        /// <summary>
        /// 接收数据
        /// </summary>
        private readonly ConcurrentDictionary<int, ReceiveQueue> queue;

        /// <summary>
        /// 发送Session
        /// </summary>
        private readonly TTLSenderSession senderSession = null;

        /// <summary>
        /// 记录已经关闭的接收
        /// </summary>
        private readonly ConcurrentDictionary<int, DateTime> receiveTime;

        public DataNotify dataNotify;

        /// <summary>
        /// 对应的主机端点
        /// </summary>
        public HostAndPort Host { get; set; }

        /// <summary>
        /// 最后使用时间
        /// </summary>
        public int LastUseTime { get; set; }

        /// <summary>
        /// 计时器
        /// </summary>
        public int CountTime { get; set; }

        /// <summary>
        /// 是否关闭
        /// </summary>
        public bool IsShut { get; set; }

        public HostReceive(TTLSenderSession session)
        {
            senderSession = session;
            queue = new ConcurrentDictionary<int, ReceiveQueue>();
            receiveTime = new ConcurrentDictionary<int, DateTime>();
        }

        /// <summary>
        /// 加入数据包
        /// </summary>
        /// <param name="package"></param>
        public void Add(BasePackage package)
        {

            ReceiveQueue receiveQueue = null;
            if (receiveTime.ContainsKey(package.PackID))
            {
                //发送一次完成，触发发送端移除
                SendSucess(package.PackID, package.DestHost, package.DestPort);
                return;
            }

            if (!queue.ContainsKey(package.PackID))
            {
                //接收队列计算一个真实的量用于请求发送
                int len = (int)(package.Length / package.PackSize);
                len = package.Length % package.PackSize > 0 ? len + 1 : len;
                receiveQueue = new ReceiveQueue(len)
                {
                    Stream = senderSession.SocketStream,
                    DestHost = this.Host.Host,
                    DestPort = this.Host.Port
                };
                queue[package.PackID] = receiveQueue;
            }

            if (queue.TryGetValue(package.PackID, out receiveQueue))
            {
                if (package is DATAPackage)
                {
                    if (receiveQueue.Add((DATAPackage)package))
                    {
                        //接收完成
                        ReceiveSucess(package.PackID);
                        if (dataNotify != null)
                        {
                            dataNotify.Invoke(this, receiveQueue.GetData(), Host);
                        }
                        receiveQueue.Stop();
                    }
                }
                else if (package is ShutDownPackage)
                {
                    ShutDownPackage pack = package as ShutDownPackage;
                    if (0 == pack.direction)
                    {
                        senderSession.Add(pack);
                    }
                    else
                    {
                        receiveQueue.Add(pack);
                    }
                  
                }
                else if (package is LostPackage)
                {
                    //说明是回执信息，转给发送队列
                    LostPackage pack =package as LostPackage;
                    if (0 == pack.direction)
                    {
                        senderSession.Add(package);
                    }
                    else
                    {
                        receiveQueue.Add(pack);
                    }
                   
                }
                else if (package is ACKPackage)
                {
                    senderSession.Add(package);
                }
                else if (package is FINPackage)
                {
                    senderSession.Add(package);
                }
            }
        }

        internal void Stop()
        {
            IsShut = true;
            foreach (var kv in queue)
            {
                kv.Value.Stop();
            }
            queue.Clear();
            receiveTime.Clear();
        }

        /// <summary>
        /// 接收完成则移除
        /// </summary>
        /// <param name="id"></param>
        private void ReceiveSucess(int id)
        {
            receiveTime[id] = DateTime.Now;
            ReceiveQueue receive;
            queue.TryRemove(id, out receive);
            SendSucess(receive.PackID, receive.DestHost, receive.DestPort);
        }

        /// <summary>
        /// 发送完成数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        private void SendSucess(int id, string host, int port)
        {
            FINPackage package = new FINPackage() { PackID = id };
            senderSession.SocketStream.Write(host, port, package.GetBuffer());
        }
       

    }
}
