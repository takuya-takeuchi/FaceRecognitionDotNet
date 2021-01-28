using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using FaceRecognitionDotNet;

namespace RgbBgr
{

    internal class Program
    {

        #region Fields

        private const string File = "Lenna.bmp";

        #endregion

        #region Methods

        private static void Main()
        {
            byte[] rgb = null;
            byte[] bgr = null;

            using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(File))
            {
                BitmapData data = null;

                try
                {
                    var width = bitmap.Width;
                    var height = bitmap.Height;
                    var format = bitmap.PixelFormat;

                    var rect = new Rectangle(0, 0, width, height);
                    data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, format);

                    rgb = new byte[width * height * 3];
                    bgr = new byte[width * height * 3];

                    // Windows Bitmap has BGR pixel
                    unsafe
                    {
                        var stride = data.Stride;
                        var ptr = (byte*)data.Scan0;
                        fixed (byte* pRgb = &rgb[0])
                        fixed (byte* pBgr = &bgr[0])
                        {
                            for (var y = 0; y < height; y++)
                                for (var x = 0; x < width; x++)
                                {
                                    pBgr[y * width * 3 + x * 3 + 0] = ptr[y * stride + x * 3 + 0];
                                    pBgr[y * width * 3 + x * 3 + 1] = ptr[y * stride + x * 3 + 1];
                                    pBgr[y * width * 3 + x * 3 + 2] = ptr[y * stride + x * 3 + 2];

                                    pRgb[y * width * 3 + x * 3 + 0] = ptr[y * stride + x * 3 + 2];
                                    pRgb[y * width * 3 + x * 3 + 1] = ptr[y * stride + x * 3 + 1];
                                    pRgb[y * width * 3 + x * 3 + 2] = ptr[y * stride + x * 3 + 0];
                                }
                        }
                    }

                    using (var fr = FaceRecognition.Create("models"))
                    {
                        using (var fileImage = FaceRecognition.LoadImageFile(File))
                        using (var rgbImage = FaceRecognition.LoadImage(rgb, height, width, width * 3, Mode.Rgb))
                        using (var bgrImage = FaceRecognition.LoadImage(bgr, height, width, width * 3, Mode.Rgb))
                        {
                            var fileDetect = fr.FaceLocations(fileImage, 1, Model.Hog).ToArray().FirstOrDefault();
                            var rgbDetect = fr.FaceLocations(rgbImage, 1, Model.Hog).ToArray().FirstOrDefault();
                            var bgrDetect = fr.FaceLocations(bgrImage, 1, Model.Hog).ToArray().FirstOrDefault();

                            Console.WriteLine();
                            Console.WriteLine("FaceLocations (Hog)");
                            Console.WriteLine($"\t File [LTRB]: {GetFaceLocationResult(fileDetect)}");
                            Console.WriteLine($"\t  RGB [LTRB]: {GetFaceLocationResult(rgbDetect)}");
                            Console.WriteLine($"\t  BGR [LTRB]: {GetFaceLocationResult(bgrDetect)}");

                            var fileEncoding = fr.FaceEncodings(rgbImage, new[] { fileDetect }, 1, PredictorModel.Small).ToArray().FirstOrDefault();
                            var rgbEncoding = fr.FaceEncodings(rgbImage, new[] { fileDetect }, 1, PredictorModel.Small).ToArray().FirstOrDefault();
                            var bgrEncoding = fr.FaceEncodings(bgrImage, new[] { fileDetect }, 1, PredictorModel.Small).ToArray().FirstOrDefault();

                            Console.WriteLine();
                            Console.WriteLine("FaceEncodings by File Location (Hog)");
                            Console.WriteLine($"\t vs  RGB [Distance]: {GetDistance(fileEncoding, rgbEncoding)}");
                            Console.WriteLine($"\t vs  BGR [Distance]: {GetDistance(fileEncoding, bgrEncoding)}");
                            Console.WriteLine("FaceEncodings by RGB Location (Hog)");
                            Console.WriteLine($"\t vs File [Distance]: {GetDistance(rgbEncoding, fileEncoding)}");
                            Console.WriteLine($"\t vs  BGR [Distance]: {GetDistance(rgbEncoding, bgrEncoding)}");
                            Console.WriteLine("FaceEncodings by BGR Location (Hog)");
                            Console.WriteLine($"\t vs File [Distance]: {GetDistance(bgrEncoding, fileEncoding)}");
                            Console.WriteLine($"\t vs  RGB [Distance]: {GetDistance(bgrEncoding, rgbEncoding)}");

                            fileDetect = fr.FaceLocations(fileImage, 1, Model.Cnn).ToArray().FirstOrDefault();
                            rgbDetect = fr.FaceLocations(rgbImage, 1, Model.Cnn).ToArray().FirstOrDefault();
                            bgrDetect = fr.FaceLocations(bgrImage, 1, Model.Cnn).ToArray().FirstOrDefault();

                            Console.WriteLine();
                            Console.WriteLine("FaceLocations (Cnn)");
                            Console.WriteLine($"\t File [LTRB]: {GetFaceLocationResult(fileDetect)}");
                            Console.WriteLine($"\t  RGB [LTRB]: {GetFaceLocationResult(rgbDetect)}");
                            Console.WriteLine($"\t  BGR [LTRB]: {GetFaceLocationResult(bgrDetect)}");

                            fileEncoding = fr.FaceEncodings(rgbImage, new[] { fileDetect }, 1, PredictorModel.Small).ToArray().FirstOrDefault();
                            rgbEncoding = fr.FaceEncodings(rgbImage, new[] { fileDetect }, 1, PredictorModel.Small).ToArray().FirstOrDefault();
                            bgrEncoding = fr.FaceEncodings(bgrImage, new[] { fileDetect }, 1, PredictorModel.Small).ToArray().FirstOrDefault();

                            Console.WriteLine();
                            Console.WriteLine("FaceEncodings by File Location (Cnn)");
                            Console.WriteLine($"\t vs  RGB [Distance]: {GetDistance(fileEncoding, rgbEncoding)}");
                            Console.WriteLine($"\t vs  BGR [Distance]: {GetDistance(fileEncoding, bgrEncoding)}");
                            Console.WriteLine("FaceEncodings by RGB Location (Cnn)");
                            Console.WriteLine($"\t vs File [Distance]: {GetDistance(rgbEncoding, fileEncoding)}");
                            Console.WriteLine($"\t vs  BGR [Distance]: {GetDistance(rgbEncoding, bgrEncoding)}");
                            Console.WriteLine("FaceEncodings by BGR Location (Cnn)");
                            Console.WriteLine($"\t vs File [Distance]: {GetDistance(bgrEncoding, fileEncoding)}");
                            Console.WriteLine($"\t vs  RGB [Distance]: {GetDistance(bgrEncoding, rgbEncoding)}");
                        }
                    }
                }
                finally
                {
                    if (data != null)
                        bitmap.UnlockBits(data);
                }
            }
        }

        private static string GetDistance(FaceEncoding a, FaceEncoding b)
        {
            return a == null || b == null ? "N/A" : FaceRecognition.FaceDistance(a, b).ToString();
        }

        private static string GetFaceLocationResult(Location location)
        {
            return location == null ? "missing" : $"{location.Left}, {location.Top}, {location.Right}, {location.Bottom}";
        }

        #endregion

    }

}
