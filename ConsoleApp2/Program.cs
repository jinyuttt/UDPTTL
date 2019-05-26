using System;
using System.Text;
using UDPTTL;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {

            UDPSocket uDPSocket = new UDPSocket(new HostAndPort() {  Host="127.0.0.1", port=5555});
         
            uDPSocket.ReceviceData += UDPSocket_ReceviceData;
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        private static void UDPSocket_ReceviceData(object sender, byte[] data, HostAndPort host)
        {
            Console.WriteLine(Encoding.UTF8.GetString(data));
        }
    }
}
