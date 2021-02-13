using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GuessWhatLookingAt
{
    public class MainWindowViewModel : BaseViewModel
    {
        private IPageViewModel _currentPageViewModel;
        private List<IPageViewModel> _pageViewModels;

        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if (_pageViewModels == null)
                    _pageViewModels = new List<IPageViewModel>();

                return _pageViewModels;
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }
            set
            {
                _currentPageViewModel = value;
                OnPropertyChanged("CurrentPageViewModel");
            }
        }

        private void ChangeViewModel(IPageViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
                PageViewModels.Add(viewModel);

            CurrentPageViewModel = PageViewModels
                .FirstOrDefault(vm => vm == viewModel);
        }

        private void OnGoToSettings(object obj)
        {
            ChangeViewModel(PageViewModels[0]);
        }

        private void OnGoToFreezeGame(object obj)
        {
            ChangeViewModel(PageViewModels[1]);
        }

        private void OnGoToRanking(object obj)
        {
            ChangeViewModel(PageViewModels[2]);
        }

        public MainWindowViewModel(MainWindow mainWindow, FreezeGameSettings gameSettings, ListOfRankingRecords rankingRecords)
        {
            // Add available pages and set page
            PageViewModels.Add(new SettingsViewModel(gameSettings));
            PageViewModels.Add(new FreezeGameViewModel(mainWindow, gameSettings, rankingRecords));
            PageViewModels.Add(new RankingViewModel(rankingRecords));

            CurrentPageViewModel = PageViewModels[1];

            Mediator.Subscribe("GoToSettings", OnGoToSettings);
            Mediator.Subscribe("GoToFreezeGame", OnGoToFreezeGame);
            Mediator.Subscribe("GoToRanking", OnGoToRanking);
        }
    }
}