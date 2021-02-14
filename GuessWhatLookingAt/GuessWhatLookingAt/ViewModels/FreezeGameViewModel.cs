using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace GuessWhatLookingAt
{
    public class FreezeGameViewModel : BaseViewModel, IPageViewModel, INotifyPropertyChanged
    {
        FreezeGameModel model;

        #region XAML Properties
        public ImageSource imageFromPupil { get; set; }

        private string connectPupilButtonContentString = "Connect with Pupil";
        public string ConnectPupilButtonContentString
        {
            get => connectPupilButtonContentString;
            set
            {
                connectPupilButtonContentString = value;
                OnPropertyChanged("ConnectPupilButtonContentString");
            }
        }

        private string connectEyeTribeButtonContentString = "Connect with Eye Tribe";
        public string ConnectEyeTribeButtonContentString
        {
            get => connectEyeTribeButtonContentString;
            set
            {
                connectEyeTribeButtonContentString = value;
                OnPropertyChanged("ConnectEyeTribeButtonContentString");
            }
        }

        private string mouseDistanceToPupilGazePoint = "";
        public string MouseDistanceToPupilGazePoint
        {
            get => mouseDistanceToPupilGazePoint;
            set
            {
                mouseDistanceToPupilGazePoint = value;
                OnPropertyChanged("MouseDistanceToPupilGazePoint");
            }
        }

        private string gameInfoLabelContentString = "";
        public string GameInfoLabelContentString
        {
            get => gameInfoLabelContentString;
            set
            {
                gameInfoLabelContentString = value;
                OnPropertyChanged("GameInfoLabelContentString");
            }
        }

        private string eyeTribeCoordinatesString = "X:      Y:     ";
        public string EyeTribeCoordinatesString
        {
            get => eyeTribeCoordinatesString;
            set
            {
                eyeTribeCoordinatesString = value;
                OnPropertyChanged("EyeTribeCoordinatesString");
            }
        }

        private string startRoundButtonContentString = "Start game";
        public string StartRoundButtonContentString
        {
            get => startRoundButtonContentString;
            set
            {
                startRoundButtonContentString = value;
                OnPropertyChanged("StartRoundButtonContentString");
            }
        }


        private string roundValueLabelContentString = "";
        public string RoundValueLabelContentString
        {
            get => roundValueLabelContentString;
            set
            {
                roundValueLabelContentString = value;
                OnPropertyChanged("RoundValueLabelContentString");
            }
        }

        private string attemptValueLabelContentString = "";
        public string AttemptValueLabelContentString
        {
            get => attemptValueLabelContentString;
            set
            {
                attemptValueLabelContentString = value;
                OnPropertyChanged("AttemptValueLabelContentString");
            }
        }

        private string pointsValueLabelContentString = "";
        public string PointsValueLabelContentString
        {
            get => pointsValueLabelContentString;
            set
            {
                pointsValueLabelContentString = value;
                OnPropertyChanged("PointsValueLabelContentString");
            }
        }

        public static WindowViewParameters _WindowViewParameters { get; set; }
        #endregion

        MainWindow MainWindow { get; set; }
        FreezeGameSettings GameSettings;
        ListOfRankingRecords RankingRecords;

        int _lastMouseClickTimestamp = 0;
        bool _lockMouseLeftButton = false;
        bool _lockEyeTribeTimer = false;
        bool _isLastAttempt = false;
        bool _isLastRound = false;
        int _lastPointsAmount = 0;

        #region Constructor
        public FreezeGameViewModel(MainWindow mainWindow, FreezeGameSettings gameSettings, ListOfRankingRecords rankingRecords)
        {
            MainWindow = mainWindow;
            mainWindow.WindowViewParametersChangedEvent += OnWindowViewParametersChanged;
            mainWindow.GameClosedEvent += OnGameClosed;

            _WindowViewParameters = new WindowViewParameters();
            GameSettings = gameSettings;
            RankingRecords = rankingRecords;
            model = new FreezeGameModel(_WindowViewParameters, GameSettings, rankingRecords);

            model.BitmapSourceReached += OnBitmapSourceReached;
            model.EyeTribeGazePointReached += OnEyeTribeGazePointReached;
            model.PhotoTimeChangedEvent += OnPhotoTimeChanged;
        }
        #endregion

        #region Commands

        #region Go to settings
        private ICommand _goToSettings;

        public ICommand GoToSettings
        {
            get
            {
                return _goToSettings ?? (_goToSettings = new RelayCommand(x =>
                {
                    Mediator.Notify("GoToSettings", "");
                }));
            }
        }
        #endregion

        #region Go to ranking
        private ICommand _goToRanking;

        public ICommand GoToRanking
        {
            get
            {
                return _goToRanking ?? (_goToRanking = new RelayCommand(x =>
                {
                    Mediator.Notify("GoToRanking", "");
                }));
            }
        }
        #endregion

        #region Connect with Pupil 
        private ICommand _ConnectDisconnectWithPupil;
        public ICommand ConnectDisconnectWithPupil
        {
            get
            {
                return _ConnectDisconnectWithPupil ?? (_ConnectDisconnectWithPupil = new RelayCommand(
                    x =>
                    {
                        if (!model.IsPupilConnected)
                            ConnectWithPupil();
                        else
                            DisconnectPupil();
                    }));
            }
        }

        void ConnectWithPupil()
        {
            model.ConnectWithPupil();
            ConnectPupilButtonContentString = "Disconnect Pupil";
            MouseDistanceToPupilGazePoint = "";
        }

        void DisconnectPupil()
        {
            model.DisconnectPupil();
            ConnectPupilButtonContentString = "Connect with Pupil";
        }
        #endregion

        #region Connect with Eye Tribe 
        private ICommand _ConnectDisconnectWithEyeTribe;
        public ICommand ConnectDisconnectWithEyeTribe
        {
            get
            {
                return _ConnectDisconnectWithEyeTribe ?? (_ConnectDisconnectWithEyeTribe = new RelayCommand(
                    x =>
                    {
                        if (!model.IsEyeTribeConnected)
                            ConnectWithEyeTribe();
                        else
                            DisconnectEyeTribe();
                    }));
            }
        }

        void ConnectWithEyeTribe()
        {
            model.ConnectWithEyeTribe();
            ConnectEyeTribeButtonContentString = "Disconnect Eye Tribe";
        }

        void DisconnectEyeTribe()
        {
            model.DisconnectEyeTribe();
            ConnectEyeTribeButtonContentString = "Connect with Eye Tribe";
            EyeTribeCoordinatesString = "X:      Y:     "; ;
        }
        #endregion

        #region Start round
        private ICommand _StartRound;

        public ICommand StartRound
        {
            get
            {
                return _StartRound ?? (_StartRound = new RelayCommand(
                    x =>
                    {
                        if (!model.HasPhoto) //new game
                            ClickedNewGameButton();
                        else if (model.HasPhoto && !_isLastAttempt)
                            ClickedNextAttemptButton();
                        else if (model.HasPhoto && _isLastAttempt) //after last attempt
                            ClickedNextRoundButton();
                    }));
            }
        }

        void ClickedNewGameButton()
        {
            model.ConnectWithPupil();
            model.StartRound();
            RoundValueLabelContentString = model.NumberOfGameRound.ToString();
            AttemptValueLabelContentString = model.AttemptNumber.ToString();
        }

        void ClickedNextAttemptButton()
        {
            model.EyeTribeTimerEvent += OnEyeTribeTimerChanged;
            model.StartEyeTribeTimer();
            AttemptValueLabelContentString = (model.AttemptNumber + 1).ToString();
            _lockEyeTribeTimer = false;
        }

        void ClickedNextRoundButton()
        {
            if (!model.IsPupilConnected)
                model.ConnectWithPupil();

            model.StartRound();
            RoundValueLabelContentString = model.NumberOfGameRound.ToString();
            AttemptValueLabelContentString = model.AttemptNumber.ToString();
        }
        #endregion//Start round

        #region Display Pupil gaze points

        private ICommand _DisplayPupilGazePoint;
        public ICommand DisplayPupilGazePoint
        {
            get
            {
                return _DisplayPupilGazePoint ?? (_DisplayPupilGazePoint = new RelayCommand(
                    x =>
                    {
                        GameSettings.DisplayPupilGazePoint = !GameSettings.DisplayPupilGazePoint;
                    }));
            }
        }

        #endregion

        #region Display Eye Tribe gaze points

        private ICommand _DisplayEyeTribeGazePoint;

        public ICommand DisplayEyeTribeGazePoint
        {
            get
            {
                return _DisplayEyeTribeGazePoint ?? (_DisplayEyeTribeGazePoint = new RelayCommand(
                    x =>
                    {
                        GameSettings.DisplayEyeTribeGazePoint = !GameSettings.DisplayEyeTribeGazePoint;
                    }));
            }
        }

        #endregion

        #endregion

        #region Events services

        void OnWindowViewParametersChanged(object sender, MainWindow.WindowViewParametersEventArgs args)
        {
            _WindowViewParameters.WindowState = args.WndState;

            if (args.WasLoaded && args.WndState == WindowState.Maximized)
                _WindowViewParameters.WindowMaximizedRect = args.WindowRect;
            else
                _WindowViewParameters.WindowRect = args.WindowRect;
        }

        void OnBitmapSourceReached(object sender, FreezeGameModel.BitmapSourceEventArgs args) => LoadImageFromPupil(args.Image);

        Point GetAbsoluteMousePos() => MainWindow.PointToScreen(Mouse.GetPosition(MainWindow));

        private void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (model.HasPhoto && e.LeftButton == MouseButtonState.Pressed &&
                !_lockMouseLeftButton && _lastMouseClickTimestamp != e.Timestamp)
            {
                Point relativeMousePosition = Mouse.GetPosition(MainWindow);
                Point absoluteMousePosition = GetAbsoluteMousePos();

                //when mouse position is located on game window
                if ((_WindowViewParameters.WindowState == WindowState.Maximized && _WindowViewParameters.WindowMaximizedRect.Contains(relativeMousePosition)) ||
                    (_WindowViewParameters.WindowState == WindowState.Normal && _WindowViewParameters.WindowRect.Contains(absoluteMousePosition)))
                {
                    double? distance;
                    int? points;
                    model.MouseAttemptStarted(relativeMousePosition, out distance, out points);

                    _isLastAttempt = points != null;

                    if (_isLastAttempt)
                    {
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            App.Current.MainWindow.MouseDown -= OnLeftMouseButtonDown;
                        });

                        _lockMouseLeftButton = true;
                    }
                    else
                        AttemptValueLabelContentString = (model.AttemptNumber + 1).ToString();

                    //Actualise mouse click timstamp
                    _lastMouseClickTimestamp = e.Timestamp;
                    MouseDistanceToPupilGazePoint = Math.Round(distance.Value, 4).ToString();

                    if (_isLastRound && _isLastAttempt)
                        DisplayAfterLastAttemptInRound();
                    else if (points != null)
                        DisplayAfterLastAttempt(points);
                }
            }
        }

        private void OnPhotoTimeChanged(object sender, FreezeGameModel.PhotoTimeChangedEventArgs args)
        {
            if (args.Time != 0)
            {
                if (args.Time != 1)
                    GameInfoLabelContentString = "Take photo in " + args.Time + " seconds";
                else
                    GameInfoLabelContentString = "Take photo in 1 second";
            }
            else //Time == 0
            {
                if (!_isLastRound)
                    StartRoundButtonContentString = "Start next round";

                GameInfoLabelContentString = "";
                ConnectPupilButtonContentString = "Connect with Pupil";

                if (!model.IsEyeTribeConnected)
                {
                    App.Current.Dispatcher.Invoke(delegate //Guessing gaze position player with mouse using
                        {
                            App.Current.MainWindow.MouseDown += OnLeftMouseButtonDown;
                        });
                    _lockMouseLeftButton = false;
                }
                else // Eye Tribe is connected
                {
                    model.EyeTribeTimerEvent += OnEyeTribeTimerChanged;
                    model.StartEyeTribeTimer();
                    _lockEyeTribeTimer = false;
                }

                _isLastRound = args.IsLastRound;
            }
        }

        private void OnEyeTribeGazePointReached(object sender, FreezeGameModel.EyeTribeGazePositionEventArgs args)
        {
            EyeTribeCoordinatesString = "X: " + Math.Round(args.GazePoint.X, 0).ToString() + " Y: " + Math.Round(args.GazePoint.Y, 0).ToString();
        }

        private void OnEyeTribeTimerChanged(object sender, FreezeGameModel.EyeTribeTimerEventArgs args)
        {
            if (!_lockEyeTribeTimer)
            {
                if (args.Time != 0)
                {
                    if (args.Time != 1)
                        GameInfoLabelContentString = "Save Eye Tribe gaze point to indicate second player gaze point in " + args.Time + " seconds";
                    else
                        GameInfoLabelContentString = "Save Eye Tribe gaze point to indicate second player gaze point in 1 second";
                }
                else //Time == 0
                {
                    GameInfoLabelContentString = "";

                    double? distance;
                    int? points;
                    model.EyeTribeAttemptStarted(out distance, out points);

                    _isLastAttempt = points != null;

                    model.EyeTribeTimerEvent -= OnEyeTribeTimerChanged;
                    _lockEyeTribeTimer = true;

                    MouseDistanceToPupilGazePoint = Math.Round(distance.Value, 4).ToString();

                    if (_isLastRound && _isLastAttempt)
                        DisplayAfterLastAttemptInRound();
                    else if (!_isLastAttempt)
                        StartRoundButtonContentString = "Start next attempt";
                    else
                    {
                        StartRoundButtonContentString = "Start next round";
                        if (points != null)
                            DisplayAfterLastAttempt(points);
                    }
                }
            }
        }

        void DisplayAfterLastAttemptInRound()
        {
            GameInfoLabelContentString = String.Concat("Your score for this game is ",
                            model.TotalPoints.ToString(),
                            "/",
                            (model.NumberOfGameRound * 10).ToString());
            PointsValueLabelContentString = "";
            RoundValueLabelContentString = "";
            AttemptValueLabelContentString = "";
            StartRoundButtonContentString = "Start game";
            _lastPointsAmount = 0;
        }

        void DisplayAfterLastAttempt(int? points)
        {
            PointsValueLabelContentString = String.Concat(
                           points.Value.ToString(),
                           "/",
                           (model.NumberOfGameRound * 10).ToString(),
                           " (+",
                           (points.Value - _lastPointsAmount).ToString(), ")");
            _lastPointsAmount = points.Value;
        }

        private void OnGameClosed(object sender, MainWindow.GameClosedEventArgs args)
        {
            try
            {
                Properties.Settings.Default.NameToRanking = GameSettings.NameToRanking;
                Properties.Settings.Default.PupilAdressString = GameSettings.PupilAdressString;
                Properties.Settings.Default.EyeTribePort = GameSettings.EyeTribePort;
                Properties.Settings.Default.AttemptsAmount = GameSettings.AttemptsAmount;
                Properties.Settings.Default.RoundsAmount = GameSettings.RoundsAmount;
                Properties.Settings.Default.PhotoTime = GameSettings.PhotoTime;
                Properties.Settings.Default.EyeTribeTime = GameSettings.EyeTribeTime;
                Properties.Settings.Default.DisplayPupilGazePoint = GameSettings.DisplayPupilGazePoint;
                Properties.Settings.Default.DisplayEyeTribeGazePoint = GameSettings.DisplayEyeTribeGazePoint;

                Properties.Settings.Default.Save();

                XmlSerializer xml = new XmlSerializer(typeof(ListOfRankingRecords));
                TextWriter xmlWriter = new StreamWriter("rank.xml");
                xml.Serialize(xmlWriter, RankingRecords);
                xmlWriter.Close();
            }
            catch (Exception)
            { }
        }

        #endregion

        #region Actualise XAML
        public void LoadImageFromPupil(ImageSource image)
        {
            if (image != null)
            {
                image.Freeze();
                imageFromPupil = image;

                OnPropertyChanged("imageFromPupil");
            }
        }
        #endregion
    }
}