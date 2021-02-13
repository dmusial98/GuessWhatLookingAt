using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

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

            //WindowViewParameters viewSettings = new WindowViewParameters();

            #region Reading settings
            
            var gameSettings = new FreezeGameSettings();
            gameSettings.NameToRanking = GuessWhatLookingAt.Properties.Settings.Default.NameToRanking;
            gameSettings.PupilAdressString = GuessWhatLookingAt.Properties.Settings.Default.PupilAdressString;
            gameSettings.EyeTribePort = GuessWhatLookingAt.Properties.Settings.Default.EyeTribePort;
            gameSettings.AttemptsAmount = GuessWhatLookingAt.Properties.Settings.Default.AttemptsAmount;
            gameSettings.RoundsAmount = GuessWhatLookingAt.Properties.Settings.Default.RoundsAmount;
            gameSettings.PhotoTime = GuessWhatLookingAt.Properties.Settings.Default.PhotoTime;
            gameSettings.EyeTribeTime = GuessWhatLookingAt.Properties.Settings.Default.EyeTribeTime;
            gameSettings.DisplayPupilGazePoint = GuessWhatLookingAt.Properties.Settings.Default.DisplayPupilGazePoint;
            gameSettings.DisplayEyeTribeGazePoint = GuessWhatLookingAt.Properties.Settings.Default.DisplayEyeTribeGazePoint;

            #endregion

            #region Reading ranking records

            ListOfRankingRecords rankingRecords = new ListOfRankingRecords();

            try
            {
                var xml = new XmlSerializer(typeof(ListOfRankingRecords));
                FileStream fs = new FileStream("rank.xml", FileMode.OpenOrCreate);
                TextReader reader = new StreamReader(fs);
                rankingRecords = (ListOfRankingRecords)xml.Deserialize(reader);
            }
            catch (Exception)
            {
                rankingRecords = new ListOfRankingRecords();
            }

            #endregion

            MainWindow app = new MainWindow();
            MainWindowViewModel context = new MainWindowViewModel(app, gameSettings, rankingRecords);
            app.DataContext = context;
            app.Show();
        }
    }
}
