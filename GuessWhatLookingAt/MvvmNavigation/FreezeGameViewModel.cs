using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhatLookingAt
{
    public class FreezeGameViewModel : BaseViewModel, IPageViewModel, INotifyPropertyChanged
    {
        FreezeGameModel model = new FreezeGameModel();

        #region XAML Properties
        public ImageSource imageFromPupil { get; set; }

        public string ConnectPupilResultString { get; set; } = "Disconnected";

        public string CoordinatesMouseString { get; set; } = "Coordinates: ";

        public string RemainingPhotoTimeString { get; set; } = "";
        #endregion

        #region Constructors
        public FreezeGameViewModel()
        {
            model.pupil.PupilDataReceivedEvent += e_PupilDataReached;
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
        private ICommand _ConnectWithPupil;
        public ICommand ConnectWithPupil
        {
            get
            {
                return _ConnectWithPupil ?? (_ConnectWithPupil = new RelayCommand(
                    x =>
                    {
                        model.ConnectWithPupil();
                        ConnectPupilResultString = "Connected";
                        OnPropertyChanged("ConnectPupilResultString");
                    }));
            }
        }
        #endregion

        #region Disconnect Pupil
        private ICommand _DisconnectPupil;
        public ICommand DisconnectPupil
        {
            get
            {
                return _DisconnectPupil ?? (_DisconnectPupil = new RelayCommand(
                    x =>
                    {
                        model.DisconnectPupil();
                        ConnectPupilResultString = "Disconnected";
                        OnPropertyChanged("ConnectPupilResultString");
                    }));
            }
        }
        #endregion

        #region Take a photo
        private ICommand _TakePhoto;

        public ICommand TakePhoto
        {
            get
            {
                return _TakePhoto ?? (_TakePhoto = new RelayCommand(
                    x =>
                    {
                        App.Current.MainWindow.MouseDown += OnLeftMouseButtonDown;
                        model.TakePhoto();
                    }));
            }
        }
        #endregion

        #endregion

        #region Events services
        void e_PupilDataReached(object sender, Pupil.PupilReceivedDataEventArgs args)
        {
            LoadImageFromPupil(args.image);
        }

        private void OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (model.hasPhoto)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point mousePosition = /*Point.Subtract(*/Mouse.GetPosition(App.Current.MainWindow)/*, new Point(20,60))*/;
                    var distance = model.CountPointsDifference(mousePosition);
                    distance = Math.Round(distance, 2);
                    CoordinatesMouseString = "Distance: " + distance.ToString();
                    OnPropertyChanged("CoordinatesMouseString");
                }
            }
        }

        private void OnPhotoTimeChanged(object sender, FreezeGameModel.PhotoTimeChangedEventArgs args)
        {
            if(args.Time != 0)
            {
                if (args.Time != 1)
                {
                    RemainingPhotoTimeString = "Take photo in " + args.Time + " seconds";
                    OnPropertyChanged("RemainingPhotoTimeString");
                }
                else
                {
                    RemainingPhotoTimeString = "Take photo in 1 second";
                    OnPropertyChanged("RemainingPhotoTimeString");
                }
            }
            else
            {
                RemainingPhotoTimeString = "";
                OnPropertyChanged("RemainingPhotoTimeString");
            }
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