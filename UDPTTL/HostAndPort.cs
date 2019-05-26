#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：UDPTTL
* 项目描述 ：
* 类 名 称 ：HostAndPort
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
using System.Text;

namespace UDPTTL
{

    /* ============================================================================== 
* 功能描述：HostAndPort 
* 创 建 者：jinyu 
* 创建日期：2019 
* 更新时间 ：2019
* ==============================================================================*/
   public class HostAndPort
    {
        public string Host;
        public int Port;
        public bool isIP6;
        public HostAndPort()
        {
            Host = "127.0.0.1";
            Port = 0;
            isIP6 = false;
        }
        public HostAndPort(int port=0,string host="127.0.0.1",bool isIP6=false)
        {
            Host = "127.0.0.1";
            port = 0;
            isIP6 = false;
        }

        public override string ToString()
        {
            return Host + ":" + Port;
        }
    }
}
