using System;
using System.Text;
using System.Threading;
using UDPTTL;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            UDPSocket dPSocket = new UDPSocket();
            dPSocket.RemoteHost = new HostAndPort() { Host = "127.0.0.1", port = 5555 };
            while (true)
            {
                byte[] bytes = Encoding.Default.GetBytes(DateTime.Now.ToString());
                dPSocket.SendBytes(bytes, 0, bytes.Length);
                Thread.Sleep(1000);
            }
           
        }
    }
}
