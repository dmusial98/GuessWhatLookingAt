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
        #region Variables

        #region Image variables

        EmguCVImage image = new EmguCVImage();
        public Size ImageSize { get; private set; }

        double _ImageXScale = 1.0;
        double _ImageYScale = 1.0;

        Point _BegginingImagePoint = new Point(0.0, 0.0);

        public Rect ImageRect { get; private set; }

        public event EventHandler<BitmapSourceEventArgs> BitmapSourceReached;

        public class BitmapSourceEventArgs : EventArgs
        {
            public BitmapSourceEventArgs(ImageSource im) => image = im;

            public ImageSource image { get; set; }
        }
        #endregion//Image variables

        #region Pupil variables

        public Pupil pupil = new Pupil();
        Thread pupilThread;
        public bool IsPupilConnected { get; private set; } = false;

        #endregion//Pupil variables

        #region Eye Tribe variables

        public EyeTribe eyeTribe = new EyeTribe();
        Point? EyeTribeGazePoint;
        public bool IsEyeTribeConnected { get; private set; } = false;

        public event EventHandler<EyeTribeGazePositionEventArgs> EyeTribeGazePointReached;

        public class EyeTribeGazePositionEventArgs : EventArgs
        {
            public EyeTribeGazePositionEventArgs(double gazeX, double gazeY) => gazePoint = new Point(gazeX, gazeY);

            public Point gazePoint { get; set; }
        }



        #endregion//Eye Tribe variables

        #region Photo variables

        System.Timers.Timer photoTimer;
        const int _timerSeconds = 3;
        public int PhotoRemainingTime { get; private set; } = _timerSeconds;
        public bool hasPhoto { get; private set; } = false;

        public event EventHandler<PhotoTimeChangedEventArgs> PhotoTimeChangedEvent;

        public class PhotoTimeChangedEventArgs : EventArgs
        {
            public PhotoTimeChangedEventArgs(int time)
            {
                Time = time;
            }

            public int Time { get; set; }
        }
        #endregion //Photo variables

        #region Logic variables

        const int _maxAttemptAmount = 3;
        public int _remainningNumberOfAttempts { get; private set; } = _maxAttemptAmount;

        double _minAttemptsDistance;

        #endregion //Logic variables

        #endregion //Variables

        #region Constructors
        public FreezeGameModel(Size imageSize, Point begginingImagePoint)
        {
            //image variables setting up
            ImageSize = imageSize;
            pupil.ImageScaleChangedEvent += OnImageScaleChanged;
            pupil.ImageSizeToDisplay = ImageSize;
            _BegginingImagePoint = begginingImagePoint;
            ImageRect = new Rect(_BegginingImagePoint, ImageSize);

            //logic variables setting up
            ResetMinAttemptDistance();

            //events service setting up
            eyeTribe.OnData += OnEyeTribeDataReached;
            pupil.PupilDataReceivedEvent += OnPupilDataReached;
        }
        #endregion//Constructors

        #region Methods

        #region Photo methods

        public void SavePhotoImage()
        {
            image.SaveImage("");
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

        #endregion//Photo meethods

        #region Image methods
        private void OnImageScaleChanged(object sender, Pupil.ImageScaleChangedEventArgs args)
        {
            _ImageXScale = args.XScaleImage;
            _ImageYScale = args.YScaleImage;
        }

        private void OnImageSourceReached(BitmapSourceEventArgs args)
        {
            var handler = BitmapSourceReached;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #endregion//Image methods

        #region Pupil methods
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

        #endregion//Pupil methods

        #region Eye Tribe methods
        public void ConnectWithEyeTribe()
        {
            if (!eyeTribe.isRunning)
            {
                eyeTribe.Connect("localhost", 6555);
                IsEyeTribeConnected = true;
            }
        }

        public void DisconnectEyeTribe()
        {
            if (eyeTribe.isRunning)
            {
                eyeTribe.Disconnect();
                IsEyeTribeConnected = false;
            }
        }

        void OnEyeTribeDataReached(object sender, EyeTribe.EyeTribeReceivedDataEventArgs e)
        {
            JObject values = JObject.Parse(e.data.values);
            JObject gaze = JObject.Parse(values.SelectToken("frame").SelectToken("avg").ToString());
            double gazeX = (double)gaze.Property("x").Value;
            double gazeY = (double)gaze.Property("y").Value;

            //TODO: Draw Circle dla Eye Tribe'a
            var eyeTribePoint = new Point(gazeX, gazeY);

            if (ImageRect.Contains(eyeTribePoint))
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

        public void OnEyeTribeGazePositionReached(EyeTribeGazePositionEventArgs args)
        {
            var handler = EyeTribeGazePointReached;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #endregion//eye Tribe methods

        #region Logic methods

        public bool MouseAttemptStarted(Point mousePosition, out double? distance)
        {
            distance = CountPointsDifference(mousePosition);
            _remainningNumberOfAttempts--;

            if (distance < _minAttemptsDistance)
                _minAttemptsDistance = distance.Value;

            if (_remainningNumberOfAttempts == 0) //last attempt
            {
                distance = _minAttemptsDistance;
                ResetMinAttemptDistance();
                _remainningNumberOfAttempts = _maxAttemptAmount;
                return true;
            }
            else
            {
                return false;
            }
        }

        public double CountPointsDifference(Point p1)
        {
            var gazeResizedPoint = new Point(pupil.gazePoint.X * _ImageXScale, pupil.gazePoint.Y * _ImageYScale);
            gazeResizedPoint.Offset(_BegginingImagePoint.X, _BegginingImagePoint.Y);
            var distance = Point.Subtract(gazeResizedPoint, p1).Length;
            return Math.Round(distance, 2);
        }

        private void ResetMinAttemptDistance() => _minAttemptsDistance = Point.Subtract(ImageRect.TopLeft, ImageRect.BottomRight).Length;


        #endregion//logic methods

        #endregion//Methods

    }
}
