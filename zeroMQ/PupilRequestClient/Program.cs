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
                    //subscriber.Subscribe("gaze.");

                    try
                    {
                        //open file for saving data
                        StreamWriter sw = new StreamWriter("C:\\Users\\dmusi\\source\\repos\\GuessWhatLookingAt\\zeroMQ\\PupilRequestClient\\Gaze_Pupil.txt");

                        //for (int i = 0; i < 10; i++)
                        //{
                        //var msg = subscriber.ReceiveFrameString();
                        //var gaze = subscriber.ReceiveFrameBytes();

                        //sw.WriteLine("Data length: {0}", gaze.Length);
                        //sw.WriteLine("Text: {0}", msg);

                        //Console.WriteLine("Data length: {0}", gaze.Length);
                        //Console.WriteLine("Text: {0}", msg);

                        //foreach (var element in gaze)
                        //{
                        //    sw.Write("{0} ", element.ToString("X"));
                        //    Console.WriteLine("0x{0} ", element.ToString("X"));
                        //}

                        //sw.WriteLine();

                        //MsgPack unpackMsgPack = new MsgPack();
                        //unpackMsgPack.DecodeFromBytes(gaze);

                        //var baseData = unpackMsgPack.ForcePathObject("base_data").AsArray;

                        //Console.WriteLine("method: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("method").AsString);
                        //Console.WriteLine("topic: {0}", unpackMsgPack.ForcePathObject("topic").AsString);
                        //Console.WriteLine("confidence: {0}", unpackMsgPack.ForcePathObject("confidence").AsFloat);
                        //Console.WriteLine("phi: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("phi").AsFloat);
                        //Console.WriteLine("theta: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("theta").AsFloat);

                        //sw.WriteLine("method: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("method").AsString);
                        //sw.WriteLine("topic: {0}", unpackMsgPack.ForcePathObject("topic").AsString);
                        //sw.WriteLine("confidence: {0}", unpackMsgPack.ForcePathObject("confidence").AsFloat);
                        //sw.WriteLine("phi: {0}", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("phi").AsFloat);
                        //sw.WriteLine("theta: {0}\n", unpackMsgPack.ForcePathObject("base_data").AsArray[0].ForcePathObject("theta").AsFloat);

                        subscriber.Subscribe("frame.");

          
                        var msgpackPackNotify = new MsgPack();
                        msgpackPackNotify.ForcePathObject("subject").AsString = "frame_publishing.set_format";
                        msgpackPackNotify.ForcePathObject("format").AsString = "bgr";
                        var byteArrayNotify = msgpackPackNotify.Encode2Bytes();

                        foreach(byte element in byteArrayNotify)
                        {
                            sw.Write("{0} ", element.ToString("X"));
                        }

                        sw.WriteLine("\n");

                        client.SendMoreFrame("topic.frame_publishing.set_format")
                            .SendFrame(byteArrayNotify);

                        Console.WriteLine();
                        Console.WriteLine("{0}", client.ReceiveFrameString());


                        string topic = subscriber.ReceiveFrameString(); //nazwa kamery
                        var payload = subscriber.ReceiveFrameBytes();  //json z opisem danych

                        var msgpackFrameDecode = new MsgPack();
                        msgpackFrameDecode.DecodeFromBytes(payload);

                        long height = msgpackFrameDecode.ForcePathObject("height").AsInteger;
                        long width = msgpackFrameDecode.ForcePathObject("width").AsInteger;
                        var rawData = msgpackFrameDecode.ForcePathObject("__raw_data__").AsArray;


                        sw.WriteLine(topic);
                        Console.WriteLine(topic);

                        foreach(byte element in payload)
                        {
                            sw.Write("{0} ", element.ToString("X"));
                        }

                        sw.WriteLine("\n");

                        payload = subscriber.ReceiveFrameBytes();

                        var msgpackFrameDecode2 = new MsgPack();
                        msgpackFrameDecode2.DecodeFromBytes(payload);
                        height = msgpackFrameDecode2.ForcePathObject("height").AsInteger;
                        width = msgpackFrameDecode2.ForcePathObject("width").AsInteger;
                        rawData = msgpackFrameDecode2.ForcePathObject("__raw_data__").AsArray;

                        foreach (byte element in payload)
                        {
                            sw.Write("{0} ", element.ToString("X"));
                        }

                        sw.WriteLine("\n");

                        topic = subscriber.ReceiveFrameString();

                        payload = subscriber.ReceiveFrameBytes();

                        //foreach (byte element in payload)
                        //{
                        //    sw.Write("{0} ", element.ToString("X"));
                        //}

                        sw.WriteLine(topic);

                        sw.WriteLine("\n");
                        //}

                        foreach (byte element in payload)
                        {
                            sw.Write("{0} ", element.ToString("X"));
                        }

                        sw.WriteLine("\n");

                        payload = subscriber.ReceiveFrameBytes();

                        foreach (byte element in payload)
                        {
                            sw.Write("{0} ", element.ToString("X"));
                        }

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
