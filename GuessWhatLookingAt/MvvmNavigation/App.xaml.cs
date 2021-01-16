using System.Windows;

namespace GuessWhatLookingAt
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            WindowViewParameters viewSettings = new WindowViewParameters();
            var gameSettings = new FreezeGameSettings();
            gameSettings.PupilAdressString = GuessWhatLookingAt.Properties.Settings.Default.PupilAdressString;
            gameSettings.EyeTribePort = GuessWhatLookingAt.Properties.Settings.Default.EyeTribePort;
            gameSettings.AttemptsAmount = GuessWhatLookingAt.Properties.Settings.Default.AttemptsAmount;
            gameSettings.RoundsAmount = GuessWhatLookingAt.Properties.Settings.Default.RoundsAmount;
            gameSettings.PhotoTime = GuessWhatLookingAt.Properties.Settings.Default.PhotoTime;
            gameSettings.EyeTribeTime = GuessWhatLookingAt.Properties.Settings.Default.EyeTribeTime;
            gameSettings.DisplayPupilGazePoint = GuessWhatLookingAt.Properties.Settings.Default.DisplayPupilGazePoint;
            gameSettings.DisplayEyeTribeGazePoint = GuessWhatLookingAt.Properties.Settings.Default.DisplayEyeTribeGazePoint;

            MainWindow app = new MainWindow();
            MainWindowViewModel context = new MainWindowViewModel(app, gameSettings);
            app.DataContext = context;
            app.Show();
        }
    }
}
