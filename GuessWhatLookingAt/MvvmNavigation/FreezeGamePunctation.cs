using System.Collections.Generic;

namespace GuessWhatLookingAt
{
    public class FreezeGamePunctation
    {
        public List<double> PunctationList = new List<double>();
        private int maxPoints = 10;

        public FreezeGamePunctation()
        {
            for(int i = 0; i < maxPoints; i++)
            {
                PunctationList.Add((i + 1) * 50.0);
            }
        }

        public FreezeGamePunctation(List<double> punctation) => PunctationList = punctation;
    }
}
