using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace GuessWhatLookingAt
{
    public class SettingsViewModel : BaseViewModel, IPageViewModel, INotifyPropertyChanged
    {
        #region Settings variables

        FreezeGameSettings GameSettings;

        string _nameToRanking = "";
        public string NameToRanking
        {
            get
            {
                return _nameToRanking;
            }
            set
            {
                _nameToRanking = value;
                OnPropertyChanged("NameToRanking");
            }
        }

        string _pupilAdressString = "";
        public string PupilAdressString
        {
            get
            {
                return _pupilAdressString;
            }
            set
            {
                _pupilAdressString = value;
                OnPropertyChanged("PupilAdressString");
            }
        }

        string _eyeTribePortString = "6255";
        public string EyeTribePortString
        {
            get
            {
                return _eyeTribePortString;
            }

            set
            {
                _eyeTribePortString = value;
                OnPropertyChanged("EyeTribePortString");
            }

        }


        int _attemptsAmount = 3;
        public int AttemptsAmount
        {
            get
            {
                return _attemptsAmount;
            }
            set
            {
                _attemptsAmount = value;
                OnPropertyChanged("AttemptsAmount");
            }
        }

        int _roundsAmount = 7;
        public int RoundsAmount
        {
            get
            {
                return _roundsAmount;
            }
            set
            {
                _roundsAmount = value;
                OnPropertyChanged("RoundsAmount");
            }
        }

        int _photoTime = 3;
        public int PhotoTime
        {
            get
            {
                return _photoTime;
            }
            set
            {
                _photoTime = value;
                OnPropertyChanged("PhotoTime");
            }
        }

        int _eyeTribeTime = 5;
        public int EyeTribeTime
        {
            get
            {
                return _eyeTribeTime;
            }
            set
            {
                _eyeTribeTime = value;
                OnPropertyChanged("EyeTribeTime");
            }
        }

        bool _displayPupilGazePoint = false;
        public bool DisplayPupilGazePoint
        {
            get
            {
                return _displayPupilGazePoint;
            }
            set
            {
                _displayPupilGazePoint = value;
                OnPropertyChanged("DisplayPupilGazePoint");
            }
        }

        bool _displayEyeTribeGazePoint = false;
        public bool DisplayEyeTribeGazePoint
        {
            get
            {
                return _displayEyeTribeGazePoint;
            }
            set
            {
                _displayEyeTribeGazePoint = value;
                OnPropertyChanged("DisplayEyeTribeGazePoint");
            }
        }

        #endregion

        #region Constructor

        public SettingsViewModel(FreezeGameSettings gameSettings)
        {
            GameSettings = gameSettings;
            _nameToRanking = GameSettings.NameToRanking;
            _pupilAdressString = GameSettings.PupilAdressString;
            _eyeTribePortString = GameSettings.EyeTribePort.ToString();
            _attemptsAmount = GameSettings.AttemptsAmount;
            _roundsAmount = GameSettings.RoundsAmount;
            _photoTime = GameSettings.PhotoTime;
            _eyeTribeTime = GameSettings.EyeTribeTime;
            _displayPupilGazePoint = GameSettings.DisplayPupilGazePoint;
            _displayEyeTribeGazePoint = GameSettings.DisplayEyeTribeGazePoint;
        }

        #endregion

        #region GoToFreezeGame

        private ICommand _goToFreezeGame;

        public ICommand GoToFreezeGame
        {
            get
            {
                return _goToFreezeGame ?? (_goToFreezeGame = new RelayCommand(x =>
                {
                    Mediator.Notify("GoToFreezeGame", "");
                }));
            }
        }

        #endregion


        private ICommand _saveSettings;

        public ICommand SaveSettings
        {
            get
            {
                return _saveSettings ?? (_saveSettings = new RelayCommand(x =>
                {
                    GameSettings.NameToRanking = _nameToRanking;
                    GameSettings.PupilAdressString = _pupilAdressString;
                    GameSettings.EyeTribePort = Int32.Parse(_eyeTribePortString);
                    GameSettings.AttemptsAmount = _attemptsAmount;
                    GameSettings.RoundsAmount = _roundsAmount;
                    GameSettings.PhotoTime = _photoTime;
                    GameSettings.EyeTribeTime = _eyeTribeTime;
                    GameSettings.DisplayPupilGazePoint = _displayPupilGazePoint;
                    GameSettings.DisplayEyeTribeGazePoint = _displayEyeTribeGazePoint;
                }));
            }
        }
    }
}