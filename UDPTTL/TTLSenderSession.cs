#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UDPTTL
* 项目描述 ：
* 类 名 称 ：SendQueue
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
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UDPTTL.Package;

namespace UDPTTL
{

    /* ============================================================================== 
* 功能描述：TTLSenderSession  控制每次发送 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class TTLSenderSession
    {

        /// <summary>
        /// 发送的数据包
        /// </summary>
        private ConcurrentDictionary<int, SendQueue> queue;

        /// <summary>
        /// 回调数据
        /// </summary>
        private DataNotify dataNotify = null;

        /// <summary>
        /// socket
        /// </summary>
        public SocketStream SocketStream { get; set; }

        public TTLSenderSession(Socket socket)
        {
            queue = new ConcurrentDictionary<int, SendQueue>();
            SocketStream = new SocketStream(socket);
        }

        /// <summary>
        /// 转入包
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public bool Add(BasePackage  package)
        {
            SendQueue sessionQueue=null;
            if (queue.TryGetValue(package.PackID, out sessionQueue))
            {
                if(sessionQueue.IsShut)
                {
                    queue.TryRemove(package.PackID, out sessionQueue);
                    return false;
                }
                if (package is DATAPackage)
                {
                    sessionQueue.AddData((DATAPackage)package);
                }
                else if (package is FINPackage)
                {
                    sessionQueue.AddFin((FINPackage)package);
                    //删除
                    queue.TryRemove(package.PackID, out sessionQueue);
                }
                else if (package is ACKPackage)
                {
                    sessionQueue.AddAck((ACKPackage)package);
                    if(sessionQueue.IsEmpty)
                    {
                        //接收完成
                        queue.TryRemove(package.PackID, out sessionQueue);
                    }
                }
                else if(package is LostPackage)
                {
                    sessionQueue.AddLost((LostPackage)package);
                }
            }
            else
            {
                if(package is LostPackage)
                {
                    //如果是接收端发送的丢失包没有找到，则发送一次关闭
                    SendShut(package);
                    return true;
                }
                int len =(int)(package.Length / package.PackSize) + 1;
                sessionQueue = new SendQueue(SocketStream,len);
                sessionQueue.AddData((DATAPackage)package);
                queue[package.PackID] = sessionQueue;
            }
            //
            return true;
        }

        /// <summary>
        /// 调用数据接收
        /// </summary>
        internal void Receive()
        {
            SocketStream.Receive();
            SocketStream.DataCall += Stream_DataCall;
        }

        /// <summary>
        /// 提供接收的数据接口
        /// </summary>
        /// <param name="notify"></param>
        internal void Regster(DataNotify notify)
        {
            dataNotify += notify;
        }


        private void Stream_DataCall(object sender, byte[] data, HostAndPort host)
        {
            if(dataNotify!=null)
            {
                dataNotify.Invoke(sender, data, host);
            }
        }

        /// <summary>
        /// 立即关闭
        /// </summary>
        internal void Shutdown()
        {
            SocketStream.IsShut = true;
            SocketStream.Close();
        }

        /// <summary>
        /// 等待数据发送完成关闭
        /// </summary>
        internal void Close()
        {
           //
           if(queue.IsEmpty)
            {
                Shutdown();
            }
           else
            {
                Task.Factory.StartNew(() =>
                {
                    List<int> lst = new List<int>();
                    while(!queue.IsEmpty)
                    {
                        foreach (var kv in queue)
                        {
                            if(kv.Value.IsShut||kv.Value.IsEmpty)
                            {
                                kv.Value.Stop();
                                lst.Add(kv.Key);
                            }
                        }
                        if(queue.Count==lst.Count)
                        {
                            queue.Clear();
                        }
                        else if(lst.Count>0)
                        {
                            foreach(var k in lst)
                            {
                                SendQueue v;
                                queue.TryRemove(k, out v);
                            }
                        }
                        //等待完成
                        Thread.Sleep(1000);
                    }
                    Shutdown();
                });
            }

        }


        /// <summary>
        /// 发送关闭信息
        /// </summary>
        /// <param name="package"></param>
        internal void SendShut(BasePackage package)
        {
            ShutDownPackage shut = new ShutDownPackage() { DestHost = package.DestHost, DestPort = package.DestPort, PackID = package.PackID, direction=1 };
            this.SocketStream.Write(shut.DestHost, shut.DestPort, shut.GetBuffer());
        }

    }
}
