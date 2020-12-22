using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace GuessWhatLookingAt
{
    class FreezeGameModel
    {
        Thread pupilThread;
        public Pupil pupil { get; private set; } = new Pupil();

        System.Timers.Timer photoTimer;
        const int timerSeconds = 3;
        public int tempTimerSeconds { get; private set; } = timerSeconds;
        public bool hasPhoto { get; private set; } = false;

        public void ConnectWithPupil()
        {
            if (!pupil.isConnected)
            {
                hasPhoto = false;
                pupil.Connect();

                pupilThread = new Thread(pupil.ReceiveData);
                pupilThread.Start();
            }
        }

        public void DisconnectPupil()
        {
            if (pupil.isConnected)
            {
                pupil.Disconnect();
                pupilThread?.Abort();
            }
        }

        public void TakePhoto()
        {
            if (pupil.isConnected)
            {
                //for (int i = 0; i < timerSeconds; i++)
                //{
                //    photoTimer = new System.Timers.Timer(1000);
                //    photoTimer.Elapsed += OnTakePhotoTimerEvent;
                //    photoTimer.Enabled = true;
                //}

                photoTimer = new System.Timers.Timer(1000);
                photoTimer.Elapsed += OnTakePhotoTimerEvent;
                photoTimer.Enabled = true;
                photoTimer.AutoReset = false;
            }
        }

        public double CountPointsDifference(Point p1)
        {
            return Point.Subtract(pupil.gazePoint, p1).Length;
        }

        private void OnTakePhotoTimerEvent(Object source, ElapsedEventArgs e)
        {
            //if (tempTimerSeconds != 0)
            //{
            //    tempTimerSeconds--;
            //}
            //else
            //{
                pupil.Disconnect();
                pupilThread?.Abort();
                hasPhoto = true;
                tempTimerSeconds = timerSeconds;
            //}
        }
    }
}
