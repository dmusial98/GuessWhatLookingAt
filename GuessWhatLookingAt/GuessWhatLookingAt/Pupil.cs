using NetMQ;
using NetMQ.Sockets;
using SimpleMsgPack;
using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhatLookingAt
{
    public class Pupil
    {
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

        public event EventHandler<PupilReceivedDataEventArgs> PupilDataReceivedEvent;

        PupilImage pupilImage;

        public void ConnectAndReceiveFromPupil()
        {
            isConnected = true;
            while (isConnected)
            {
                using (var requestClient = new RequestSocket())
                {
                    using (var frameSubscriber = new SubscriberSocket())
                    {
                        using (var gazeSubscriber = new SubscriberSocket())
                        {
                            requestClient.Connect("tcp://127.0.0.1:50020");

                            //getting subscriber and publisher port
                            requestClient.SendFrame("SUB_PORT");
                            subPort = requestClient.ReceiveFrameString();
                            requestClient.SendFrame("PUB_PORT");
                            pubPort = requestClient.ReceiveFrameString();

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

                            //receive name and parameters of video 
                            frameTopic = frameSubscriber.ReceiveFrameString(); //camera name
                            framePayload = frameSubscriber.ReceiveFrameBytes();  //json with data describe

                            MsgPack msgpackFrameDecode = new MsgPack();
                            msgpackFrameDecode.DecodeFromBytes(framePayload);

                            //read video parameters
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

                            pupilImage = new PupilImage();
                            pupilImage.SetMat(pointer, frameWidth, frameHeight);
                            pinnedArray.Free();

                            pupilImage.DrawCircle(msgpackGazeDecode./*ForcePathObject("base_data").AsArray[0].*/ForcePathObject("norm_pos").AsArray[0].AsFloat, msgpackGazeDecode./*ForcePathObject("base_data").AsArray[0].*/ForcePathObject("norm_pos").AsArray[1].AsFloat);
                            pupilImage.PutConfidenceText(msgpackGazeDecode./*ForcePathObject("base_data").AsArray[0].*/ForcePathObject("confidence").AsFloat);
                            args.image = pupilImage.GetBitmapSourceFromMat();

                            OnPupilReceivedData(args);
                        }
                    }
                }
            }
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

