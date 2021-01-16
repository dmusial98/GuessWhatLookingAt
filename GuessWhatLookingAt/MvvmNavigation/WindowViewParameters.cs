using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GuessWhatLookingAt
{
    public class WindowViewParameters
    {
        public Rect WindowRect { get; set; }

        public Rect WindowMaximizedRect { get; set; }

        public WindowState WindowState { get; set; }
    }
}
