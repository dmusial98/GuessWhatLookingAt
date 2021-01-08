using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace GuessWhatLookingAt
{
    public class FreezeGameViewModel : BaseViewModel, IPageViewModel, INotifyPropertyChanged
    {
        FreezeGameModel model = new FreezeGameModel(_PupilImageSize, ViewSettings.ImageFromPupilPosition);

        #region XAML Properties
        public ImageSource imageFromPupil { get; set; }

        public string ConnectPupilButtonContentString { get; set; } = "Connect with Pupil";

        public string ConnectEyeTribeButtonContentString { get; set; } = "Connect with Eye Tribe";

        public string MouseDistanceToPupilGazePoint { get; set; } = "";

        public string GameInfoLabelContentString { get; set; } = "";

        public string EyeTribeCoordinatesString { get; set; } = "X:      Y:     ";

        public string StartRoundButtonContentString { get; set; } = "Start game";

        public string RoundValueLabelContentString { get; set; } = "";

        public string PointsValueLabelContentString { get; set; } = "";

        public static FreezeGameViewSettings ViewSettings { get; set; } = new FreezeGameViewSettings();

        #endregion

        int _lastMouseClickTimestamp = 0;
        bool _lockMouseLeftButton = false;
        bool _isLastRound = false;

        static readonly Size _PupilImageSize = new Size(1632.0, 918.0);

        #region Constructors
        public FreezeGameViewModel()
        {
            model.BitmapSourceReached += OnBitmapSourceReached;
            model.EyeTribeGazePointReached += OnEyeTribeGazePointReached;
            model.PhotoTimeChangedEvent += OnPhotoTimeChanged;
        }
        #endregion

        #region Commands

        #region Go to main menu
        private ICommand _goToMainMenu;

        public ICommand GoToMainMenu
        {
            get
            {
                return _goToMainMenu ?? (_goToMainMenu = new RelayCommand(x =>
                {
                    Mediator.Notify("GoToMainMenu", "");
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
                        if (!model.IsPupilConnected)
                            model.ConnectWithPupil();

                        model.StartRound();
                        RoundValueLabelContentString = model.NumberOfGameRound.ToString();
                        OnPropertyChanged("RoundValueLabelContentString");
                    }));
            }
        }
        #endregion//Start round

        #endregion

        #region Events services
        void OnBitmapSourceReached(object sender, FreezeGameModel.BitmapSourceEventArgs args)
        {
            LoadImageFromPupil(args.image);
        }

        private void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (model.hasPhoto)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (!_lockMouseLeftButton && _lastMouseClickTimestamp != e.Timestamp)
                    {
                        Point mousePosition = Mouse.GetPosition(App.Current.MainWindow);

                        if (model.ImageRect.Contains(mousePosition))
                        {
                            double? distance;
                            int? points;
                            var isLastAttempt = model.MouseAttemptStarted(mousePosition, out distance, out points);

                            if (isLastAttempt)
                            {
                                App.Current.Dispatcher.Invoke(delegate
                                {
                                    App.Current.MainWindow.MouseDown -= OnLeftMouseButtonDown;
                                });

                                _lockMouseLeftButton = true;
                            }

                            _lastMouseClickTimestamp = e.Timestamp;
                            MouseDistanceToPupilGazePoint = distance.ToString();
                            OnPropertyChanged("MouseDistanceToPupilGazePoint");

                            if (_isLastRound && isLastAttempt)
                            {
                                GameInfoLabelContentString = "Congratulations, your score for this game is " + model.TotalPoints.ToString();
                                PointsValueLabelContentString = "";
                                RoundValueLabelContentString = "";
                                StartRoundButtonContentString = "Start game";
                                OnPropertyChanged("GameInfoLabelContentString");
                                OnPropertyChanged("PointsValueLabelContentString");
                                OnPropertyChanged("RoundValueLabelContentString");
                                OnPropertyChanged("StartRoundButtonContentString");
                            }
                            else if (points != null)
                            {
                                PointsValueLabelContentString = points.Value.ToString();
                                OnPropertyChanged("PointsValueLabelContentString");
                            }
                        }
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
            else
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
                    App.Current.Dispatcher.Invoke(delegate
                        {
                            App.Current.MainWindow.MouseDown += OnLeftMouseButtonDown;
                        });
                    _lockMouseLeftButton = false;
                }

                _isLastRound = args.IsLastRound;
                //model.SavePhotoImage();
            }
        }

        private void OnEyeTribeGazePointReached(object sender, FreezeGameModel.EyeTribeGazePositionEventArgs args)
        {
            EyeTribeCoordinatesString = "X: " + Math.Round(args.gazePoint.X, 0).ToString() + " Y: " + Math.Round(args.gazePoint.Y, 0).ToString();
            OnPropertyChanged("EyeTribeCoordinatesString");
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