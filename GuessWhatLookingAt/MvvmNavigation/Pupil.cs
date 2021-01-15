using NetMQ;
using NetMQ.Sockets;
using SimpleMsgPack;
using System;
using System.Collections.Generic;
using System.Windows;

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

        System.Threading.Thread frameThread;

        string frameTopic = "";
        byte[] framePayload;
        int frameHeight = 10;
        int frameWidth = 10;
        byte[] frameData;

        string gazeMsg;
        byte[] gazeData;

        public Point gazePoint { get; private set; } = new Point(0, 0);

        public event EventHandler<PupilReceivedDataEventArgs> PupilDataReceivedEvent;
        //public event EventHandler<ImageScaleChangedEventArgs> ImageScaleChangedEvent;

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

            frameThread = new System.Threading.Thread(ReceiveFrame);

            frameThread.Start();
        }

        public void ReceiveFrame()
        {
            while (isConnected)
            {
                frameTopic = frameSubscriber.ReceiveFrameString(); //camera name
                framePayload = frameSubscriber.ReceiveFrameBytes();  //json with data describe

                MsgPack msgpackFrameDecode = new MsgPack();
                msgpackFrameDecode.DecodeFromBytes(framePayload);

                //new size of image 
                //if (Convert.ToInt32(msgpackFrameDecode.ForcePathObject("height").AsInteger) != frameHeight ||
                //    Convert.ToInt32(msgpackFrameDecode.ForcePathObject("width").AsInteger) != frameWidth)
                //{
                //    //write height and width parameters
                //    frameHeight = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("height").AsInteger);
                //    frameWidth = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("width").AsInteger);

                //    //notify about event occurence
                //    var imageScaleArgs = new ImageScaleChangedEventArgs();
                //    imageScaleArgs.FrameSize = new Size(frameWidth, frameHeight);
                //    OnImageScaleChanged(imageScaleArgs);
                //}

                frameWidth = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("width").AsInteger);
                frameHeight = Convert.ToInt32(msgpackFrameDecode.ForcePathObject("height").AsInteger);

                var imageArgs = new PupilReceivedDataEventArgs();
                imageArgs.GazePoints = new List<GazePoint>();

                //receive video frame in bgr
                frameData = frameSubscriber.ReceiveFrameBytes();

                bool gazeReceived = true;

                while (gazeReceived)
                {
                    //receive gaze information
                    gazeSubscriber.TryReceiveFrameString(out gazeMsg);
                    gazeReceived = gazeSubscriber.TryReceiveFrameBytes(out gazeData);

                    if (gazeData != null)
                    {
                        var msgpackGazeDecode = new MsgPack();
                        msgpackGazeDecode.DecodeFromBytes(gazeData);

                        //new event for inform about video data
                        if (msgpackGazeDecode.ForcePathObject("norm_pos").AsArray.Length >= 2 &&
                            msgpackGazeDecode.ForcePathObject("confidence").AsFloat > 0.5)
                        {
                            imageArgs.GazePoints.Add(new GazePoint(
                                new Point(
                                    msgpackGazeDecode.ForcePathObject("norm_pos").AsArray[0].AsFloat,
                                    (1.0 - msgpackGazeDecode.ForcePathObject("norm_pos").AsArray[1].AsFloat)),
                                msgpackGazeDecode.ForcePathObject("confidence").AsFloat));
                        }
                    }
                }

                imageArgs.RawImageData = frameData;
                imageArgs.ImageTimestamp = msgpackFrameDecode.ForcePathObject("timestamp").AsFloat;
                imageArgs.ImageSize = new Size(frameWidth, frameHeight);

                OnPupilReceivedData(imageArgs);
            }
        }

        public void Disconnect()
        {
            isConnected = false;

            frameThread?.Abort();

            requestClient.Dispose();
            frameSubscriber.Dispose();
            gazeSubscriber.Dispose();
        }

        protected virtual void OnPupilReceivedData(PupilReceivedDataEventArgs args)
        {
            PupilDataReceivedEvent?.Invoke(this, args);
        }

        //protected virtual void OnImageScaleChanged(ImageScaleChangedEventArgs args)
        //{
        //    ImageScaleChangedEvent?.Invoke(this, args);
        //}

        public class PupilReceivedDataEventArgs : EventArgs
        {
            public byte[] RawImageData { get; set; }
            public double ImageTimestamp { get; set; }
            public Size ImageSize { get; set; }
            public List<GazePoint> GazePoints { get; set; }
        }

        //public class ImageScaleChangedEventArgs : EventArgs
        //{
        //    public Size FrameSize { get; set; }
        //}
    }
}

