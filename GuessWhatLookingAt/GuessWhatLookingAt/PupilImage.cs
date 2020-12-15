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

        public BitmapSource SetSourceImageFromRawBytes(IntPtr dataPointer, int frameWidth, int frameHeight)
        {
            try
            {
                var matrix = new Mat(frameHeight, frameWidth, DepthType.Cv8U, 3, dataPointer, frameWidth * 3);
                var bmp = matrix.ToBitmap();
                var bmpRes = ConvertBitmap(bmp);
                bmp.Dispose();
                matrix.Dispose();
                return bmpRes;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public BitmapSource ConvertBitmap(Bitmap bitmap)
        {

            BitmapSource pupilBitmapImage = null;

            if (bitmap != null)
            {
                IntPtr handle = IntPtr.Zero;
                try
                {
                    handle = bitmap.GetHbitmap();
                    pupilBitmapImage = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    bitmap.Dispose();
                    return pupilBitmapImage;
                }
                catch (Exception ex)
                {
                    return pupilBitmapImage;
                }
            }
            else
            {
                throw new ArgumentNullException("bitmap");
            }
        }

        public Int32 GetColor(byte b, byte g, byte r)
        {
            return Int32.Parse(System.Windows.Media.Color.FromRgb(r, g, b).ToString().Trim('#'), System.Globalization.NumberStyles.HexNumber);
        }

        public BitmapSource DrawImage(Int32[] pixels, int height, int width)
        {
            WriteableBitmap writableImg = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);

            //lock the buffer
            writableImg.Lock();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    IntPtr backbuffer = writableImg.BackBuffer;
                    //the buffer is a monodimensionnal array...
                    backbuffer += j * writableImg.BackBufferStride;
                    backbuffer += i * 4;
                    System.Runtime.InteropServices.Marshal.WriteInt32(backbuffer, pixels[j * width + i]);
                }
            }

            //specify the area to update
            writableImg.AddDirtyRect(new Int32Rect(0, 0, width, height));
            //release the buffer and show the image
            writableImg.Unlock();

            return writableImg;
        }

        public Int32[] GetColorFrame(byte[] bytes)
        {
            List<Int32> ColorFrame = new List<Int32>();
            for (int i = 0; i < bytes.Length; i += 3)
            {
                ColorFrame.Add(GetColor(bytes[i], bytes[i + 1], bytes[i + 2]));
            }

            return ColorFrame.ToArray();
        }
    }
}
