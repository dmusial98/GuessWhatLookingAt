using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using System.Threading;
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

        Point? _PupilGazePoint;
        public bool IsPupilConnected { get; private set; } = false;

        #endregion//Pupil variables

        #region Eye Tribe variables

        public EyeTribe eyeTribe = new EyeTribe();
        Point? _EyeTribeGazePoint;
        public bool IsEyeTribeConnected { get; private set; } = false;

        System.Threading.Timer _eyeTribeTimer;

        const int _eyeTribeTimerSeconds = 5;
        public int EyeTribeTimerRemainingTime { get; private set; } = _eyeTribeTimerSeconds;

        public event EventHandler<EyeTribeGazePositionEventArgs> EyeTribeGazePointReached;

        public class EyeTribeGazePositionEventArgs : EventArgs
        {
            public EyeTribeGazePositionEventArgs(double gazeX, double gazeY) => gazePoint = new Point(gazeX, gazeY);

            public Point gazePoint { get; set; }
        }

        public event EventHandler<EyeTribeTimerEventArgs> EyeTribeTimerEvent;

        public class EyeTribeTimerEventArgs : EventArgs
        {
            public EyeTribeTimerEventArgs(int time, bool isLastAttempt)
            {
                Time = time;
                IsLastAttempt = isLastAttempt;
            }

            public int Time { get; set; }

            public bool IsLastAttempt { get; set; }
        }

        #endregion//Eye Tribe variables

        #region Photo variables

        System.Threading.Timer photoTimer;
        const int _timerSeconds = 3;
        public int PhotoRemainingTime { get; private set; } = _timerSeconds;
        public bool hasPhoto { get; private set; } = false;

        public event EventHandler<PhotoTimeChangedEventArgs> PhotoTimeChangedEvent;

        public class PhotoTimeChangedEventArgs : EventArgs
        {
            public PhotoTimeChangedEventArgs(int time, bool isLastRound)
            {
                Time = time;
                IsLastRound = isLastRound;
            }

            public int Time { get; set; }
            public bool IsLastRound { get; set; }
        }
        #endregion //Photo variables

        #region Logic variables

        const int _maxAttemptAmount = 3;
        int _remainingNumberOfAttempts = _maxAttemptAmount;

        public int AttemptNumber { get; set; } = 1;

        double _minAttemptsDistance;

        public int TotalPoints { get; private set; }

        FreezeGamePunctation _punctation = new FreezeGamePunctation();

        const int _maximalNumberOfRounds = 3;

        int _gameRoundIndex = _maximalNumberOfRounds;

        public int NumberOfGameRound { get; private set; } = 1;

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

        public void TakePhoto()
        {
            if (pupil.isConnected)
            {
                photoTimer = new System.Threading.Timer(
                    OnTakePhotoTimerEvent,
                    this,
                    1000,
                    1000);

                OnPhotoTimeEvent();
            }
        }

        public void SavePhotoImage()
        {
            image.SaveImage("");
        }

        private void OnTakePhotoTimerEvent(object state)
        {
            if (PhotoRemainingTime != 0)
            {
                PhotoRemainingTime--;
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

                photoTimer.Change(
                    Timeout.Infinite,
                    Timeout.Infinite); //turn off photoTimer
            }
        }

        private void OnPhotoTimeEvent()
        {
            PhotoTimeChangedEventArgs args = new PhotoTimeChangedEventArgs(
                time: PhotoRemainingTime,
                isLastRound: _gameRoundIndex == 0 ? true : false);
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
            }
        }
        public void DisconnectPupil()
        {
            if (pupil.isConnected)
            {
                pupil.Disconnect();
                IsPupilConnected = false;
            }
        }

        void OnPupilDataReached(object sender, Pupil.PupilReceivedDataEventArgs pupilArgs)
        {
            if (pupilArgs.rawImageData != null)
            {
                GCHandle pinnedarray = GCHandle.Alloc(pupilArgs.rawImageData, GCHandleType.Pinned);
                IntPtr pointer = pinnedarray.AddrOfPinnedObject();

                image.SetMat(pointer, Convert.ToInt32(pupilArgs.imageSize.Width), Convert.ToInt32(pupilArgs.imageSize.Height));
                pinnedarray.Free();
            }
            if (pupilArgs.gazePoints.Count != 0)
            {
                for(int i =0; i < pupilArgs.gazePoints.Count; i++)
                {
                    _PupilGazePoint = pupilArgs.gazePoints[i];
                    image.DrawCircleForPupil(_PupilGazePoint.Value, pupilArgs.gazeConfidence[i]);
                }
            }
            if (_EyeTribeGazePoint != null)
                image.DrawCircleForEyeTribe(_EyeTribeGazePoint.GetValueOrDefault());

            if (image.OutMat != null)
            {
                var imageSourceArgs = new BitmapSourceEventArgs(image.GetBitmapSourceFromMat(pupilArgs.imageXScale, pupilArgs.imageYScale));

                OnImageSourceReached(imageSourceArgs);
            }
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

            var eyeTribePoint = new Point(gazeX, gazeY);

            if (ImageRect.Contains(eyeTribePoint))
            {
                eyeTribePoint.Offset(-_BegginingImagePoint.X, -_BegginingImagePoint.Y);
                _EyeTribeGazePoint = new Point(eyeTribePoint.X / _ImageXScale, eyeTribePoint.Y / _ImageYScale);
            }
            else
            {
                _EyeTribeGazePoint = null;
            }

            var args = new EyeTribeGazePositionEventArgs(gazeX, gazeY);
            OnEyeTribeGazePositionReached(args);

            //if (hasPhoto && !IsPupilConnected)
            //{
            //    image.DrawCircleForPupil(
            //        gazePoint: _PupilGazePoint.Value,
            //        confidence: 1.0,
            //        cleanImage: true);

            //    image.DrawCircleForEyeTribe(_EyeTribeGazePoint.Value);

            //    var imageSourceArgs = new BitmapSourceEventArgs(image.GetBitmapSourceFromMat(_ImageXScale, _ImageYScale));
            //    OnImageSourceReached(imageSourceArgs);
            //}
        }

        public void OnEyeTribeGazePositionReached(EyeTribeGazePositionEventArgs args)
        {
            var handler = EyeTribeGazePointReached;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public void StartEyeTribeTimer()
        {
            if (eyeTribe.isRunning /*&& _eyeTribeTimer == null*/)
            {
                _eyeTribeTimer = new System.Threading.Timer(
                    callback: OnEyeTribeTimerEvent,
                    state: this,
                    dueTime: 1000,
                    period: 1000);

                OnEyeTribeTimeEvent();
            }
        }

        private void OnEyeTribeTimerEvent(object state)
        {
            if (EyeTribeTimerRemainingTime != 0)
            {
                EyeTribeTimerRemainingTime--;

                OnEyeTribeTimeEvent();
            }
            else //Time == 0
            {
                OnEyeTribeTimeEvent();
                EyeTribeTimerRemainingTime = _eyeTribeTimerSeconds;
                _eyeTribeTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void OnEyeTribeTimeEvent()
        {
            EyeTribeTimerEventArgs args = new EyeTribeTimerEventArgs(
                time: EyeTribeTimerRemainingTime,
                isLastAttempt: _remainingNumberOfAttempts == 0 ? true : false);
            EventHandler<EyeTribeTimerEventArgs> handler = EyeTribeTimerEvent;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #endregion//Eye Tribe methods

        #region Logic methods

        public void StartRound()
        {
            _gameRoundIndex--;
            TakePhoto();

            if (_gameRoundIndex == -1)
            {
                _gameRoundIndex = _maximalNumberOfRounds - 1;
                NumberOfGameRound = 1;
                TotalPoints = 0;
            }
            else
                ActualiseRoundNumber();
        }

        public bool MouseAttemptStarted(Point mousePosition, out double? distance, out int? points)
        {
            distance = CountPointsDifference(mousePosition);
            _remainingNumberOfAttempts--;

            return RealiseAttemptLogic(ref distance, out points);
        }

        public bool EyeTribeAttemptStarted(out double? distance, out int? points)
        {
            distance = CountPointsDifference(_EyeTribeGazePoint == null ? new Point(0.0, 0.0) : _EyeTribeGazePoint.Value);
            _remainingNumberOfAttempts--;

            return RealiseAttemptLogic(ref distance, out points);
        }

        private double CountPointsDifference(Point p1)
        {
            var gazeResizedPoint = new Point(pupil.gazePoint.X * _ImageXScale, pupil.gazePoint.Y * _ImageYScale);
            gazeResizedPoint.Offset(_BegginingImagePoint.X, _BegginingImagePoint.Y);
            var distance = Point.Subtract(gazeResizedPoint, p1).Length;
            return Math.Round(distance, 2);
        }

        private void CountPointsAfterAttempts()
        {
            for (int i = _punctation.PunctationList.Count; i > 0; i--)
            {
                if (_minAttemptsDistance <= _punctation.PunctationList[i - 1])
                {
                    TotalPoints += i;
                    return;
                }
            }
        }

        private bool RealiseAttemptLogic(ref double? distance, out int? points)
        {
            if (distance.Value < _minAttemptsDistance)
                _minAttemptsDistance = distance.Value;

            if (_remainingNumberOfAttempts == 0) //last attempt
            {
                distance = _minAttemptsDistance;
                _remainingNumberOfAttempts = _maxAttemptAmount;
                CountPointsAfterAttempts();
                points = TotalPoints;
                ResetMinAttemptDistance();
                AttemptNumber = 1;
                return true;
            }
            else
            {
                ActualiseAttemptNumber();
                points = null;
                return false;
            }
        }

        private void ResetMinAttemptDistance() => _minAttemptsDistance = Point.Subtract(ImageRect.TopLeft, ImageRect.BottomRight).Length;

        private void ActualiseAttemptNumber() => AttemptNumber = _maxAttemptAmount - _remainingNumberOfAttempts;

        private void ActualiseRoundNumber() => NumberOfGameRound = _maximalNumberOfRounds - _gameRoundIndex;

        #endregion//logic methods

        #endregion//Methods

    }
}
