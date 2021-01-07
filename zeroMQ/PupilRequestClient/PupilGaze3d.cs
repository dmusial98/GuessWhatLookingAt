using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PupilRequestClient
{
    public class PupilGaze3d
    {
        public int id { get; set; }
        public string topic { get; set; }
        public string method { get; set; }
        public decimal[] norm_pos { get; set; }
        public decimal diameter { get; set; }
        public decimal timestamp { get; set; }
        public decimal confidence { get; set; }
        
        public Ellipse ellipse { get; set; }

        public decimal model_birth_timestamp { get; set; }
        public decimal model_confidence { get; set; }
        public int model_id { get; set; }

        public int theta { get; set; }
        public int phi { get; set; }

        public Circle_3d circle_3d { get; set; }
        public decimal diameter_3d { get; set; }

        public Sphere sphere { get; set; }
        public Projected_sphere projected_sphere { get; set; }

        public override string ToString()
        {
            return String.Concat("id: ", id.ToString(), " topic: ", topic.ToString(), " method: ", method.ToString(), " norm_pos: ", norm_pos.ToString(), " diameter: ", diameter.ToString(),
                " timestamp: ", timestamp.ToString(), " confidence: ", confidence.ToString(), " ellipse: ", ellipse.ToString(), " model_birth_timestamp: ", model_birth_timestamp.ToString(), 
                " model_confidence: ", model_confidence.ToString(), " model_id: ", model_id.ToString(), " theta: ", theta.ToString(), " phi: ", phi.ToString(), " circle_3d: ", circle_3d.ToString(),
                " diameter_3d: ", diameter_3d.ToString(), " sphere: ", sphere.ToString(), " projected:sphere: ", projected_sphere.ToString());
        }

        public class Ellipse
        {
            public decimal angle { get; set; }
            public decimal[] center { get; set; }
            public decimal[] axes { get; set; }

            public override string ToString()
            {
                return "angle: " + angle.ToString() + " center: " + center.ToString() + "axes: " + axes.ToString();
            }
        }

        public class Circle_3d
        {
            public decimal[] normal { get; set; }
            public decimal radius { get; set; }
            public decimal[] center { get; set; }

            public override string ToString()
            {
                return String.Concat(normal.ToString(), " ", radius.ToString(), " ", center.ToString());
            }
        }

        public class Sphere
        {
            public decimal radius { get; set; }
            public decimal[] center { get; set; }

            public override string ToString()
            {
                return String.Concat(radius.ToString(), " ", center.ToString() );
            }
        }

        public class Projected_sphere
        {
            public decimal angle { get; set; }
            public int[] center { get; set; }
            public int[] axes { get; set; }

            public override string ToString()
            {
                return String.Concat(angle.ToString(), " ", center.ToString(), " ", axes.ToString());
            }
        }

    }
}
