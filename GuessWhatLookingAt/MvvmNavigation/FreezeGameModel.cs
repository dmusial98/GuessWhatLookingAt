using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Timers;
using System.Windows;

namespace GuessWhatLookingAt
{
    class FreezeGameModel
    {
        #region Image size fields
        double _ImageWidth;
        double _ImageHeight;

        double ImageXScale = 1.0;
        double ImageYScale = 1.0;
        #endregion

        Thread pupilThread;
        public Pupil pupil { get; private set; } = new Pupil();
        public EyeTribe eyeTribe = new EyeTribe();

        Point _BegginingImagePoint = new Point(0.0, 0.0);

        public bool IsPupilConnected { get; private set; } = false;
        public bool IsEyeTribeConnected { get; private set; } = false;

        #region PhotoFields
        System.Timers.Timer photoTimer;
        const int _timerSeconds = 3;
        public int PhotoRemainingTime { get; private set; } = _timerSeconds;
        public bool hasPhoto { get; private set; } = false;

        public event EventHandler<PhotoTimeChangedEventArgs> PhotoTimeChangedEvent;
        #endregion //PhotoFields

        public event EventHandler<EyeTribeGazePositionEventArgs> EyeTribeGazePointReached;

        #region Constructors
        public FreezeGameModel(double pupilImageWidth, double pupilImageHeight, Point begginingImagePoint)
        {
            _ImageWidth = pupilImageWidth;
            _ImageHeight = pupilImageHeight;
            pupil.ImageScaleChangedEvent += OnImageScaleChanged;
            pupil.ImageWidthToDisplay = _ImageWidth;
            pupil.ImageHeightToDisplay = _ImageHeight;
            _BegginingImagePoint = begginingImagePoint;

            eyeTribe.OnData += OnEyeTribeDataReached;
        }
        #endregion

        public void ConnectWithPupil()
        {
            if (!pupil.isConnected)
            {
                hasPhoto = false;
                pupil.Connect();

                IsPupilConnected = true;
                pupilThread = new Thread(pupil.ReceiveFrame);
                pupilThread.Start();
            }
        }

        public void DisconnectPupil()
        {
            if (pupil.isConnected)
            {
                pupil.Disconnect();
                pupilThread?.Abort();
                IsPupilConnected = false;
            }
        }

        public void ConnectWithEyeTribe()
        {
            if (!eyeTribe.isRunning)
            {
                eyeTribe.Connect("localhost", 6555);
                IsEyeTribeConnected = true;

            }
        }

        public void TakePhoto()
        {
            if (pupil.isConnected)
            {
                photoTimer = new System.Timers.Timer(1000);
                photoTimer.Elapsed += OnTakePhotoTimerEvent;
                photoTimer.AutoReset = false;

                OnPhotoTimeEvent();

                photoTimer.Enabled = true;
            }
        }

        public double CountPointsDifference(Point p1)
        {
            var gazeResizedPoint = new Point(pupil.gazePoint.X * ImageXScale, pupil.gazePoint.Y * ImageYScale);
            gazeResizedPoint.Offset(_BegginingImagePoint.X, _BegginingImagePoint.Y);
            return Point.Subtract(gazeResizedPoint, p1).Length;
        }

        private void OnTakePhotoTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (PhotoRemainingTime != 0)
            {
                PhotoRemainingTime--;
                photoTimer.Enabled = true;

                OnPhotoTimeEvent();
            }
            else
            {
                pupil.Disconnect();
                pupilThread?.Abort();
                hasPhoto = true;
                IsPupilConnected = false;


                OnPhotoTimeEvent();

                PhotoRemainingTime = _timerSeconds;
            }
        }

        private void OnPhotoTimeEvent()
        {
            PhotoTimeChangedEventArgs args = new PhotoTimeChangedEventArgs(PhotoRemainingTime);
            EventHandler<PhotoTimeChangedEventArgs> handler = PhotoTimeChangedEvent;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public class PhotoTimeChangedEventArgs : EventArgs
        {
            public PhotoTimeChangedEventArgs(int time)
            {
                Time = time;
            }

            public int Time { get; set; }
        }

        private void OnImageScaleChanged(object sender, Pupil.ImageScaleChangedEventArgs args)
        {
            ImageXScale = args.XScaleImage;
            ImageYScale = args.YScaleImage;
        }

        void OnEyeTribeDataReached(object sender, EyeTribe.EyeTribeReceivedDataEventArgs e)
        {
            JObject values = JObject.Parse(e.data.values);
            JObject gaze = JObject.Parse(values.SelectToken("frame").SelectToken("avg").ToString());
            double gazeX = (double)gaze.Property("x").Value;
            double gazeY = (double)gaze.Property("y").Value;

            var args = new EyeTribeGazePositionEventArgs(gazeX, gazeY);
            

            OnEyeTribeGazePositionReached(args);
        }

        public class EyeTribeGazePositionEventArgs : EventArgs
        {
            public EyeTribeGazePositionEventArgs(double gazeX, double gazeY) => gazePoint = new Point(gazeX, gazeY);

            public Point gazePoint { get; set; }
        }

        public void OnEyeTribeGazePositionReached(EyeTribeGazePositionEventArgs args)
        {
            var handler = EyeTribeGazePointReached;
            if(handler != null)
            {
                handler(this, args);
            }
        }

    }
}
