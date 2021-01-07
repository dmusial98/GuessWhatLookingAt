using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace GuessWhatLookingAt
{
    class FreezeGameModel
    {
        #region Image fields
        public Size ImageSize { get; private set; }

        double _ImageXScale = 1.0;
        double _ImageYScale = 1.0;

        Point _BegginingImagePoint = new Point(0.0, 0.0);
        #endregion

        Thread pupilThread;
        public Pupil pupil = new Pupil();
        public EyeTribe eyeTribe = new EyeTribe();
        EmguCVImage image = new EmguCVImage();
        Point? EyeTribeGazePoint;

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
        public event EventHandler<BitmapSourceEventArgs> BitmapSourceReached;

        #region Constructors
        public FreezeGameModel(Size imageSize, Point begginingImagePoint)
        {
            ImageSize = imageSize;
            pupil.ImageScaleChangedEvent += OnImageScaleChanged;
            pupil.ImageSizeToDisplay = ImageSize;
            _BegginingImagePoint = begginingImagePoint;

            eyeTribe.OnData += OnEyeTribeDataReached;
            pupil.PupilDataReceivedEvent += OnPupilDataReached;
        }
        #endregion


        #region Pupil
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
        #endregion

        #region Eye Tribe
        public void ConnectWithEyeTribe()
        {
            if (!eyeTribe.isRunning)
            {
                eyeTribe.Connect("localhost", 6555);
                IsEyeTribeConnected = true;
            }
        }
        #endregion

        public double CountPointsDifference(Point p1)
        {
            var gazeResizedPoint = new Point(pupil.gazePoint.X * _ImageXScale, pupil.gazePoint.Y * _ImageYScale);
            gazeResizedPoint.Offset(_BegginingImagePoint.X, _BegginingImagePoint.Y);
            return Point.Subtract(gazeResizedPoint, p1).Length;
        }

        void OnPupilDataReached(object sender, Pupil.PupilReceivedDataEventArgs pupilArgs)
        {
            GCHandle pinnedarray = GCHandle.Alloc(pupilArgs.rawImageData, GCHandleType.Pinned);
            IntPtr pointer = pinnedarray.AddrOfPinnedObject();

            image.SetMat(pointer, Convert.ToInt32(pupilArgs.imageSize.Width), Convert.ToInt32(pupilArgs.imageSize.Height));
            pinnedarray.Free();
            
            image.DrawCircleForPupil(pupilArgs.gazePoint);
            if (EyeTribeGazePoint != null)
                image.DrawCircleForEyeTribe(EyeTribeGazePoint.GetValueOrDefault());
            image.PutConfidenceText(pupilArgs.gazeConfidence);
            var imageSourceArgs = new BitmapSourceEventArgs(image.GetBitmapSourceFromMat(pupilArgs.imageXScale, pupilArgs.imageYScale));

            


            OnImageSourceReached(imageSourceArgs);
        }

        public class BitmapSourceEventArgs : EventArgs
        {
            public BitmapSourceEventArgs(ImageSource im) => image = im;

            public ImageSource image { get; set; }
        }

        private void OnImageSourceReached(BitmapSourceEventArgs args)
        {
            var handler = BitmapSourceReached;
            if(handler != null)
            {
                handler(this, args);
            }
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
            _ImageXScale = args.XScaleImage;
            _ImageYScale = args.YScaleImage;
        }

        void OnEyeTribeDataReached(object sender, EyeTribe.EyeTribeReceivedDataEventArgs e)
        {
            JObject values = JObject.Parse(e.data.values);
            JObject gaze = JObject.Parse(values.SelectToken("frame").SelectToken("avg").ToString());
            double gazeX = (double)gaze.Property("x").Value;
            double gazeY = (double)gaze.Property("y").Value;

            //TODO: Draw Circle dla Eye Tribe'a
            var eyeTribePoint = new Point(gazeX, gazeY);
            var imageRect = new Rect(_BegginingImagePoint, ImageSize);
            if (imageRect.Contains(eyeTribePoint))
            {
                eyeTribePoint.Offset(-_BegginingImagePoint.X, -_BegginingImagePoint.Y);
                EyeTribeGazePoint = new Point(eyeTribePoint.X / _ImageXScale, eyeTribePoint.Y / _ImageYScale);
            }
            else
            {
                EyeTribeGazePoint = null;
            }

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
