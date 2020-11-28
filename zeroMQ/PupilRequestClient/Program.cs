using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ.Sockets;
using NetMQ;
using SimpleMsgPack;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;

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
                   
                    try
                    {
                        //open file for saving data
                        StreamWriter sw = new StreamWriter("C:\\Users\\dmusi\\source\\repos\\GuessWhatLookingAt\\zeroMQ\\PupilRequestClient\\Gaze_Pupil.txt");

                        //ustawienie subskrbcji na odbieranie obrazu
                        subscriber.Subscribe("frame.");

                        //przygotowanie informacji o pozadanym formacie odbieranych danych dotyczacych obrazu
                        var msgpackPackNotify = new MsgPack();
                        msgpackPackNotify.ForcePathObject("subject").AsString = "frame_publishing.set_format";
                        msgpackPackNotify.ForcePathObject("format").AsString = "bgr";
                        var byteArrayNotify = msgpackPackNotify.Encode2Bytes();

                        foreach(byte element in byteArrayNotify)
                        {
                            sw.Write("{0} ", element.ToString("X"));
                        }

                        sw.WriteLine("\n");

                        //wysylanie informacji o pozadanym formacie odbieranych danych dotyczacych obrazu
                        client.SendMoreFrame("topic.frame_publishing.set_format")
                            .SendFrame(byteArrayNotify);

                        Console.WriteLine();
                        Console.WriteLine("{0}", client.ReceiveFrameString());

                        bool IsMainCamera = false;
                        string topic = "";
                        byte[] payload = new byte[1];
                        long height;
                        long width;
                        MsgPack msgpackFrameDecode = new MsgPack();

                        while (!IsMainCamera)
                        {
                            //odebranie nazwy i parametrow obrazu 
                            topic = subscriber.ReceiveFrameString(); //nazwa kamery
                            payload = subscriber.ReceiveFrameBytes();  //json z opisem danych
                            msgpackFrameDecode.DecodeFromBytes(payload);

                            if (msgpackFrameDecode.ForcePathObject("topic").AsString == "frame.world")
                                IsMainCamera = true;
                        }
                        

                        //odczytanie parametrow obrazu
                        height = msgpackFrameDecode.ForcePathObject("height").AsInteger;
                        width = msgpackFrameDecode.ForcePathObject("width").AsInteger;

                        sw.WriteLine(topic);
                        Console.WriteLine(topic);

                        foreach(byte element in payload)
                        {
                            sw.Write("{0} ", element.ToString("X"));
                        }

                        sw.WriteLine("\n");


                        //odebranie obrazu w formacie bgr
                        var data = subscriber.ReceiveFrameBytes();


                        String PupilWindow = "Pupil Window"; //The name of the window
                        CvInvoke.NamedWindow(PupilWindow); //Create the window using the specific name

                        //Mat image = new Mat(Convert.ToInt32(height), Convert.ToInt32(width), DepthType.Cv8U, 3);
                        GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
                        IntPtr pointer = pinnedArray.AddrOfPinnedObject();  
                        Mat Image2 = new Mat( Convert.ToInt32(height), Convert.ToInt32(width), DepthType.Cv8U, 3, pointer, Convert.ToInt32(width) * 3);
                        pinnedArray.Free();

                        CvInvoke.Imshow(PupilWindow, Image2); //Show the image
                        CvInvoke.WaitKey(0);  //Wait for the key pressing event
                        CvInvoke.DestroyWindow(PupilWindow); //Destroy the window if key is pressed


                      

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
