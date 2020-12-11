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

                using (var frameSubscriber = new SubscriberSocket())
                {
                    using (var gazeSubscriber = new SubscriberSocket())
                    {
                        //connect to zmq subscriber port and getting frame data
                        frameSubscriber.Connect("tcp://127.0.0.1:" + subPort);
                        gazeSubscriber.Connect("tcp://127.0.0.1:" + subPort);

                        String PupilWindow = "Pupil Window"; //The name of the window
                        CvInvoke.NamedWindow(PupilWindow); //Create the window using the specific name

                        try
                        {
                            //ustawienie subskrbcji na odbieranie obrazu
                            frameSubscriber.Subscribe("frame.world");
                            gazeSubscriber.Subscribe("gaze.");

                            //przygotowanie informacji o pozadanym formacie odbieranych danych dotyczacych obrazu
                            var msgpackPackNotify = new MsgPack();
                            msgpackPackNotify.ForcePathObject("subject").AsString = "frame_publishing.set_format";
                            msgpackPackNotify.ForcePathObject("format").AsString = "bgr";
                            var byteArrayNotify = msgpackPackNotify.Encode2Bytes();

                            //wysylanie informacji o pozadanym formacie odbieranych danych dotyczacych obrazu
                            client.SendMoreFrame("topic.frame_publishing.set_format")
                                .SendFrame(byteArrayNotify);

                            Console.WriteLine("{0}", client.ReceiveFrameString());

                            string frameTopic = "";
                            byte[] framePpayload = new byte[1];
                            long frameHeight = 100;
                            long frameWidth = 100;
                            byte[] frameData = new byte[1];

                            string gazeMsg;
                            byte[] gazeData;

                            while (true)
                            {
                                //odebranie nazwy i parametrow obrazu 
                                frameTopic = frameSubscriber.ReceiveFrameString(); //nazwa kamery
                                framePpayload = frameSubscriber.ReceiveFrameBytes();  //json z opisem danych

                                MsgPack msgpackFrameDecode = new MsgPack();
                                msgpackFrameDecode.DecodeFromBytes(framePpayload);

                                //odczytanie parametrow obrazu

                                frameHeight = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("height").AsInteger);
                                frameWidth = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("width").AsInteger);

                                //odebranie obrazu w formacie bgr
                                frameData = frameSubscriber.ReceiveFrameBytes();

                                GCHandle pinnedArray = GCHandle.Alloc(frameData, GCHandleType.Pinned);
                                IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                                Mat image = new Mat(Convert.ToInt32(frameHeight), Convert.ToInt32(frameWidth), DepthType.Cv8U, 3, pointer, Convert.ToInt32(frameWidth) * 3);
                                pinnedArray.Free();

                                CvInvoke.Imshow(PupilWindow, image); //Show the image
                                CvInvoke.WaitKey(1);  //Wait for the key pressing event

                                gazeMsg = gazeSubscriber.ReceiveFrameString();
                                gazeData = gazeSubscriber.ReceiveFrameBytes();

                                var msgpackGazeDecode = new MsgPack();
                                msgpackGazeDecode.DecodeFromBytes(gazeData);

                                Console.WriteLine("method: {0}", msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("method").AsString);
                                Console.WriteLine("topic: {0}", msgpackGazeDecode.ForcePathObject("topic").AsString);
                                Console.WriteLine("norm_pos: [{0}, {1}]", msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("norm_pos").AsArray[0].AsFloat, msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("norm_pos").AsArray[1].AsFloat);
                                Console.WriteLine("confidence: {0}", msgpackGazeDecode.ForcePathObject("confidence").AsFloat);
                                Console.WriteLine("phi: {0}", msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("phi").AsFloat);
                                Console.WriteLine("theta: {0}", msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("theta").AsFloat);
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
}
