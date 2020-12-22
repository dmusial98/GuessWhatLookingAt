using Emgu.CV;
using NetMQ;
using NetMQ.Sockets;
using SimpleMsgPack;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhatLookingAt
{
    public class Pupil
    {
        RequestSocket requestClient;
        SubscriberSocket frameSubscriber;
        SubscriberSocket gazeSubscriber;

        public bool isConnected { get; private set; }

        string subPort;
        string pubPort;

        string frameTopic = "";
        byte[] framePayload;
        int frameHeight = 100;
        int frameWidth = 100;
        byte[] frameData;

        string gazeMsg;
        byte[] gazeData;
        public Point gazePoint { get; private set; } = new Point(0, 0);

        public event EventHandler<PupilReceivedDataEventArgs> PupilDataReceivedEvent;

        PupilImage pupilImage;

        int frameNumber = 0;
        int thresholdFrameNumber = 900;

        public void Connect()
        {
            //if (requestClient == null)
            requestClient = new RequestSocket();

            requestClient.Connect("tcp://127.0.0.1:50020");

            //getting subscriber and publisher port
            requestClient.SendFrame("SUB_PORT");
            subPort = requestClient.ReceiveFrameString();
            requestClient.SendFrame("PUB_PORT");
            pubPort = requestClient.ReceiveFrameString();

            //if (frameSubscriber == null)
            frameSubscriber = new SubscriberSocket();

            //if (gazeSubscriber == null)
            gazeSubscriber = new SubscriberSocket();

            //connect to zmq subscriber port and getting frame data
            frameSubscriber.Connect("tcp://127.0.0.1:" + subPort);
            gazeSubscriber.Connect("tcp://127.0.0.1:" + subPort);

            //set up subscription on video receive
            frameSubscriber.Subscribe("frame.world");
            gazeSubscriber.Subscribe("gaze.");

            //preparing information about desired format video data
            var msgpackPackNotify = new MsgPack();
            msgpackPackNotify.ForcePathObject("subject").AsString = "frame_publishing.set_format";
            msgpackPackNotify.ForcePathObject("format").AsString = "bgr";
            var byteArrayNotify = msgpackPackNotify.Encode2Bytes();

            //sending information
            requestClient.SendMoreFrame("topic.frame_publishing.set_format")
                .SendFrame(byteArrayNotify);

            requestClient.ReceiveFrameString(); //confirm receive data

            isConnected = true;
        }

        public void ReceiveData()
        {
            pupilImage = new PupilImage();

            while (isConnected)
            {
                frameTopic = frameSubscriber.ReceiveFrameString(); //camera name
                framePayload = frameSubscriber.ReceiveFrameBytes();  //json with data describe

                MsgPack msgpackFrameDecode = new MsgPack();
                msgpackFrameDecode.DecodeFromBytes(framePayload);

                //read height and width parameters
                frameHeight = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("height").AsInteger);
                frameWidth = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("width").AsInteger);

                //receive video frame in bgr
                frameData = frameSubscriber.ReceiveFrameBytes();

                //receive gaze information
                gazeMsg = gazeSubscriber.ReceiveFrameString();
                gazeData = gazeSubscriber.ReceiveFrameBytes();

                var msgpackGazeDecode = new MsgPack();
                msgpackGazeDecode.DecodeFromBytes(gazeData);

                //new event for inform about video data
                var args = new PupilReceivedDataEventArgs();

                GCHandle pinnedArray = GCHandle.Alloc(frameData, GCHandleType.Pinned);
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();

                pupilImage.SetMat(pointer, frameWidth, frameHeight);
                pinnedArray.Free();

                gazePoint = new Point(Convert.ToInt32(msgpackGazeDecode.ForcePathObject("norm_pos").AsArray[0].AsFloat * frameWidth), 
                    Convert.ToInt32(msgpackGazeDecode.ForcePathObject("norm_pos").AsArray[1].AsFloat * frameHeight));

                pupilImage.DrawCircle(gazePoint.X, gazePoint.Y);
                pupilImage.PutConfidenceText(msgpackGazeDecode.ForcePathObject("confidence").AsFloat);
                args.image = pupilImage.GetBitmapSourceFromMat();

                OnPupilReceivedData(args);
            }

        }

        public void Disconnect()
        {
            isConnected = false;

            requestClient.Dispose();
            frameSubscriber.Dispose();
            gazeSubscriber.Dispose();
        }

        protected virtual void OnPupilReceivedData(PupilReceivedDataEventArgs args)
        {
            EventHandler<PupilReceivedDataEventArgs> handler = PupilDataReceivedEvent;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public class PupilReceivedDataEventArgs : EventArgs
        {
            public BitmapSource image { get; set; }
        }
    }
}

