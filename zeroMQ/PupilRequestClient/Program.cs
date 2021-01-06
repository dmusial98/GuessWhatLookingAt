using System;
using NetMQ.Sockets;
using NetMQ;
using SimpleMsgPack;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;
using System.Threading;

namespace PupilRequestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread thread = new Thread(GetPupilData);
            thread.Start();
        }

        public static void GetPupilData()
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

                        frameSubscriber.Subscribe("frame.world");
                        gazeSubscriber.Subscribe("gaze.3d.01.");

                        var msgpackPackNotify = new MsgPack();
                        msgpackPackNotify.ForcePathObject("subject").AsString = "frame_publishing.set_format";
                        msgpackPackNotify.ForcePathObject("format").AsString = "bgr";
                        var byteArrayNotify = msgpackPackNotify.Encode2Bytes();


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

                            frameTopic = frameSubscriber.ReceiveFrameString();
                            framePpayload = frameSubscriber.ReceiveFrameBytes();  

                            MsgPack msgpackFrameDecode = new MsgPack();
                            msgpackFrameDecode.DecodeFromBytes(framePpayload);


                            frameHeight = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("height").AsInteger);
                            frameWidth = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("width").AsInteger);

                            frameData = frameSubscriber.ReceiveFrameBytes();

                            GCHandle pinnedArray = GCHandle.Alloc(frameData, GCHandleType.Pinned);
                            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                            Mat image = new Mat(Convert.ToInt32(frameHeight), Convert.ToInt32(frameWidth), DepthType.Cv8U, 3, pointer, Convert.ToInt32(frameWidth) * 3);
                            pinnedArray.Free();

                            CvInvoke.Imshow(PupilWindow, image);
                            CvInvoke.WaitKey(1);  

                            gazeMsg = gazeSubscriber.ReceiveFrameString();
                            gazeData = gazeSubscriber.ReceiveFrameBytes();
                            
                            var msgpackGazeDecode = new MsgPack();
                            msgpackGazeDecode.DecodeFromBytes(gazeData);


                            Console.WriteLine("video timestamp: {0}", msgpackFrameDecode.ForcePathObject("timestamp").AsFloat);
                            Console.WriteLine("timestamp: {0}", msgpackGazeDecode.ForcePathObject("timestamp").AsFloat);
                            Console.WriteLine("method: {0}", msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("method").AsString);
                            Console.WriteLine("topic: {0}", msgpackGazeDecode.ForcePathObject("topic").AsString);
                            Console.WriteLine("norm_pos: [{0}, {1}]", msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("norm_pos").AsArray[0].AsFloat, msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("norm_pos").AsArray[1].AsFloat);
                            Console.WriteLine("confidence: {0}", msgpackGazeDecode.ForcePathObject("confidence").AsFloat);
                            Console.WriteLine("phi: {0}", msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("phi").AsFloat);
                            Console.WriteLine("theta: {0}", msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("theta").AsFloat);
                            Console.WriteLine("\n");
                        }
                    }
                }
            }
        }
    }
}
