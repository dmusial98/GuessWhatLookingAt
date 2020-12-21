using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace GuessWhatLookingAt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        Pupil pupil = new Pupil();
        Thread pupilThread;

        public string ConnectPupilResultString { get; set; } = "Not connected";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            pupil.PupilDataReceivedEvent += e_PupilDataReached;
        }

        private void ConnectPupilButtonClicked(object sender, RoutedEventArgs e)
        {
            ConnectPupilResultString = "Button clicked";

            if (!pupil.isConnected)
            {
                pupil.Connect();

                pupilThread = new Thread(pupil.ReceiveData);
                pupilThread.Start();

                NotifyPropertyChanged("ConnectPupilResultString");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }

        public void LoadImageFromPupil(ImageSource image)
        {
            image.Freeze();

            ImageSource imSource = image;
            PupilImageXAML.Dispatcher.Invoke(() =>
            {
                PupilImageXAML.Source = imSource;
            });
        }

        void e_PupilDataReached(object sender, Pupil.PupilReceivedDataEventArgs args)
        {
            LoadImageFromPupil(args.image);
        }

        private void DisconnectPupilButtonClick(object sender, RoutedEventArgs e)
        {
            pupil.Disconnect();
            if (pupilThread != null)
                pupilThread.Abort();

            ConnectPupilResultString = "Disconnected";

            NotifyPropertyChanged("ConnectPupilResultString");
        }
    }
}
