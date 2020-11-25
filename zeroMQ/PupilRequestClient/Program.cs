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
    class Program
    {
        static void Main(string[] args)
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

                    //decimal[] norm_pos = { 0.1295810100458894m, 0.3207013110574589m };
                    //var ellipse = new PupilGaze3d.Ellipse
                    //{
                    //    angle = -18.24879778134816m
                    //};

                    //var serializeContent = new PupilGaze3d
                    //{
                    //    id = 0,
                    //    topic = "pupil.0.3d",
                    //    method = "3d c++",
                    //    norm_pos = norm_pos,
                    //    diameter = 52.36177621161693m,
                    //    timestamp = 114229.990821m,
                    //    confidence = 0.9989116277448433m,
                    //    ellipse = ellipse,
                    //};

                    try
                    {
                        //open file for saving data
                        StreamWriter sw = new StreamWriter("C:\\Users\\dmusi\\source\\repos\\GuessWhatLookingAt\\zeroMQ\\PupilRequestClient\\Gaze_Pupil.txt");

                        //for (int i = 0; i < 10; i++)
                        //{
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
                        //Console.WriteLine("method: {0}", unpackMsgPack.ForcePathObject("method").AsString);
                        Console.WriteLine("confidence: {0}", unpackMsgPack.ForcePathObject("confidence").AsFloat);
                        Console.WriteLine("phi: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("phi").AsFloat);
                        Console.WriteLine("theta: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("theta").AsFloat);

                        sw.WriteLine("method: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("method").AsString);
                        sw.WriteLine("topic: {0}", unpackMsgPack.ForcePathObject("topic").AsString);
                        //Console.WriteLine("method: {0}", unpackMsgPack.ForcePathObject("method").AsString);
                        sw.WriteLine("confidence: {0}", unpackMsgPack.ForcePathObject("confidence").AsFloat);
                        sw.WriteLine("phi: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("phi").AsFloat);
                        sw.WriteLine("theta: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("theta").AsFloat);

                        Console.ReadKey();
                        //}

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
