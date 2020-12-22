using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhatLookingAt
{
    public class FreezeGameViewModel : BaseViewModel, IPageViewModel, INotifyPropertyChanged
    {
        FreezeGameModel model = new FreezeGameModel();

        public ImageSource imageFromPupil { get; set; }

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

        private ICommand _ConnectWithPupil;
        public ICommand ConnectWithPupil
        {
            get
            {
                return _ConnectWithPupil ?? (_ConnectWithPupil = new RelayCommand(
                    x =>
                    {
                        model.ConnectWithPupil();
                    }));
            }
        }

        public FreezeGameViewModel()
        {
            model.pupil.PupilDataReceivedEvent += e_PupilDataReached;
        }


        void e_PupilDataReached(object sender, Pupil.PupilReceivedDataEventArgs args)
        {
            LoadImageFromPupil(args.image);
        }

        public void LoadImageFromPupil(ImageSource image)
        {
            image.Freeze();
            imageFromPupil = image;

            this.OnPropertyChanged("imageFromPupil");
        }
    
        
    
    
    
    
    
    
    }
}