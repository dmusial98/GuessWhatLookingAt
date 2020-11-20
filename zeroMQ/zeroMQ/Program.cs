using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using System.Net;
using NetMQ.Sockets;

namespace zeroMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var server = new ResponseSocket())
            {
                server.Bind("tcp://*:5556");
             
                while (true)
                {
                    string msg = server.ReceiveFrameString();
                    Console.WriteLine("From Client: {0}", msg);
                    server.SendFrame("World");
                }
            }

        }
    }
}
