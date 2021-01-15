using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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

            MainWindow app = new MainWindow();
            MainWindowViewModel context = new MainWindowViewModel(app);
            app.DataContext = context;
            app.Show();
        }
    }
}
