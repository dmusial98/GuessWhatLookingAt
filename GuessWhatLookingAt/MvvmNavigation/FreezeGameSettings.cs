using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhatLookingAt
{
    public class FreezeGameSettings
    {
        public string PupilAdressString { get; set; } = "";

        public int EyeTribePort { get; set; } = 0;

        public int AttemptsAmount { get; set; } = 0;

        public int RoundsAmount { get; set; } = 0;

        public int PhotoTime { get; set; } = 0;

        public int EyeTribeTime { get; set; } = 0;

        public bool DisplayPupilGazePoint { get; set; } = false;

        public bool DisplayEyeTribeGazePoint { get; set; } = false;
    }
}
