using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.Sockets;
using NetMQ;
using SimpleMsgPack;
using System.IO;
namespace PupilRequestClient
{
    class GazeDownloadPupilLabs
    {
        static void DownloadGaze()
        {
            using (var client = new RequestSocket())
            {
                client.Connect("tcp://127.0.0.1:50020");


                //getting subscriber and publisher port
                client.SendFrame("SUB_PORT");
                var subPort = client.ReceiveFrameString();
                Console.WriteLine("SUB_PORT: {0}", subPort);

                client.SendFrame("PUB_PORT");
                var pubPort = client.ReceiveFrameString();
                Console.WriteLine("PUB_PORT: {0}", pubPort);

                using (var subscriber = new SubscriberSocket())
                {
                    //connect to zmq subscriber port and getting gaze data
                    subscriber.Connect("tcp://127.0.0.1:" + subPort);
                    subscriber.Subscribe("gaze.");

                    try
                    {
                        //open file for saving data
                        StreamWriter sw = new StreamWriter("C:\\Users\\dmusi\\source\\repos\\GuessWhatLookingAt\\zeroMQ\\PupilRequestClient\\Gaze_Pupil.txt");

                        var msg = subscriber.ReceiveFrameString();
                        var gaze = subscriber.ReceiveFrameBytes();

                        sw.WriteLine("Data length: {0}", gaze.Length);
                        sw.WriteLine("Text: {0}", msg);

                        Console.WriteLine("Data length: {0}", gaze.Length);
                        Console.WriteLine("Text: {0}", msg);

                        foreach (var element in gaze)
                        {
                            sw.Write("{0} ", element.ToString("X"));
                            Console.WriteLine("0x{0} ", element.ToString("X"));
                        }

                        sw.WriteLine();

                        MsgPack unpackMsgPack = new MsgPack();
                        unpackMsgPack.DecodeFromBytes(gaze);

                        var baseData = unpackMsgPack.ForcePathObject("base_data").AsArray;

                        Console.WriteLine("method: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("method").AsString);
                        Console.WriteLine("topic: {0}", unpackMsgPack.ForcePathObject("topic").AsString);
                        Console.WriteLine("confidence: {0}", unpackMsgPack.ForcePathObject("confidence").AsFloat);
                        Console.WriteLine("phi: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("phi").AsFloat);
                        Console.WriteLine("theta: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("theta").AsFloat);

                        sw.WriteLine("method: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("method").AsString);
                        sw.WriteLine("topic: {0}", unpackMsgPack.ForcePathObject("topic").AsString);
                        sw.WriteLine("confidence: {0}", unpackMsgPack.ForcePathObject("confidence").AsFloat);
                        sw.WriteLine("phi: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("phi").AsFloat);
                        sw.WriteLine("theta: {0}\n", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("theta").AsFloat);

                        sw.Close();
                        Console.ReadKey();

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
