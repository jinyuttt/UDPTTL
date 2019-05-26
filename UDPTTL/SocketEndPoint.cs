#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UDPTTL
* 项目描述 ：
* 类 名 称 ：SocketEndPoint
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
using System.Net.Sockets;
using UDPTTL.Package;

namespace UDPTTL
{

    /* ============================================================================== 
* 功能描述：SocketEndPoint 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
    public class SocketEndPoint
    {
        private Socket socket = null;
        private TTLSenderSession senderSession=null;
        private TTLReceiveSession receiveSession = null;

      

        public event DataNotify DataNotifyHandler = null;

        public SocketEndPoint(Socket socket)
        {
            senderSession = new TTLSenderSession(socket);
            receiveSession = new TTLReceiveSession(senderSession);
            receiveSession.Recevice();
            receiveSession.DataReceiveSucess += DataNotify;
        }

        private void DataNotify(object sender, byte[] data, HostAndPort host)
        {
            if (DataNotifyHandler != null)
            {
                DataNotifyHandler(this, data, host);
            }
        }

        public void Send(byte[]data,int offset, int length,string host,int port)
        {
            int seq = 0;
            int len = length;
            short  curentSize = 0;
            //分包
            while (len>0)
            {
                byte[] buf = QueuePool.GetBuffers();
                if (len > buf.Length)
                {
                    len = len - buf.Length;
                    curentSize =(short)buf.Length;
                }
                else
                {
                    curentSize =(short) len;
                    len = 0;
                }
                Array.Copy(data, offset+seq*curentSize, buf, 0, curentSize);
                DATAPackage package = new DATAPackage
                {
                    Length = length,
                    PackID = Uilt.GlobID,
                    PackSeq = seq,
                    PackLength = curentSize,
                    PackSize = PackageControl.PackSize-BasePackage.PackSum,
                    Data = buf,
                    DestHost = host,
                    DestPort = port
                };
                seq++;
                senderSession.Add(package);
            }
          
        }

        internal void ShutDown()
        {
            senderSession.Shutdown();
        }

        internal void Close()
        {
            senderSession.Close();
        }
    }
}
