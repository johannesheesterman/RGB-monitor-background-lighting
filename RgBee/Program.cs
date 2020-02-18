using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Win32Interop.Methods;

namespace RgBee
{
	class Program
	{
        static (int ScreenX, int ScreenY) ScreenResolution;
        static Point topLeft;
        static Point bottomRight;
        static Rectangle rect;
        static Bitmap screenshot;


        static void Main(string[] args)
		{
            var sw = new Stopwatch();

            ScreenResolution = GetResolution();
            topLeft = new Point(0, ScreenResolution.ScreenY / 3);
            bottomRight = new Point(ScreenResolution.ScreenX, 2 * ScreenResolution.ScreenY / 3);
            rect = new Rectangle(topLeft, new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y));
            screenshot = new Bitmap(rect.Width, rect.Height);

            //var dcRef = new HandleRef(new object(), new IntPtr(0));
            //var dc = Win32Interop.Methods.User32.GetDC((IntPtr)dcRef);

            while (true)
            {


                //uint r = 0;
                //uint b = 0;
                //uint g = 0;

                //for (int i = 0; i < 100; i++)
                //{
                //    var pixel = Win32Interop.Methods.Gdi32.GetPixel(dc, 100, 100);

                //    r += pixel & 0xff;
                //    g += (pixel >> 8) & 0xff;
                //    b += (pixel >> 16) & 0xff;
                //    //var a = (pixel >> 24) & 0xff;
                //}


                //r /= 100;
                //g /= 100;
                //b /= 100;

                sw.Start();
                var averageRgb = CalculateAverageColor();             
                sw.Stop();
                Console.WriteLine($"{averageRgb.R} {averageRgb.G} {averageRgb.B} in {sw.ElapsedMilliseconds}ms.");
                sw.Reset();
                Thread.Sleep(1000);
            }
		}

        private static Bitmap TakeScreenshot()
        {
         
            Graphics gfx = Graphics.FromImage(screenshot); 
            try 
            {
                gfx.CopyFromScreen(topLeft, new Point(0, 0), rect.Size);
            } 
            catch (Exception) { 
                gfx.Clear(Color.Black); 
            }
            gfx.Dispose();
            return screenshot;
        }

        private static (long R, long G, long B) CalculateAverageColor()
		{
            var screenshot = TakeScreenshot();
            BitmapData srcData = screenshot.LockBits(
                new Rectangle(0, 0, screenshot.Width, screenshot.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            long[] totals = new long[] { 0, 0, 0 };

            int width = screenshot.Width;
            int height = screenshot.Height;
            int scale = 32;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y += scale)
                {
                    for (int x = 0; x < width; x += scale)
                    {
                        int index = y * stride + x * 4;
                        totals[0] += p[index];
                        totals[1] += p[index + 1];
                        totals[2] += p[index + 2];
                    }
                }
            }

            int w = width / scale;
            int h = height / scale;

            long avgB = totals[0] / (w * h);
            long avgG = totals[1] / (w * h);
            long avgR = totals[2] / (w * h);

            screenshot.UnlockBits(srcData);

            return (avgR, avgG, avgB);
        }

        private static (int ScreenX, int ScreenY) GetResolution()
        {
            ManagementObjectSearcher mydisplayResolution = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController");
            foreach (ManagementObject record in mydisplayResolution.Get())
            {
                return (Convert.ToInt32(record["CurrentHorizontalResolution"]), Convert.ToInt32(record["CurrentVerticalResolution"])); 
            }

            return (0, 0);
        }
	}
}

