using System.Collections.Generic;

namespace GuessWhatLookingAt
{
    public class FreezeGamePunctation
    {
        public List<double> PunctationList = new List<double>();
        private int maxPoints = 10;

        public FreezeGamePunctation()
        {
            for(int i = maxPoints; i > 0; i--)
            {
                PunctationList.Add(i * 0.05);
            }
        }

        public FreezeGamePunctation(List<double> punctation) => PunctationList = punctation;
    }
}
