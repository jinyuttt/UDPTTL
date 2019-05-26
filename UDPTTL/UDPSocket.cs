#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UDPTTL
* 项目描述 ：
* 类 名 称 ：UDPClient
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
using System.Net;
using System.Net.Sockets;

namespace UDPTTL
{

    /* ============================================================================== 
* 功能描述：UDPClient 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class UDPSocket
    {
        private readonly Socket socket = null;
        private const int socketSize = 64 * 1024;
        public HostAndPort RemoteHost { get; set; }

        private readonly SocketEndPoint endPoint = null;
        public event DataNotify ReceviceData = null;
        public UDPSocket(HostAndPort local=null)
        {
            AddressFamily family = AddressFamily.InterNetwork;
            if(local==null)
            {
                local = new HostAndPort() { Host = IPAddress.Any.ToString(), Port = 0, isIP6 = false };

            }
            if(local.isIP6)
            {
                family = AddressFamily.InterNetworkV6;
            }
            socket = new Socket(family, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveBufferSize = socketSize,
                SendBufferSize = socketSize
            };
            socket.Bind(new IPEndPoint(IPAddress.Parse(local.Host), local.Port));
            endPoint = new SocketEndPoint(socket);
            endPoint.DataNotifyHandler += EndPoint_DataNotifyHandler;
        }

        private void EndPoint_DataNotifyHandler(object sender, byte[] data, HostAndPort host)
        {
            if(ReceviceData!=null)
            {
                ReceviceData(this, data, host);
            }
        }

       
       
        public void SendBytes(byte[]data,int offset,int len)
        {
            endPoint.Send(data, offset, len,RemoteHost.Host,RemoteHost.Port);
        }

        /// <summary>
        /// 等待数据发送完成关闭
        /// </summary>
        public void Close()
        {
            endPoint.Close();
        }

        /// <summary>
        /// 立即关闭，后续数据不要
        /// </summary>
        public void ShutDownNow()
        {
            endPoint.ShutDown();
        }


    }
}
