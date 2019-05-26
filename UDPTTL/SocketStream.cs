#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UDPTTL
* 项目描述 ：
* 类 名 称 ：SocketStream
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
using System.Threading.Tasks;
using UDPTTL.Package;

namespace UDPTTL
{
    public delegate void DataNotify(object sender, byte[] data, HostAndPort host);

    /* ============================================================================== 
* 功能描述：SocketStream 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class SocketStream
    {
        private readonly Socket socket = null;

      
        public event DataNotify DataCall;

        public bool IsShut { get; set; }

         
        public SocketStream(Socket socket)
        {
            this.socket = socket;
           
        }
        public void Write(EndPoint endPoint, byte[] data, int offset = 0, int length = 0)
        {

            socket.SendTo(data, offset, length == 0 ? data.Length : length, SocketFlags.None, endPoint);
        }
        public void Write(string remoteHost, int remotePort, byte[] data, int offset = 0, int length = 0)
        {
            Write(new IPEndPoint(IPAddress.Parse(remoteHost), remotePort), data, offset, length);
        }

        public void Receive()
        {
            Task.Factory.StartNew(() =>
            {
                while (!IsShut)
                {
                    byte[] buffer = new byte[PackageControl.PackSize];//直接创建
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint senderRemote = (EndPoint)sender;
                    int r = socket.ReceiveFrom(buffer, ref senderRemote);
                    if (r > 0)
                    {
                        if (DataCall != null)
                        {
                            IPEndPoint remote =(IPEndPoint) senderRemote;
                            DataCall(this, buffer, new HostAndPort() { Host =remote.Address.ToString(), Port = remote.Port });
                        }
                    }
                }
            });
           

        }

        internal void Close()
        {
            socket.Close();
        }
    }
}
