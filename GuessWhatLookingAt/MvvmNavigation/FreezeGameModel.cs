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
        const int _timerSeconds = 3;
        public int PhotoRemainingTime { get; private set; } = _timerSeconds;
        public bool hasPhoto { get; private set; } = false;

        public event EventHandler<PhotoTimeChangedEventArgs> PhotoTimeChangedEvent;

        public void ConnectWithPupil()
        {
            if (!pupil.isConnected)
            {
                hasPhoto = false;
                pupil.Connect();

                pupilThread = new Thread(pupil.ReceiveFrame);
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
                photoTimer = new System.Timers.Timer(1000);
                photoTimer.Elapsed += OnTakePhotoTimerEvent;
                photoTimer.AutoReset = false;

                OnPhotoTimeEvent();

                photoTimer.Enabled = true;
            }
        }

        public double CountPointsDifference(Point p1)
        {
            return Point.Subtract(pupil.gazePoint, p1).Length;
        }

        private void OnTakePhotoTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (PhotoRemainingTime != 0)
            {
                PhotoRemainingTime--;
                photoTimer.Enabled = true;

                OnPhotoTimeEvent();
            }
            else
            {
                pupil.Disconnect();
                pupilThread?.Abort();
                hasPhoto = true;

                OnPhotoTimeEvent();

                PhotoRemainingTime = _timerSeconds;

                
            }
        }

        private void OnPhotoTimeEvent()
        {
            PhotoTimeChangedEventArgs args = new PhotoTimeChangedEventArgs(PhotoRemainingTime);
            EventHandler<PhotoTimeChangedEventArgs> handler = PhotoTimeChangedEvent;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public class PhotoTimeChangedEventArgs : EventArgs
        {
            public PhotoTimeChangedEventArgs(int time)
            { 
                Time = time; 
            }

            public int Time { get; set; }
        }
    }
}
