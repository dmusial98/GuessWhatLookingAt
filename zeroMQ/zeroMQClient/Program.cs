using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace zeroMQClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new RequestSocket())
            {
                client.Connect("tcp://127.0.0.1:5556");
                client.SendFrame("Hello");
                var msg = client.ReceiveFrameString();
                Console.WriteLine("From Server: {0}", msg);
            }

            Console.ReadKey();
        }
    }
}
