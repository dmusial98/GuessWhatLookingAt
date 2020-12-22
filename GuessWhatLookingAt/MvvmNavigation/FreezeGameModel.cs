using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GuessWhatLookingAt
{
    class FreezeGameModel
    {
        Thread pupilThread;
        public Pupil pupil { get; private set; } = new Pupil();

        public void ConnectWithPupil()
        {
            if(!pupil.isConnected)
            {
                pupil.Connect();

                pupilThread = new Thread(pupil.ReceiveData);
                pupilThread.Start();
            }
        }

    }
}
