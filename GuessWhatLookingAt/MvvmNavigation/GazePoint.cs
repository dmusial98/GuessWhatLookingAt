using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GuessWhatLookingAt
{
    public class GazePoint : IComparable
    {
        public Point point { get; set; }
        public double confidence { get; set; } = 0;

        public GazePoint(Point p, double c)
        {
            point = p;
            confidence = c;
        }

        public GazePoint(double x, double y, double c)
        {
            point = new Point(x, y);
            confidence = c;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            GazePoint otherGazePoint = obj as GazePoint;
            if (otherGazePoint != null)
                return confidence.CompareTo(otherGazePoint.confidence);
            else
                throw new ArgumentException("Object is not a GazePoint");
        }
    }
}
