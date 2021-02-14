using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhatLookingAt
{
    public class EmguCVImage
    {
        public Mat OriginalMat { get; private set; }

        public Mat OutMat { get; private set; }

        public void SetMat(IntPtr dataPointer, int frameWidth, int frameHeight)
        {
            try
            {
                OriginalMat = new Mat(frameHeight,
                    frameWidth,
                    DepthType.Cv8U,
                    3,
                    dataPointer,
                    frameWidth * 3);
                OutMat = OriginalMat.Clone();
            }
            catch (Exception)
            {

            }
        }

        public void DrawCircleForPupil(GazePoint point, bool cleanImage = false)
        {
            try
            {
                if (OutMat != null)
                {
                    if (cleanImage)
                        OutMat = OriginalMat.Clone();

                    CvInvoke.Circle(
                        OutMat,
                        new System.Drawing.Point(Convert.ToInt32(point.point.X * OriginalMat.Width), Convert.ToInt32(point.point.Y * OriginalMat.Height)),
                        1,
                        new Emgu.CV.Structure.MCvScalar(0, 128, 0),
                        40);
                }
            }
            catch (Exception)
            {

            }

        }

        public void DrawCircleForEyeTribe(Point point, bool cleanImage = false)
        {
            try
            {
                if (OutMat != null)
                {
                    if (cleanImage)
                        OutMat = OriginalMat.Clone();

                    CvInvoke.Circle(OutMat,
                        new System.Drawing.Point(
                            Convert.ToInt32(point.X * OriginalMat.Width),
                            Convert.ToInt32(point.Y * OriginalMat.Height)),
                        1,
                        new MCvScalar(0, 0, 128),
                        40);
                }
            }
            catch (Exception)
            {

            }
        }

        public void DrawCircleForAttemptPoint(Point point)
        {
            try
            {
                CvInvoke.Circle(OutMat,
                    new System.Drawing.Point(
                        Convert.ToInt32(point.X * OriginalMat.Width),
                        Convert.ToInt32(point.Y * OriginalMat.Height)),
                    1,
                    new Emgu.CV.Structure.MCvScalar(128, 0, 0),
                    40);
            }
            catch (Exception)
            {

            }
        }

        public void CleanImage()
        {
            try
            {
                if (OriginalMat != null)
                    OutMat = OriginalMat.Clone();
            }
            catch (Exception)
            {

            }
        }

        public void DrawLineBetweenPoints(Point p1, Point p2)
        {
            try
            {
                CvInvoke.Line(
                    OutMat,
                    new System.Drawing.Point(
                        Convert.ToInt32(p1.X * OriginalMat.Width),
                        Convert.ToInt32(p1.Y * OriginalMat.Height)),
                    new System.Drawing.Point(
                        Convert.ToInt32(p2.X * OriginalMat.Width),
                        Convert.ToInt32(p2.Y * OriginalMat.Height)),
                    new MCvScalar(128, 128, 0),
                    2);
            }
            catch (Exception)
            {

            }
        }

        public BitmapSource GetBitmapSourceFromMat()
        {
            try
            {
                if (OutMat != null)
                {
                    var byteArray = OutMat.GetRawData(new int[] { });
                    return BitmapSource.Create(OutMat.Width, OutMat.Height, 96, 96, PixelFormats.Bgr24, null, byteArray, OutMat.Width * 3);
                }
            }
            catch (System.NullReferenceException)
            {

            }
            return null;
        }
    }
}
