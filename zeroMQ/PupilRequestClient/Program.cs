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

                    String PupilWindow = "Pupil Window"; //The name of the window
                    CvInvoke.NamedWindow(PupilWindow); //Create the window using the specific name

                    try
                    {
                        //ustawienie subskrbcji na odbieranie obrazu
                        subscriber.Subscribe("frame.world");

                        //przygotowanie informacji o pozadanym formacie odbieranych danych dotyczacych obrazu
                        var msgpackPackNotify = new MsgPack();
                        msgpackPackNotify.ForcePathObject("subject").AsString = "frame_publishing.set_format";
                        msgpackPackNotify.ForcePathObject("format").AsString = "bgr";
                        var byteArrayNotify = msgpackPackNotify.Encode2Bytes();

                        //wysylanie informacji o pozadanym formacie odbieranych danych dotyczacych obrazu
                        client.SendMoreFrame("topic.frame_publishing.set_format")
                            .SendFrame(byteArrayNotify);

                        Console.WriteLine("{0}", client.ReceiveFrameString());

                        string topic = "";
                        byte[] payload = new byte[1];
                        long height = 100;
                        long width = 100;
                        byte[] data = new byte[1];



                        while (true)
                        {
                            //odebranie nazwy i parametrow obrazu 
                            topic = subscriber.ReceiveFrameString(); //nazwa kamery
                            payload = subscriber.ReceiveFrameBytes();  //json z opisem danych

                            MsgPack msgpackFrameDecode = new MsgPack();
                            msgpackFrameDecode.DecodeFromBytes(payload);

                            //odczytanie parametrow obrazu

                            height = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("height").AsInteger);
                            width = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("width").AsInteger);

                            //odebranie obrazu w formacie bgr
                            data = subscriber.ReceiveFrameBytes();
                           
                            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
                            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                            Mat image = new Mat(Convert.ToInt32(height), Convert.ToInt32(width), DepthType.Cv8U, 3, pointer, Convert.ToInt32(width) * 3);
                            pinnedArray.Free();

                            CvInvoke.Imshow(PupilWindow, image); //Show the image
                            CvInvoke.WaitKey(1);  //Wait for the key pressing event
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                    finally
                    {
                        CvInvoke.DestroyWindow(PupilWindow); //Destroy the window if key is pressed
                    }

                    Console.ReadKey();
                }
            }
        }
    }
}
