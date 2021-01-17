﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace GuessWhatLookingAt
{
    public class FreezeGameViewModel : BaseViewModel, IPageViewModel, INotifyPropertyChanged
    {
        FreezeGameModel model;

        #region XAML Properties
        public ImageSource imageFromPupil { get; set; }

        public string ConnectPupilButtonContentString { get; set; } = "Connect with Pupil";

        public string ConnectEyeTribeButtonContentString { get; set; } = "Connect with Eye Tribe";

        public string MouseDistanceToPupilGazePoint { get; set; } = "";

        public string GameInfoLabelContentString { get; set; } = "";

        public string EyeTribeCoordinatesString { get; set; } = "X:      Y:     ";

        public string StartRoundButtonContentString { get; set; } = "Start game";

        public string RoundValueLabelContentString { get; set; } = "";

        public string AttemptValueLabelContentString { get; set; } = "";

        public string PointsValueLabelContentString { get; set; } = "";

        public static WindowViewParameters _WindowViewParameters { get; set; }
        #endregion

        MainWindow MainWindow { get; set; }
        FreezeGameSettings GameSettings;

        int _lastMouseClickTimestamp = 0;
        bool _lockMouseLeftButton = false;
        bool _lockEyeTribeTimer = false;
        bool _isLastAttempt = false;
        bool _isLastRound = false;
        int _lastPointsAmount = 0;

        #region Constructors
        public FreezeGameViewModel(MainWindow mainWindow, FreezeGameSettings gameSettings)
        {
            MainWindow = mainWindow;
            mainWindow.WindowViewParametersChangedEvent += OnWindowViewParametersChanged;
            mainWindow.GameClosedEvent += OnGameClosed;

            _WindowViewParameters = new WindowViewParameters();
            GameSettings = gameSettings;
            model = new FreezeGameModel(_WindowViewParameters, GameSettings);

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
                        {
                            model.ConnectWithPupil();
                            ConnectPupilButtonContentString = "Disconnect Pupil";
                            MouseDistanceToPupilGazePoint = "";
                            OnPropertyChanged("ConnectPupilButtonContentString");
                            OnPropertyChanged("MouseDistanceToPupilGazePoint");
                        }
                        else
                        {
                            model.DisconnectPupil();
                            ConnectPupilButtonContentString = "Connect with Pupil";
                            OnPropertyChanged("ConnectPupilButtonContentString");
                        }
                    }));
            }
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
                        {
                            model.ConnectWithEyeTribe();
                            ConnectEyeTribeButtonContentString = "Disconnect Eye Tribe";
                            OnPropertyChanged("ConnectEyeTribeButtonContentString");
                        }
                        else
                        {
                            model.DisconnectEyeTribe();
                            ConnectEyeTribeButtonContentString = "Connect with Eye Tribe";
                            EyeTribeCoordinatesString = "X:      Y:     "; ;
                            OnPropertyChanged("ConnectEyeTribeButtonContentString");
                            OnPropertyChanged("EyeTribeCoordinatesString");
                        }
                    }));
            }
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
                        {
                            model.StartRound();
                            RoundValueLabelContentString = model.NumberOfGameRound.ToString();
                            AttemptValueLabelContentString = model.AttemptNumber.ToString();
                            OnPropertyChanged("RoundValueLabelContentString");
                            OnPropertyChanged("AttemptValueLabelContentString");
                        }
                        else if (model.HasPhoto && !_isLastAttempt)
                        {
                            model.EyeTribeTimerEvent += OnEyeTribeTimerChanged;
                            model.StartEyeTribeTimer();
                            AttemptValueLabelContentString = (model.AttemptNumber + 1).ToString();
                            OnPropertyChanged("AttemptValueLabelContentString");
                            _lockEyeTribeTimer = false;
                        }
                        else if (model.HasPhoto && _isLastAttempt) //after last attempt
                        {
                            if (!model.IsPupilConnected)
                                model.ConnectWithPupil();

                            model.StartRound();
                            RoundValueLabelContentString = model.NumberOfGameRound.ToString();
                            AttemptValueLabelContentString = model.AttemptNumber.ToString();
                            OnPropertyChanged("AttemptValueLabelContentString");
                            OnPropertyChanged("RoundValueLabelContentString");
                        }
                    }));
            }
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
            GameInfoLabelContentString = "Window Size = " + args.WindowRect.Width.ToString() +
                " x " + args.WindowRect.Height.ToString();

            OnPropertyChanged("GameInfoLabelContentString");
            _WindowViewParameters.WindowState = args.WndState;

            if (args.WasLoaded && args.WndState == WindowState.Maximized)
                _WindowViewParameters.WindowMaximizedRect = args.WindowRect;
            else
                _WindowViewParameters.WindowRect = args.WindowRect;
        }

        void OnBitmapSourceReached(object sender, FreezeGameModel.BitmapSourceEventArgs args)
        {
            LoadImageFromPupil(args.Image);
        }

        Point GetAbsoluteMousePos() => MainWindow.PointToScreen(Mouse.GetPosition(MainWindow));

        private void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (model.HasPhoto && e.LeftButton == MouseButtonState.Pressed &&
                !_lockMouseLeftButton && _lastMouseClickTimestamp != e.Timestamp)
            {
                Point relativeMousePosition = Mouse.GetPosition(MainWindow);
                Point absoluteMousePosition = GetAbsoluteMousePos();


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
                    {
                        AttemptValueLabelContentString = (model.AttemptNumber + 1).ToString();
                        OnPropertyChanged("AttemptValueLabelContentString");
                    }

                    _lastMouseClickTimestamp = e.Timestamp;
                    MouseDistanceToPupilGazePoint = Math.Round(distance.Value, 4).ToString();
                    OnPropertyChanged("MouseDistanceToPupilGazePoint");


                    if (_isLastRound && _isLastAttempt)
                    {
                        //TODO: pozniej zrobic funkcje ktora to zrobi, bo jest powtorka w eye tribe
                        GameInfoLabelContentString = String.Concat("Your score for this game is ",
                            model.TotalPoints.ToString(),
                            "/",
                            (model.NumberOfGameRound * 10).ToString());
                        PointsValueLabelContentString = "";
                        RoundValueLabelContentString = "";
                        AttemptValueLabelContentString = "";
                        StartRoundButtonContentString = "Start game";
                        OnPropertyChanged("GameInfoLabelContentString");
                        OnPropertyChanged("PointsValueLabelContentString");
                        OnPropertyChanged("RoundValueLabelContentString");
                        OnPropertyChanged("StartRoundButtonContentString");
                        OnPropertyChanged("AttemptValueLabelContentString");
                        _lastPointsAmount = 0;
                    }
                    else if (points != null)
                    {
                        //TODO: pozniej zrobic funkcje ktora to zrobi, bo jest powtorka w eye tribe
                        PointsValueLabelContentString = String.Concat(
                            points.Value.ToString(),
                            "/",
                            (model.NumberOfGameRound * 10).ToString(),
                            " (+",
                            (points.Value - _lastPointsAmount).ToString(), ")");
                        _lastPointsAmount = points.Value;
                        OnPropertyChanged("PointsValueLabelContentString");
                    }
                }
            }
        }

        private void OnPhotoTimeChanged(object sender, FreezeGameModel.PhotoTimeChangedEventArgs args)
        {
            if (args.Time != 0)
            {
                if (args.Time != 1)
                {
                    GameInfoLabelContentString = "Take photo in " + args.Time + " seconds";
                    OnPropertyChanged("GameInfoLabelContentString");
                }
                else
                {
                    GameInfoLabelContentString = "Take photo in 1 second";
                    OnPropertyChanged("GameInfoLabelContentString");
                }
            }
            else //Time == 0
            {
                if (!_isLastRound)
                {
                    StartRoundButtonContentString = "Start next round";
                    OnPropertyChanged("StartRoundButtonContentString");
                }

                GameInfoLabelContentString = "";
                ConnectPupilButtonContentString = "Connect with Pupil";
                OnPropertyChanged("GameInfoLabelContentString");
                OnPropertyChanged("ConnectPupilButtonContentString");


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
            OnPropertyChanged("EyeTribeCoordinatesString");
        }

        private void OnEyeTribeTimerChanged(object sender, FreezeGameModel.EyeTribeTimerEventArgs args)
        {
            if (!_lockEyeTribeTimer)
            {
                if (args.Time != 0)
                {
                    if (args.Time != 1)
                    {
                        GameInfoLabelContentString = "Save Eye Tribe gaze point to indicate second player gaze point in " + args.Time + " seconds";
                        OnPropertyChanged("GameInfoLabelContentString");
                    }
                    else
                    {
                        GameInfoLabelContentString = "Save Eye Tribe gaze point to indicate second player gaze point in 1 second";
                        OnPropertyChanged("GameInfoLabelContentString");
                    }
                }
                else //Time == 0
                {
                    GameInfoLabelContentString = "";
                    OnPropertyChanged("GameInfoLabelContentString");

                    double? distance;
                    int? points;
                    model.EyeTribeAttemptStarted(out distance, out points);

                    _isLastAttempt = points != null ? true : false;

                    model.EyeTribeTimerEvent -= OnEyeTribeTimerChanged;
                    _lockEyeTribeTimer = true;

                    MouseDistanceToPupilGazePoint = distance.ToString();
                    OnPropertyChanged("MouseDistanceToPupilGazePoint");

                    if (_isLastRound && _isLastAttempt)
                    {
                        GameInfoLabelContentString = String.Concat("Your score for this game is ",
                            model.TotalPoints.ToString(),
                            "/",
                            (model.NumberOfGameRound * 10).ToString());
                        PointsValueLabelContentString = "";
                        RoundValueLabelContentString = "";
                        StartRoundButtonContentString = "Start game";
                        AttemptValueLabelContentString = "";
                        OnPropertyChanged("GameInfoLabelContentString");
                        OnPropertyChanged("PointsValueLabelContentString");
                        OnPropertyChanged("RoundValueLabelContentString");
                        OnPropertyChanged("StartRoundButtonContentString");
                        OnPropertyChanged("AttemptValueLabelContentString");
                        _lastPointsAmount = 0;
                    }
                    else if (!_isLastAttempt)
                    {
                        StartRoundButtonContentString = "Start next attempt";
                        OnPropertyChanged("StartRoundButtonContentString");
                    }
                    else if (_isLastAttempt)
                    {
                        StartRoundButtonContentString = "Start next round";
                        OnPropertyChanged("StartRoundButtonContentString");
                        if (points != null)
                        {
                            PointsValueLabelContentString = String.Concat(
                                points.Value.ToString(),
                                "/",
                                (model.NumberOfGameRound * 10).ToString(),
                                " (+",
                                (points.Value - _lastPointsAmount).ToString(), ")");
                            _lastPointsAmount = points.Value;
                            OnPropertyChanged("PointsValueLabelContentString");
                        }


                    }
                }
            }
        }

        private void OnGameClosed(object sender, MainWindow.GameClosedEventArgs args)
        {
            Properties.Settings.Default.PupilAdressString = GameSettings.PupilAdressString;
            Properties.Settings.Default.EyeTribePort = GameSettings.EyeTribePort;
            Properties.Settings.Default.AttemptsAmount = GameSettings.AttemptsAmount;
            Properties.Settings.Default.RoundsAmount = GameSettings.RoundsAmount;
            Properties.Settings.Default.PhotoTime = GameSettings.PhotoTime;
            Properties.Settings.Default.EyeTribeTime = GameSettings.EyeTribeTime;
            Properties.Settings.Default.DisplayPupilGazePoint = GameSettings.DisplayPupilGazePoint;
            Properties.Settings.Default.DisplayEyeTribeGazePoint = GameSettings.DisplayEyeTribeGazePoint;

            Properties.Settings.Default.Save();
        }

        #endregion

        #region Actualise XAML
        public void LoadImageFromPupil(ImageSource image)
        {
            image.Freeze();
            imageFromPupil = image;

            OnPropertyChanged("imageFromPupil");
        }
        #endregion
    }
}