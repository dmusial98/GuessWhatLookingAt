using Newtonsoft.Json.Linq;
using System;
using System.Linq;
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

        readonly EmguCVImage image = new EmguCVImage();
        public WindowViewParameters _WindowViewParameters { get; private set; }

        public event EventHandler<BitmapSourceEventArgs> BitmapSourceReached;

        public class BitmapSourceEventArgs : EventArgs
        {
            public BitmapSourceEventArgs(ImageSource im) => Image = im;

            public ImageSource Image { get; set; }
        }
        #endregion//Image variables

        #region Pupil variables

        public Pupil pupil = new Pupil();

        GazePoint _PupilGazePoint;
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
            public EyeTribeGazePositionEventArgs(double gazeX, double gazeY) => GazePoint = new Point(gazeX, gazeY);

            public Point GazePoint { get; set; }
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
        public bool HasPhoto { get; private set; } = false;

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
        public FreezeGameModel(WindowViewParameters windowViewParameters)
        {
            _WindowViewParameters = windowViewParameters;

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
                HasPhoto = true;
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
                isLastRound: _gameRoundIndex == 0);
            PhotoTimeChangedEvent?.Invoke(this, args);
        }

        #endregion//Photo meethods

        #region Image methods

        private void OnImageSourceReached(BitmapSourceEventArgs args)
        {
            BitmapSourceReached?.Invoke(this, args);
        }

        #endregion//Image methods

        #region Pupil methods
        public void ConnectWithPupil()
        {
            if (!pupil.isConnected)
            {
                HasPhoto = false;
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
            if (pupilArgs.RawImageData != null)
            {
                GCHandle pinnedarray = GCHandle.Alloc(pupilArgs.RawImageData, GCHandleType.Pinned);
                IntPtr pointer = pinnedarray.AddrOfPinnedObject();

                image.SetMat(pointer, Convert.ToInt32(pupilArgs.ImageSize.Width), Convert.ToInt32(pupilArgs.ImageSize.Height));
                pinnedarray.Free();
            }
            if (pupilArgs.GazePoints.Count != 0)
            {
                _PupilGazePoint = pupilArgs.GazePoints.Max();

                foreach (GazePoint gazePoint in pupilArgs.GazePoints)
                    image.DrawCircleForPupil(gazePoint);
            }
            if (_EyeTribeGazePoint != null)
                image.DrawCircleForEyeTribe(_EyeTribeGazePoint.GetValueOrDefault());

            if (image.OutMat != null)
            {
                var imageSourceArgs = new BitmapSourceEventArgs(image.GetBitmapSourceFromMat(/*pupilArgs.ImageXScale, pupilArgs.ImageYScale*/));
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

            if (_WindowViewParameters.WindowRect.Contains(eyeTribePoint))
                _EyeTribeGazePoint = NormalizePointToImage(
                    point: eyeTribePoint,
                    relativeToWindow: false);
            else
                _EyeTribeGazePoint = null;
            
            var args = new EyeTribeGazePositionEventArgs(gazeX, gazeY);
            OnEyeTribeGazePositionReached(args);

            if (HasPhoto && !IsPupilConnected)
            {
                image.DrawCircleForPupil(
                    point: _PupilGazePoint,
                    cleanImage: true);

                image.DrawCircleForEyeTribe(_EyeTribeGazePoint.Value);

                var imageSourceArgs = new BitmapSourceEventArgs(image.GetBitmapSourceFromMat());
                OnImageSourceReached(imageSourceArgs);
            }
        }

        public void OnEyeTribeGazePositionReached(EyeTribeGazePositionEventArgs args)
        {
            EyeTribeGazePointReached?.Invoke(this, args);
        }

        public void StartEyeTribeTimer()
        {
            if (eyeTribe.isRunning)
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
                isLastAttempt: _remainingNumberOfAttempts == 0);

            EyeTribeTimerEvent?.Invoke(this, args);
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

        private double CountPointsDifference(Point point, bool coordinatesRelativeToWindow = true)
        {
            var normalizedMousePositionOnImage = NormalizePointToImage(point, coordinatesRelativeToWindow);
            return Point.Subtract(_PupilGazePoint.point, normalizedMousePositionOnImage).Length;
        }

        private Point NormalizePointToImage(Point point, bool relativeToWindow = true)
        {
            if (!relativeToWindow)
                point.Offset(-_WindowViewParameters.WindowRect.X, -_WindowViewParameters.WindowRect.Y);

            return new Point(
                point.X / (_WindowViewParameters.WindowRect.Width * 0.9),
                point.Y / (_WindowViewParameters.WindowRect.Height * 0.9));
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

        private void ResetMinAttemptDistance() => _minAttemptsDistance = Point.Subtract(_WindowViewParameters.WindowRect.TopLeft, _WindowViewParameters.WindowRect.BottomRight).Length;

        private void ActualiseAttemptNumber() => AttemptNumber = _maxAttemptAmount - _remainingNumberOfAttempts;

        private void ActualiseRoundNumber() => NumberOfGameRound = _maximalNumberOfRounds - _gameRoundIndex;

        #endregion//logic methods

        #endregion//Methods

    }
}
