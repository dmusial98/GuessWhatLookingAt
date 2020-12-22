using System.ComponentModel;
using System.Drawing;
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
        #endregion

        #region Constructors
        public FreezeGameViewModel()
        {
            model.pupil.PupilDataReceivedEvent += e_PupilDataReached;
        }
        #endregion

        #region Go To second screen
        private ICommand _goTo2;

        public ICommand GoTo2
        {
            get
            {
                return _goTo2 ?? (_goTo2 = new RelayCommand(x =>
                {
                    Mediator.Notify("GoTo2Screen", "");
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
                        model.TakePhoto();
                    }));
            }
        }
        #endregion

        #region Events
        void e_PupilDataReached(object sender, Pupil.PupilReceivedDataEventArgs args)
        {
            LoadImageFromPupil(args.image);
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