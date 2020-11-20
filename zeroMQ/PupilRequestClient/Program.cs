using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.Sockets;
using NetMQ;

using System.IO;

namespace PupilRequestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new RequestSocket())
            {
                client.Connect("tcp://127.0.0.1:50020");

                client.SendFrame("SUB_PORT");
                var subPort = client.ReceiveFrameString();
                Console.WriteLine("SUB_PORT: {0}", subPort);

                client.SendFrame("PUB_PORT");
                var pubPort = client.ReceiveFrameString();
                Console.WriteLine("PUB_PORT: {0}", pubPort);

                using (var subscriber = new SubscriberSocket())
                {
                    subscriber.Connect("tcp://127.0.0.1:" + subPort );
                    subscriber.Subscribe("gaze.");

                   
                        try
                        {
                            //Pass the filepath and filename to the StreamWriter Constructor
                            StreamWriter sw = new StreamWriter("C:\\Users\\dmusi\\source\\repos\\zeroMQ\\PupilRequestClient\\Gaze_Pupil.txt");

                        for (int i = 0; i < 5; i++) 
                        {
                            var gaze = subscriber.ReceiveFrameBytes();
                            var msg = subscriber.ReceiveFrameString();

                            sw.WriteLine("Data length: {0}", gaze.Length);
                            Console.WriteLine("Data length: {0}", gaze.Length);
                            foreach (var element in gaze)
                            {
                                sw.Write("{0} ", element.ToString("X"));
                                Console.WriteLine("0x{0} ", element.ToString("X"));
                            }
                            
                            sw.WriteLine();
                            Console.ReadKey();
                            }

                            //Close the file
                            sw.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception: " + e.Message);
                        }
                        finally 
                        {
                            Console.WriteLine("Executing finally block.");
                        }

                    Console.ReadKey();
                }   
            }
        }
    }
}
