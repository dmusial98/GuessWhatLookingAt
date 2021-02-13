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

        public event EventHandler<PupilReceivedDataEventArgs> PupilDataReceivedEvent;

        public void Connect(object address)
        {
            var _address = (string)address;

            requestClient = new RequestSocket();

            requestClient.Connect(_address);

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
            var msgpackNotify = new MsgPack();
            msgpackNotify.ForcePathObject("subject").AsString = "frame_publishing.set_format";
            msgpackNotify.ForcePathObject("format").AsString = "bgr";
            var byteArrayNotify = msgpackNotify.Encode2Bytes();

            //sending information
            requestClient.SendMoreFrame("topic.frame_publishing.set_format")
                .SendFrame(byteArrayNotify);

            requestClient.ReceiveFrameString(); //confirm receive data

            isConnected = true;

            frameThread = new System.Threading.Thread(ReceiveFrame);
            //gazeThread = new System.Threading.Thread(ReceiveGaze);

            frameThread.Start();
            //gazeThread.Start();
        }

        public void ReceiveFrame()
        {
            while (isConnected)
            {
                frameTopic = frameSubscriber.ReceiveFrameString(); //camera name
                framePayload = frameSubscriber.ReceiveFrameBytes();  //json with data describe

                MsgPack msgpackFrame = new MsgPack();
                msgpackFrame.DecodeFromBytes(framePayload);

                frameWidth = Convert.ToInt32(msgpackFrame.ForcePathObject("width").AsInteger);
                frameHeight = Convert.ToInt32(msgpackFrame.ForcePathObject("height").AsInteger);

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
                        var msgpackGaze = new MsgPack();
                        msgpackGaze.DecodeFromBytes(gazeData);

                        //new event for inform about video data
                        if (msgpackGaze.ForcePathObject("norm_pos").AsArray.Length >= 2 &&
                            msgpackGaze.ForcePathObject("confidence").AsFloat > 0.5)
                        {
                            imageArgs.GazePoints.Add(new GazePoint(
                                new Point(
                                    msgpackGaze.ForcePathObject("norm_pos").AsArray[0].AsFloat,
                                    (1.0 - msgpackGaze.ForcePathObject("norm_pos").AsArray[1].AsFloat)),
                                msgpackGaze.ForcePathObject("confidence").AsFloat));
                        }
                    }
                }

                imageArgs.RawImageData = frameData;
                imageArgs.ImageTimestamp = msgpackFrame.ForcePathObject("timestamp").AsFloat;
                imageArgs.ImageSize = new Size(frameWidth, frameHeight);

                OnPupilReceivedData(imageArgs);
            }
        }

        public void Disconnect()
        {
            isConnected = false;
            frameThread?.Abort();

            //clean after disconnecting
            requestClient.Dispose();
            frameSubscriber.Dispose();
            gazeSubscriber.Dispose();
        }

        protected virtual void OnPupilReceivedData(PupilReceivedDataEventArgs args)
        {
            PupilDataReceivedEvent?.Invoke(this, args);
        }

        public class PupilReceivedDataEventArgs : EventArgs
        {
            public byte[] RawImageData { get; set; }
            public double ImageTimestamp { get; set; }
            public Size ImageSize { get; set; }
            public List<GazePoint> GazePoints { get; set; }
        }
    }
}

