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
using System.Collections.Concurrent;
using UDPTTL.Package;
namespace UDPTTL
{

    /* ============================================================================== 
* 功能描述：TTLReceiveSession 每个socket接收数据的会话，不论接收发送都有
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class TTLReceiveSession
    {
      
        private readonly TTLSenderSession senderSession = null;
        public DataNotify DataReceiveSucess;
        private readonly ConcurrentDictionary<string, HostReceive> hostQueue = null;
        private readonly System.Timers.Timer timer = null;
        private const int IntervalTime = 60 * 1000;//1分钟
        private const int CacheHostTime = 10;//相当于10分钟

        public TTLReceiveSession(TTLSenderSession session)
        {
            senderSession = session;
            hostQueue = new ConcurrentDictionary<string, HostReceive>();
            timer = new System.Timers.Timer();
            timer.Interval = IntervalTime;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //监测数据
            foreach(var kv in hostQueue)
            {
                kv.Value.CountTime++;//在这里计时
                if(kv.Value.CountTime>kv.Value.LastUseTime+CacheHostTime)
                {
                    //该端点已经超时没有使用了
                    HostReceive hostReceive = null;
                    if(hostQueue.TryRemove(kv.Key,out hostReceive))
                    {
                        hostReceive.Stop();
                        hostReceive.dataNotify -= this.ReceiveData;
                        Console.WriteLine(string.Format("{0}空闲移除", hostReceive.Host));
                    }
                   
                }
            }
            
        }

        /// <summary>
        /// 接收；
        /// 
        /// </summary>
        public void Recevice()
        {
            //接收其实是TTLSenderSession启动的
            senderSession.Receive();
            //接收的数据返回
            senderSession.Regster(this.Stream_DataCall);
        }

        private void AddPackage(HostAndPort host,BasePackage package)
        {
            HostReceive receive;
            if(hostQueue.TryGetValue(host.Host+host.Port,out receive))
            {
                if(receive.IsShut)
                {
                    hostQueue.TryRemove(host.Host + host.Port, out receive);
                    AddPackage(host, package);
                    return;
                }
                receive.Add(package);
                receive.LastUseTime = receive.CountTime;
            }
            else
            {
                receive = new HostReceive(senderSession) { Host = host };
                receive.dataNotify += ReceiveData;
                receive.Add(package);
                hostQueue[host.Host + host.Port] = receive;
                receive.LastUseTime = receive.CountTime;//记录使用
            }
        }
        private void ReceiveData(object sender, byte[] data, HostAndPort host)
        {
            if (DataReceiveSucess != null)
            {
                DataReceiveSucess.Invoke(sender, data, host);
            }
        }

        /// <summary>
        /// 从接收session接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <param name="host"></param>
        private void Stream_DataCall(object sender, byte[] data, HostAndPort host)
        {
            //创建类型
            BasePackage package = new BasePackage(data);
            //所有接收信息都经过
            switch(package.PackType)
            {
                case PackageControl.Data:
                    package = new DATAPackage(data);
                    break;
                case PackageControl.Ack:
                    package = new ACKPackage(data);
                    break;
                case PackageControl.Fin:
                    package = new  FINPackage(data);
                    break;
                case PackageControl.Lost:
                    package = new LostPackage(data);
                    break;
                case PackageControl.ShutDown:
                    package = new ShutDownPackage(data);
                    break;
                case PackageControl.Syn:
                    package = new SYNPackage(data);
                    break;
            }
            package.DestHost = host.Host;
            package.DestPort = host.Port;
            AddPackage(host, package);
        }




    }
}
