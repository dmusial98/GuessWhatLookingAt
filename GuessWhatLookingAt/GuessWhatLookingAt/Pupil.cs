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
using System.Windows.Media.Imaging;

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
        int frameHeight = 100;
        int frameWidth = 100;
        byte[] frameData = new byte[1];

        string gazeMsg;
        byte[] gazeData;

        public PupilImage image { get; set; } = new PupilImage();

        public event EventHandler<PupilReceivedDataEventArgs> PupilDataReceivedEvent;

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

               requestClient.ReceiveFrameString(); //potwierdzenie odebrania komunikatu dotyczacego formatu obrazu
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
            while (isConnected)
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

                gazeMsg = gazeSubscriber.ReceiveFrameString();
                gazeData = gazeSubscriber.ReceiveFrameBytes();

                var msgpackGazeDecode = new MsgPack();
                msgpackGazeDecode.DecodeFromBytes(gazeData);

                //poinformowanie eventem o odebraniu obrazu i wspolrzednych punku obserwacji
                var args = new PupilReceivedDataEventArgs();

                GCHandle pinnedArray = GCHandle.Alloc(frameData, GCHandleType.Pinned);
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                image.SetSourceImageFromRawBytes(pointer, frameWidth, frameHeight);

                args.pupilImage = image;
                pinnedArray.Free();

                args.xGaze = msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("norm_pos").AsArray[0].AsFloat;
                args.yGaze = msgpackGazeDecode.ForcePathObject("base_data").AsArray[0].ForcePathObject("norm_pos").AsArray[1].AsFloat;

                OnPupilReceivedData(args);
            }
        }

        protected virtual void OnPupilReceivedData(PupilReceivedDataEventArgs args)
        {
            EventHandler<PupilReceivedDataEventArgs> handler = PupilDataReceivedEvent;
            if(handler != null)
            {
                handler(this, args);
            }
        }

        public class PupilReceivedDataEventArgs: EventArgs
        {
            public PupilImage pupilImage { get; set; }

            public double xGaze { get; set; }
            public double yGaze { get; set; }
        }
    }
}

