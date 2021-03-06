﻿using System;
using System.Windows;


namespace GuessWhatLookingAt
{
    public partial class MainWindow : Window
    {
        public event EventHandler<WindowViewParametersEventArgs> WindowViewParametersChangedEvent;
        public event EventHandler<GameClosedEventArgs> GameClosedEvent;
        public MainWindow() => InitializeComponent();

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var args = new WindowViewParametersEventArgs(
                new Rect(
                    x: Left,
                    y: Top,
                    width: Width,
                    height: Height));
            args.WndState = WindowState;
            WindowViewParametersChangedEvent?.Invoke(this, args);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            var args = new WindowViewParametersEventArgs(
                new Rect(
                    x: Left,
                    y: Top,
                    width: Width,
                    height: Height));
            args.WndState = WindowState;
            WindowViewParametersChangedEvent?.Invoke(this, args);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var args = new WindowViewParametersEventArgs(
                new Rect(
                    x: Left,
                    y: Top,
                    width: Width,
                    height: Height));
            args.WndState = WindowState;
            args.WasLoaded = true;
            WindowViewParametersChangedEvent?.Invoke(this, args);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var args = new GameClosedEventArgs();
            GameClosedEvent?.Invoke(this, args);

            Environment.Exit(Environment.ExitCode);
        }

        public class WindowViewParametersEventArgs : EventArgs
        {
            public WindowViewParametersEventArgs(Rect r) => WindowRect = r;
            public Rect WindowRect { get; set; }
            public WindowState WndState { get; set; }

            public bool WasLoaded { get; set; } = false;
        }

        public class GameClosedEventArgs : EventArgs
        {}
        
    }
}
