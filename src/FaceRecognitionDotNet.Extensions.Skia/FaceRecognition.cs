using System;
using System.Runtime.InteropServices;

using DlibDotNet;
using SkiaSharp;

namespace FaceRecognitionDotNet.Extensions.Skia
{

    public sealed class FaceRecognition
    {

        #region Methods

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified existing bitmap image.
        /// </summary>
        /// <param name="bitmap">The <see cref="SKBitmap"/> from which to create the new <see cref="Image"/>.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified <see cref="SKColorType"/> is not supported.</exception>
        public static Image LoadImage(SKBitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var colorType = bitmap.ColorType;

            Mode mode;
            int srcChannel;
            int dstChannel;
            switch (colorType)
            {
                case SKColorType.Rgba8888:
                    mode = Mode.Rgb;
                    srcChannel = 4;
                    dstChannel = 3;
                    break;
                case SKColorType.Bgra8888:
                    mode = Mode.Rgb;
                    srcChannel = 4;
                    dstChannel = 3;
                    break;
                case SKColorType.Gray8:
                    mode = Mode.Greyscale;
                    srcChannel = 1;
                    dstChannel = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(bitmap)}", $"The specified {nameof(SKColorType)} is not supported.");
            }
            
            unsafe
            {
                var array = new byte[width * height * dstChannel];
                fixed (byte* pArray = &array[0])
                {

                    switch (srcChannel)
                    {
                        case 1:
                            {
                                var src = bitmap.GetPixels();
                                var stride = bitmap.RowBytes;

                                for (var h = 0; h < height; h++)
                                    Marshal.Copy(IntPtr.Add(src, h * stride), array, h * width, width * dstChannel);
                            }
                            break;
                        case 3:
                        case 4:
                            {
                                if (colorType == SKColorType.Rgba8888)
                                {
                                    var src = (byte*)bitmap.GetPixels();
                                    var stride = bitmap.RowBytes;

                                    for (var h = 0; h < height; h++)
                                    {
                                        var srcOffset = h * stride;
                                        var dstOffset = h * width * dstChannel;

                                        for (var w = 0; w < width; w++)
                                        {
                                            pArray[dstOffset + w * dstChannel + 0] = src[srcOffset + w * srcChannel + 0];
                                            pArray[dstOffset + w * dstChannel + 1] = src[srcOffset + w * srcChannel + 1];
                                            pArray[dstOffset + w * dstChannel + 2] = src[srcOffset + w * srcChannel + 2];
                                        }
                                    }
                                }
                                else
                                {
                                    var src = (byte*)bitmap.GetPixels();
                                    var stride = bitmap.RowBytes;

                                    for (var h = 0; h < height; h++)
                                    {
                                        var srcOffset = h * stride;
                                        var dstOffset = h * width * dstChannel;

                                        for (var w = 0; w < width; w++)
                                        {
                                            // BGR order to RGB order
                                            pArray[dstOffset + w * dstChannel + 0] = src[srcOffset + w * srcChannel + 2];
                                            pArray[dstOffset + w * dstChannel + 1] = src[srcOffset + w * srcChannel + 1];
                                            pArray[dstOffset + w * dstChannel + 2] = src[srcOffset + w * srcChannel + 0];
                                        }
                                    }
                                }
                            }
                            break;
                    }

                    var ptr = (IntPtr)pArray;
                    switch (mode)
                    {
                        case Mode.Rgb:
                            return new Image(new Matrix<RgbPixel>(ptr, height, width, width * 3), Mode.Rgb);
                        case Mode.Greyscale:
                            return new Image(new Matrix<byte>(ptr, height, width, width), Mode.Greyscale);
                    }
                }
            }

            return null;
        }

        #endregion

    }

}
