using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace GuessWhatLookingAt
{
    public class MainMenuViewModel : BaseViewModel, IPageViewModel, INotifyPropertyChanged
    {
        #region Settings variables

        FreezeGameSettings GameSettings;

        string _pupilAdressString = "adres dla pupila";
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

        #endregion

        #region Constructor

        public MainMenuViewModel(FreezeGameSettings gameSettings)
        {
            GameSettings = gameSettings;
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

    }
}