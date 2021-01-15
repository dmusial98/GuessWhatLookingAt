using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhatLookingAt
{
    public class FreezeGameSettings
    {
        public string PupilAdressString { get; set; } = "tcp://127.0.0.1:50020";

        public int EyeTribePort { get; set; } = 6555;

        public int AttemptsAmount { get; set; } = 3;

        public int RoundsAmount { get; set; } = 3;

        public int PhotoTime { get; set; } = 3;

        public int EyeTribeTime { get; set; } = 5;

        public bool DisplayPupilGazePoint { get; set; } = true;

        public bool DisplayEyeTribeGazePoint { get; set; } = true;
    }
}
