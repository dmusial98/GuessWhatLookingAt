﻿using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GuessWhatLookingAt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        Pupil pupil = new Pupil();
        Thread pupilThread;

        public string ConnectPupilResultString { get; set; } = "Not connected";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ConnectPupilButtonClicked(object sender, RoutedEventArgs e)
        {
            ConnectPupilResultString = "Button clicked";

            pupil.ConnectWithPupil();
            
            if(pupilThread == null)
            {
                pupilThread = new Thread(pupil.ReceiveData);
            }

            //Notify Label about chenged property
            NotifyPropertyChanged("ConnectPupilResultString");
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
