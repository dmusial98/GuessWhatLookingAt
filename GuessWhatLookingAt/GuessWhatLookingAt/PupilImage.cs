using Emgu.CV;
using Emgu.CV.CvEnum;
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
        public Mat Matrix {get; private set; }

        public void SetSourceImageFromRawBytes(IntPtr dataPointer, int frameWidth, int frameHeight)
        {
            try
            {
                Matrix = new Mat(frameHeight, frameWidth, DepthType.Cv8U, 3, dataPointer, frameWidth * 3);
                var bmp = Matrix.ToBitmap();
                ConvertBitmap(bmp);
                bmp.Dispose();
                Matrix.Dispose();
            }
            catch(Exception ex)
            {

            }
        }

        public void ConvertBitmap(Bitmap bitmap)
        {
            if(bitmap != null)
            {
                IntPtr handle = IntPtr.Zero;
                try
                {
                    handle = bitmap.GetHbitmap();
                    pupilBitmapImage = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    bitmap.Dispose();
                }
                catch (Exception ex)
                {
                    
                }
            }
            else
            {
                throw new ArgumentNullException("bitmap");
            }
        }



    }
}
