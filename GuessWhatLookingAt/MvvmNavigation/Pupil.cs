using NetMQ;
using NetMQ.Sockets;
using SimpleMsgPack;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GuessWhatLookingAt
{
    public class Pupil
    {
        RequestSocket requestClient;
        SubscriberSocket frameSubscriber;
        SubscriberSocket gazeSubscriber;

        public bool isConnected { get; private set; } = false;

        string subPort;
        string pubPort;

        string frameTopic = "";
        byte[] framePayload;
        int frameHeight = 10;
        int frameWidth = 10;
        byte[] frameData;

        string gazeMsg;
        byte[] gazeData;

        public Point gazePoint { get; private set; } = new Point(0, 0);
        public double gazeConfidence = 0;

        public event EventHandler<PupilReceivedDataEventArgs> PupilDataReceivedEvent;
        public event EventHandler<ImageScaleChangedEventArgs> ImageScaleChangedEvent;

        public Size ImageSizeToDisplay { get; set; }

        double _imageXScale = 1.0;
        double _imageYScale = 1.0;

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

        public void ReceiveFrame()
        {
            //pupilImage = new EmguCVImage();

            while (isConnected)
            {
                frameTopic = frameSubscriber.ReceiveFrameString(); //camera name
                framePayload = frameSubscriber.ReceiveFrameBytes();  //json with data describe

                MsgPack msgpackFrameDecode = new MsgPack();
                msgpackFrameDecode.DecodeFromBytes(framePayload);

                //new size of image and new scale for image
                if (Convert.ToInt32(msgpackFrameDecode.ForcePathObject("height").AsInteger) != frameHeight ||
                    Convert.ToInt32(msgpackFrameDecode.ForcePathObject("width").AsInteger) != frameWidth)
                {
                    //write height and width parameters
                    frameHeight = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("height").AsInteger);
                    frameWidth = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("width").AsInteger);

                    _imageXScale = ImageSizeToDisplay.Width / frameWidth;
                    _imageYScale = ImageSizeToDisplay.Height / frameHeight;

                    //notify about event occurence
                    var imageScaleArgs = new ImageScaleChangedEventArgs();
                    imageScaleArgs.XScaleImage = _imageXScale;
                    imageScaleArgs.YScaleImage = _imageYScale;
                    OnImageScaleChanged(imageScaleArgs);
                }

                //receive video frame in bgr
                frameData = frameSubscriber.ReceiveFrameBytes();

                //receive gaze information
                gazeMsg = gazeSubscriber.ReceiveFrameString();
                gazeData = gazeSubscriber.ReceiveFrameBytes();

                var msgpackGazeDecode = new MsgPack();
                msgpackGazeDecode.DecodeFromBytes(gazeData);

                //new event for inform about video data
                var imageArgs = new PupilReceivedDataEventArgs();

                gazePoint = imageArgs.gazePoint = new Point(
                    msgpackGazeDecode.ForcePathObject("norm_pos").AsArray[0].AsFloat * frameWidth,
                    msgpackGazeDecode.ForcePathObject("norm_pos").AsArray[1].AsFloat * frameHeight);
                gazeConfidence = imageArgs.gazeConfidence = msgpackGazeDecode.ForcePathObject("confidence").AsFloat;

                imageArgs.rawImageData = frameData;
                imageArgs.imageSize = new Size(frameWidth, frameHeight);
                imageArgs.imageXScale = _imageXScale;
                imageArgs.imageYScale = _imageYScale;

                OnPupilReceivedData(imageArgs);
            }
        }

        public void ReceiveGaze()
        {
            while (isConnected)
            {
                //TODO: zrobic w nastepnym watku odbieranie rownolegle wszystkich gazeow i wyswietlenie ich


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

        protected virtual void OnImageScaleChanged(ImageScaleChangedEventArgs args)
        {
            EventHandler<ImageScaleChangedEventArgs> handler = ImageScaleChangedEvent;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public class PupilReceivedDataEventArgs : EventArgs
        {
            public byte[] rawImageData { get; set; }
            public Point gazePoint { get; set; }
            public double gazeConfidence { get; set; }
            public Size imageSize { get; set; }
            public double imageXScale { get; set; }
            public double imageYScale { get; set; }
        }

        public class ImageScaleChangedEventArgs : EventArgs
        {
            public double XScaleImage { get; set; }
            public double YScaleImage { get; set; }
        }
    }
}

