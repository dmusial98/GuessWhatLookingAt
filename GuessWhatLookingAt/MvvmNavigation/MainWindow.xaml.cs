using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class MainWindow : Window
    {
        public event EventHandler<WindowViewParametersEventArgs> WindowViewParametersChangedEvent;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var args = new WindowViewParametersEventArgs(
                new Rect(
                    x: Left,
                    y: Top,
                    width: Width,
                    height: Height));
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
            WindowViewParametersChangedEvent?.Invoke(this, args);
        }

        public class WindowViewParametersEventArgs : EventArgs
        {
            public WindowViewParametersEventArgs(Rect r) => WindowRect = r;
            public Rect WindowRect { get; set; }
        }

        
    }
}
