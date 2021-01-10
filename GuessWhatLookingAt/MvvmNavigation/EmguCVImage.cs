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

        VideoWriter videoWriter = null;

        public void SetMat(IntPtr dataPointer, int frameWidth, int frameHeight)
        {
            try
            {
                OriginalMat = new Mat(frameHeight, frameWidth, DepthType.Cv8U, 3, dataPointer, frameWidth * 3);
                OutMat = OriginalMat.Clone();
            }
            catch (Exception ex)
            {

            }
        }

        public void DrawCircleForPupil(Point gazePoint, double confidence, bool cleanImage = false)
        {
            if (cleanImage)
                OutMat = OriginalMat.Clone();

            //if(confidence > 0.5)
                CvInvoke.Circle(
                    OutMat, 
                    new System.Drawing.Point(Convert.ToInt32(gazePoint.X), Convert.ToInt32(gazePoint.Y)),
                    8, 
                    new Emgu.CV.Structure.MCvScalar(0, 128, 0),
                    40);
        }

        public void DrawCircleForEyeTribe(Point gazePoint, bool cleanImage = false)
        {
            if (cleanImage)
                OutMat = OriginalMat.Clone();

            CvInvoke.Circle(OutMat, new System.Drawing.Point(Convert.ToInt32(gazePoint.X), Convert.ToInt32(gazePoint.Y)), 8, new Emgu.CV.Structure.MCvScalar(0, 0, 128), 40);
        }

        public void PutConfidenceText(double confidence)
        {
            string confidenceString = "Confidence: " + Math.Round(confidence, 3).ToString();
            MCvScalar color = new MCvScalar(20, 255 * confidence, 1 - 255 * confidence);

            CvInvoke.PutText(OriginalMat, confidenceString, new System.Drawing.Point(200, 700), FontFace.HersheyDuplex, 1.0, color);
        }

        public BitmapSource GetBitmapSourceFromMat(double XScale, double YScale)
        {
            var byteArray = OutMat.GetRawData(new int[] { });
            var bmpSource = BitmapSource.Create(OutMat.Width, OutMat.Height, 96, 96, PixelFormats.Bgr24, null, byteArray, OutMat.Width * 3);

            return new TransformedBitmap(bmpSource, new ScaleTransform(XScale, YScale));

        }

        public void SaveImage(string path)
        {
            OutMat.Save("photo.jpeg");
        }

        public void StartRecord()
        {
            if (videoWriter == null)
            {
                videoWriter = new VideoWriter("pupilVideo.mp4", VideoWriter.Fourcc('M', 'P', '4', 'V'), 30, new System.Drawing.Size(OriginalMat.Width, OriginalMat.Height), true);
            }
        }

        public void AddFrameToVideo()
        {
            if (videoWriter.IsOpened)
            {
                videoWriter.Write(OutMat);
            }
        }

        public void StopRecord()
        {
            if (videoWriter != null)
            {
                videoWriter.Dispose();
            }
        }
    }
}
