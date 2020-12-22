using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhatLookingAt
{
    public class PupilImage
    {
        public ImageSource pupilBitmapImage { get; private set; }
        public Mat mat {get; private set; }

        VideoWriter videoWriter = null;

        public void SetMat(IntPtr dataPointer, int frameWidth, int frameHeight)
        {
            try
            {
                mat = new Mat(frameHeight, frameWidth, DepthType.Cv8U, 3, dataPointer, frameWidth * 3);
            }
            catch(Exception ex)
            {
                
            }
        }
 
        public void DrawCircle(double xGaze, double yGaze)
        {
            CvInvoke.Circle(mat, new System.Drawing.Point(Convert.ToInt32(xGaze), Convert.ToInt32(yGaze)), 15, new Emgu.CV.Structure.MCvScalar(0, 128, 0), 40);
        }

        public void PutConfidenceText(double confidence)
        {
            string confidenceString = "Confidence: " + Math.Round(confidence, 3).ToString();
            MCvScalar color = new MCvScalar(20, 255 * confidence, 1 - 255 * confidence);

            CvInvoke.PutText(mat, confidenceString, new System.Drawing.Point(200, 700), FontFace.HersheyDuplex, 1.0, color );
        }

        public BitmapSource GetBitmapSourceFromMat()
        {
            var byteArray = mat.GetRawData(new int[] { });
            return BitmapSource.Create(mat.Width, mat.Height, 96, 96, PixelFormats.Bgr24, null, byteArray, mat.Width * 3);
        }

        public void StartRecord()
        {
            if(videoWriter == null)
            {
                videoWriter = new VideoWriter("pupilVideo.mp4", VideoWriter.Fourcc('M','P','4','V'), 30, new System.Drawing.Size(mat.Width, mat.Height), true);
            }
        }

        public void AddFrameToVideo()
        {
            if(videoWriter.IsOpened)
            {
                videoWriter.Write(mat);
            }
        }

        public void StopRecord()
        {
            if(videoWriter != null)
            {
                videoWriter.Dispose();
            }
        }
    }
}
