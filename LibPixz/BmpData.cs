using System;
using System.Drawing;
using System.Drawing.Imaging;
using LibPixz.Colorspaces;

namespace LibPixz
{
	unsafe internal class BmpData
    {
        private struct PixelData
        {
            public byte blue;
            public byte green;
            public byte red;
            public byte alpha;

            public override string ToString()
            {
                return "(" + alpha.ToString() + ", " + red.ToString() + ", " + green.ToString() + ", " + blue.ToString() + ")";
            }
        }

        internal int Width
        {
            get { return width; }
        }

        internal int Height
        {
            get { return height; }
        }

        internal Bitmap WorkingBitmap
        {
            get { return workingBitmap; }
        }

        int width = 0;
        int height = 0;
        int stride = 0;
        Bitmap workingBitmap = null;
        BitmapData bitmapData = null;
		byte* pBase = null;
        PixelData* pixelData = null;
        
        internal BmpData(Bitmap inputBitmap)
        {
            workingBitmap = inputBitmap;
            width = workingBitmap.Width;
            height = workingBitmap.Height;
        }

        void LockImage()
        {
            var bounds = new Rectangle(Point.Empty, workingBitmap.Size);

            stride = (int)(bounds.Width * sizeof(PixelData));
            if (stride % 4 != 0) stride = 4 * (stride / 4 + 1);

            //Lock Image
            bitmapData = workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            pBase = (byte*)bitmapData.Scan0.ToPointer();
        }

        internal Color2 GetPixel(int x, int y)
        {
            pixelData = (PixelData*)(pBase + y * stride + x * sizeof(PixelData));
            return new Color2() { a = pixelData->alpha, r = pixelData->red, g = pixelData->green, b = pixelData->blue };
        }

        internal void SetPixel(int x, int y, Color2 color)
        {
            var data = (PixelData*)(pBase + y * stride + x * sizeof(PixelData));
            data->alpha = color.a;
            data->red = color.r;
            data->green = color.g;
            data->blue = color.b;
        }

        internal void UnlockImage()
        {
            workingBitmap.UnlockBits(bitmapData);
            bitmapData = null;
            pBase = null;
        }

        internal Color2[,] GetImage()
        {
            var imagen = new Color2[height, width];

            LockImage();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    imagen[y, x] = GetPixel(x, y);
                }
            }
            UnlockImage();

            return imagen;
        }

        internal void SetImage(Color2[,] imagen)
        {
            LockImage();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    SetPixel(x, y, imagen[y, x]);
                }
            }
            UnlockImage();
        }
    }
}
