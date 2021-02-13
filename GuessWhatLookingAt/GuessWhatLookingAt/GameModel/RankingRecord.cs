using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhatLookingAt
{
    public class ListOfRankingRecords
    {
        public List<RankingRecord> list { get; set; }

        public ListOfRankingRecords() => list = new List<RankingRecord>();

        public void NewElement()
        {
            var args = new RankingRecordSavedEventArgs();
            OnRankingRecordSaved?.Invoke(this, args);
        }

        public event EventHandler<RankingRecordSavedEventArgs> OnRankingRecordSaved;

        public class RankingRecordSavedEventArgs : EventArgs
        {
            
        }
    }

    public class RankingRecord
    {
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public decimal PointsGenerally { get; set; }

        public int PointsInGame { get; set; }

        public int AttemptsAmountInRound { get; set; }

        public int RoundsAmount { get; set; }

        public double AverageDistance { get; set; }


    }
}
