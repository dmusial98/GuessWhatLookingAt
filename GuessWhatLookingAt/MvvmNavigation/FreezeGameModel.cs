using System;
using System.Threading;
using System.Timers;
using System.Windows;

namespace GuessWhatLookingAt
{
    class FreezeGameModel
    {
        #region Image size fields
        static double _ImageWidth = 1632.0;
        static double _ImageHeight = 918.0;

        double ImageXScale = 1.0;
        double ImageYScale = 1.0;
        #endregion

        Thread pupilThread;
        public Pupil pupil { get; private set; } = new Pupil();
        Point BegginingImagePoint = new Point(60.0, 70.0);

        #region PhotoFields
        System.Timers.Timer photoTimer;
        const int _timerSeconds = 3;
        public int PhotoRemainingTime { get; private set; } = _timerSeconds;
        public bool hasPhoto { get; private set; } = false;

        public event EventHandler<PhotoTimeChangedEventArgs> PhotoTimeChangedEvent;
        #endregion //PhotoFields

        public FreezeGameModel()
        {
            pupil.ImageScaleChangedEvent += OnImageScaleChanged;
            pupil.ImageWidthToDisplay = _ImageWidth;
            pupil.ImageHeightToDisplay = _ImageHeight;
        }
       

        public void ConnectWithPupil()
        {
            if (!pupil.isConnected)
            {
                hasPhoto = false;
                pupil.Connect();

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
            gazeResizedPoint.Offset(BegginingImagePoint.X, BegginingImagePoint.Y);
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
    }
}
