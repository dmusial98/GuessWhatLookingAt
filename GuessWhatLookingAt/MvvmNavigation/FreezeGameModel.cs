using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        List<Point> _AttemptPoints = new List<Point>();

        #endregion//Image variables

        #region Pupil variables

        public Pupil pupil = new Pupil();

        GazePoint _PupilGazePoint;
        public bool IsPupilConnected { get; private set; } = false;

        System.Threading.Thread pupilConnectThread;
        #endregion//Pupil variables

        #region Eye Tribe variables

        public EyeTribe eyeTribe = new EyeTribe();

        System.Threading.Thread eyeTribeConnectThread;

        Point? _EyeTribeGazePoint;
        public bool IsEyeTribeConnected { get; private set; } = false;

        System.Threading.Timer _eyeTribeTimer;

        public int EyeTribeTimerRemainingTime { get; private set; }

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
        public int PhotoRemainingTime { get; private set; }
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

        FreezeGameSettings GameSettings;

        int _remainingNumberOfAttempts;

        bool _wasLastAttempt = false;

        public int AttemptNumber { get; set; } = 1;

        double _minAttemptsDistance = Double.MaxValue;

        public int TotalPoints { get; private set; }

        FreezeGamePunctation _punctation = new FreezeGamePunctation();

        int _gameRoundIndex;

        public int NumberOfGameRound { get; private set; } = 1;

        #endregion //Logic variables

        #endregion //Variables

        #region Constructors
        public FreezeGameModel(WindowViewParameters windowViewParameters, FreezeGameSettings gameSettings)
        {
            _WindowViewParameters = windowViewParameters;
            GameSettings = gameSettings;

            //logic variables setting up
            EyeTribeTimerRemainingTime = GameSettings.EyeTribeTime;
            PhotoRemainingTime = GameSettings.PhotoTime;
            _remainingNumberOfAttempts = GameSettings.AttemptsAmount;

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

                if (GameSettings.DisplayPupilGazePoint)
                {
                    image.DrawCircleForPupil(_PupilGazePoint, cleanImage: true);
                    OnImageSourceReached(new BitmapSourceEventArgs(image.GetBitmapSourceFromMat()));
                }

                OnPhotoTimeEvent();
                PhotoRemainingTime = GameSettings.PhotoTime;

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

        private void DrawImageWithAllAttempts()
        {
            _wasLastAttempt = true;

            image.DrawCircleForPupil(_PupilGazePoint, true);

            foreach (Point point in _AttemptPoints)
            {
                image.DrawCircleForAttemptPoint(point);
                image.DrawLineBetweenPoints(_PupilGazePoint.point, point);
            }

            var imageSourceArgs = new BitmapSourceEventArgs(image.GetBitmapSourceFromMat());
            OnImageSourceReached(imageSourceArgs);
        }

        #endregion//Image methods

        #region Pupil methods
        public void ConnectWithPupil()
        {
            if (!pupil.isConnected)
            {
                HasPhoto = false;
                pupilConnectThread = new Thread(() => pupil.Connect(GameSettings.PupilAdressString));
                pupilConnectThread.Start();
            }
        }
        public void DisconnectPupil()
        {
            if (pupil.isConnected)
            {
                pupil.Disconnect();
                IsPupilConnected = false;
            }
            else
                pupilConnectThread?.Abort();
        }

        void OnPupilDataReached(object sender, Pupil.PupilReceivedDataEventArgs pupilArgs)
        {
            IsPupilConnected = true;

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

                if (GameSettings.DisplayPupilGazePoint)
                    foreach (GazePoint gazePoint in pupilArgs.GazePoints)
                        image.DrawCircleForPupil(gazePoint);
            }
            if (GameSettings.DisplayEyeTribeGazePoint && _EyeTribeGazePoint != null)
                image.DrawCircleForEyeTribe(_EyeTribeGazePoint.GetValueOrDefault());

            if (image.OutMat != null)
            {
                var imageSourceArgs = new BitmapSourceEventArgs(image.GetBitmapSourceFromMat());
                OnImageSourceReached(imageSourceArgs);
            }
        }

        #endregion//Pupil methods

        #region Eye Tribe methods
        public void ConnectWithEyeTribe()
        {
            if (!eyeTribe.isRunning)
            {
                eyeTribeConnectThread = new Thread(() => eyeTribe.Connect(GameSettings.EyeTribePort));
                eyeTribeConnectThread.Start();
            }
        }

        public void DisconnectEyeTribe()
        {
            if (eyeTribe.isRunning)
            {
                eyeTribe.Disconnect();
                IsEyeTribeConnected = false;
            }
            else
                eyeTribeConnectThread?.Abort();
        }

        void OnEyeTribeDataReached(object sender, EyeTribe.EyeTribeReceivedDataEventArgs e)
        {
            IsEyeTribeConnected = true;

            JObject values = JObject.Parse(e.data.values);
            JObject gaze = JObject.Parse(values.SelectToken("frame").SelectToken("avg").ToString());
            double gazeX = (double)gaze.Property("x").Value;
            double gazeY = (double)gaze.Property("y").Value;

            var eyeTribePoint = new Point(gazeX, gazeY);

            if ((_WindowViewParameters.WindowState == WindowState.Maximized && _WindowViewParameters.WindowMaximizedRect.Contains(eyeTribePoint)) ||
              (_WindowViewParameters.WindowState == WindowState.Normal && _WindowViewParameters.WindowRect.Contains(eyeTribePoint)))
                _EyeTribeGazePoint = NormalizePointCoordinatesToImage(
                    point: eyeTribePoint,
                    saveToAttemptPoints: false,
                    relativeToWindow: false);
            else
                _EyeTribeGazePoint = null;

            var args = new EyeTribeGazePositionEventArgs(gazeX, gazeY);
            OnEyeTribeGazePositionReached(args);


            if (HasPhoto && !IsPupilConnected) //during "has photo" time
            {
                if (GameSettings.DisplayPupilGazePoint)
                    image.DrawCircleForPupil(
                        point: _PupilGazePoint,
                        cleanImage: true);
                else
                    image.CleanImage();

                if (GameSettings.DisplayEyeTribeGazePoint)
                {
                    image.DrawCircleForEyeTribe(_EyeTribeGazePoint.Value);
                }

                foreach (Point point in _AttemptPoints)
                    image.DrawCircleForAttemptPoint(point);

                if (_wasLastAttempt) //last attempt
                {
                    image.DrawCircleForPupil(_PupilGazePoint);
                    foreach (Point point in _AttemptPoints)
                        image.DrawLineBetweenPoints(point, _PupilGazePoint.point);
                }

                var imageSourceArgs = new BitmapSourceEventArgs(image.GetBitmapSourceFromMat());
                OnImageSourceReached(imageSourceArgs);
            }
            else if(!IsPupilConnected)
            {
                image.CleanImage();

                if (GameSettings.DisplayPupilGazePoint)
                    image.DrawCircleForPupil(_PupilGazePoint);

                if (GameSettings.DisplayEyeTribeGazePoint)
                    image.DrawCircleForEyeTribe(_EyeTribeGazePoint.Value);

                OnImageSourceReached(new BitmapSourceEventArgs(image.GetBitmapSourceFromMat()));
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
                EyeTribeTimerRemainingTime = GameSettings.EyeTribeTime;
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
                _gameRoundIndex = GameSettings.RoundsAmount - 1;
                NumberOfGameRound = 1;
                TotalPoints = 0;
            }
            else
                ActualiseRoundNumber();

            _wasLastAttempt = false;
            _AttemptPoints.Clear();
        }

        public bool MouseAttemptStarted(Point mousePosition, out double? distance, out int? points)
        {
            distance = CountPointsDifferenceMouse(mousePosition);
            _remainingNumberOfAttempts--;

            var normalizedMousePointCoordinates = NormalizePointCoordinatesToImage(
                mousePosition,
                false);

            image.DrawCircleForAttemptPoint(normalizedMousePointCoordinates);
            OnImageSourceReached(new BitmapSourceEventArgs(image.GetBitmapSourceFromMat()));

            return RealiseAttemptLogic(ref distance, out points);
        }

        public bool EyeTribeAttemptStarted(out double? distance, out int? points)
        {
            var eyeTribePoint = _EyeTribeGazePoint != null ? _EyeTribeGazePoint.Value : new Point(0, 0);

            distance = CountPointsDifferenceEyeTribe(eyeTribePoint);
            _remainingNumberOfAttempts--;

            _AttemptPoints.Add(eyeTribePoint);

            return RealiseAttemptLogic(ref distance, out points);
        }

        private double CountPointsDifferenceMouse(Point point, bool coordinatesRelativeToWindow = true)
        {
            var normalizedMousePositionOnImage = NormalizePointCoordinatesToImage(
                point: point,
                saveToAttemptPoints: true,
                relativeToWindow: coordinatesRelativeToWindow);

            return Point.Subtract(_PupilGazePoint.point, normalizedMousePositionOnImage).Length;
        }

        private double CountPointsDifferenceEyeTribe(Point point)
        {
            return Point.Subtract(_PupilGazePoint.point, point).Length;
        }

        private Point NormalizePointCoordinatesToImage(Point point, bool saveToAttemptPoints, bool relativeToWindow = true)
        {
            Rect rect;
            if (_WindowViewParameters.WindowState == WindowState.Maximized)
                rect = _WindowViewParameters.WindowMaximizedRect;
            else
                rect = _WindowViewParameters.WindowRect;

            if (!relativeToWindow)
            {
                point.Offset(-rect.X, -rect.Y);
            }

            var normalizedPoint = new Point(
                point.X / (rect.Width * 0.9),
                point.Y / (rect.Height * 0.9));

            if (saveToAttemptPoints)
                _AttemptPoints.Add(normalizedPoint);

            return normalizedPoint;
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
            if (distance != null && distance.Value < _minAttemptsDistance)
                _minAttemptsDistance = distance.Value;

            if (_remainingNumberOfAttempts == 0) //last attempt
            {
                DrawImageWithAllAttempts();
                distance = _minAttemptsDistance;
                _remainingNumberOfAttempts = GameSettings.AttemptsAmount;
                CountPointsAfterAttempts();
                points = TotalPoints;
                ResetMinAttemptDistance();
                AttemptNumber = 1;

                if (!IsEyeTribeConnected)
                    _AttemptPoints.Clear();

                return true;
            }
            else
            {
                ActualiseAttemptNumber();
                points = null;
                return false;
            }
        }

        private void ResetMinAttemptDistance()
        {
            if (_WindowViewParameters.WindowState == WindowState.Maximized)
                _minAttemptsDistance = Point.Subtract(_WindowViewParameters.WindowMaximizedRect.TopLeft, _WindowViewParameters.WindowMaximizedRect.BottomRight).Length;
            else
                _minAttemptsDistance = Point.Subtract(_WindowViewParameters.WindowRect.TopLeft, _WindowViewParameters.WindowRect.BottomRight).Length;
        }

        private void ActualiseAttemptNumber() => AttemptNumber = GameSettings.AttemptsAmount - _remainingNumberOfAttempts;

        private void ActualiseRoundNumber() => NumberOfGameRound = GameSettings.RoundsAmount - _gameRoundIndex;

        #endregion//logic methods

        #endregion//Methods

    }
}
