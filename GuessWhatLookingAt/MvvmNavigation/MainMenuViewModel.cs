using System.Windows.Input;

namespace GuessWhatLookingAt
{
    public class MainMenuViewModel : BaseViewModel, IPageViewModel
    {
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