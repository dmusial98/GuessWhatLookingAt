using Emgu.CV;
using Emgu.CV.CvEnum;
using NetMQ;
using NetMQ.Sockets;
using SimpleMsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhatLookingAt
{
    public class Pupil
    {
        public bool isConnected { get; private set; }

        RequestSocket requestClient;
        SubscriberSocket frameSubscriber;
        SubscriberSocket gazeSubscriber;
        string subPort;
        string pubPort;


        string frameTopic = "";
        byte[] framePpayload = new byte[1];
        long frameHeight = 100;
        long frameWidth = 100;
        byte[] frameData = new byte[1];

        string gazeMsg;
        byte[] gazeData;


        Image image = new Image();

        public bool ConnectWithPupil()
        {
            try
            {
                requestClient = new RequestSocket();
                requestClient.Connect("tcp://127.0.0.1:50020");

                //getting subscriber and publisher port
                requestClient.SendFrame("SUB_PORT");
                subPort = requestClient.ReceiveFrameString();
                requestClient.SendFrame("PUB_PORT");
                pubPort = requestClient.ReceiveFrameString();

                //connect to zmq subscriber port and getting frame data
                frameSubscriber = new SubscriberSocket();
                gazeSubscriber = new SubscriberSocket();
                frameSubscriber.Connect("tcp://127.0.0.1:" + subPort);
                gazeSubscriber.Connect("tcp://127.0.0.1:" + subPort);


                //ustawienie subskrbcji na odbieranie obrazu
                frameSubscriber.Subscribe("frame.world");
                gazeSubscriber.Subscribe("gaze.");

                //przygotowanie informacji o pozadanym formacie odbieranych danych dotyczacych obrazu
                var msgpackPackNotify = new MsgPack();
                msgpackPackNotify.ForcePathObject("subject").AsString = "frame_publishing.set_format";
                msgpackPackNotify.ForcePathObject("format").AsString = "bgr";
                var byteArrayNotify = msgpackPackNotify.Encode2Bytes();

                //wysylanie informacji o pozadanym formacie odbieranych danych dotyczacych obrazu
                requestClient.SendMoreFrame("topic.frame_publishing.set_format")
                    .SendFrame(byteArrayNotify);

                Console.WriteLine("{0}", requestClient.ReceiveFrameString()); //potwierdzenie odebrania komunikatu dotyczacego formatu obrazu
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                isConnected = false;
                return isConnected;
            }

            isConnected = true;
            return isConnected;
        }

        public void ReceiveData()
        {
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

                gazeMsg = gazeSubscriber.ReceiveFrameString();
                gazeData = gazeSubscriber.ReceiveFrameBytes();

                var msgpackGazeDecode = new MsgPack();
                msgpackGazeDecode.DecodeFromBytes(gazeData);

                Console.WriteLine("norm_pos: [{0}, {1}]", msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("norm_pos").AsArray[0].AsFloat, msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("norm_pos").AsArray[1].AsFloat);
            
                //poinformowanie eventem o odebraniu obrazu i wspolrzednych punku obserwacji
                /////
                ///
                ///
                ////
                ///

            }
        }
    }
}

