using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GuessWhatLookingAt
{
    public class RankingViewModel : BaseViewModel, IPageViewModel, INotifyPropertyChanged
    {
        ListOfRankingRecords listOfRankingRecords;

        List<RankingRecord> _rankingRecords;
        public List<RankingRecord> RankingRecords
        {
            get
            {
                return _rankingRecords;
            }
            set
            {
                _rankingRecords = value;
                _rankingRecords.Sort((RankingRecord r1, RankingRecord r2) =>
                {
                    if (r1.PointsGenerally < r2.PointsGenerally)
                        return 1;
                    else if (r1.PointsGenerally > r2.PointsGenerally)
                        return -1;
                    else
                        return 0;
                });
                OnPropertyChanged("RankingRecords");
            }
        }

        public RankingViewModel(ListOfRankingRecords rankingRecords)
        {
            listOfRankingRecords = rankingRecords;
            listOfRankingRecords.OnRankingRecordSaved += OnNewRankingRecord;
            RankingRecords = rankingRecords.list;
        }

        public void OnNewRankingRecord(object sender, ListOfRankingRecords.RankingRecordSavedEventArgs args)
        {
            RankingRecords = listOfRankingRecords.list;
            OnPropertyChanged("RankingRecords");
        }

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
    }
}
