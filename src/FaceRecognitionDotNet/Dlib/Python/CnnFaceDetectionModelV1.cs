using System;
using System.Collections.Generic;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;

namespace FaceRecognitionDotNet.Dlib.Python
{

    internal sealed class CnnFaceDetectionModelV1
    {

        #region Methods

        public static IEnumerable<MModRect> Detect(LossMmod net, Image image, int upsampleNumTimes)
        {
            using (var pyr = new PyramidDown(2))
            {
                var rects = new List<MModRect>();

                // Copy the data into dlib based objects
                using (var matrix = new Matrix<RgbPixel>())
                {
                    var type = image.Mode;
                    switch (type)
                    {
                        case Mode.Greyscale:
                        case Mode.Rgb:
                            DlibDotNet.Dlib.AssignImage(image.Matrix, matrix);
                            break;
                        default:
                            throw new NotSupportedException("Unsupported image type, must be 8bit gray or RGB image.");
                    }

                    // Upsampling the image will allow us to detect smaller faces but will cause the
                    // program to use more RAM and run longer.
                    var levels = upsampleNumTimes;
                    while (levels > 0)
                    {
                        levels--;
                        DlibDotNet.Dlib.PyramidUp<PyramidDown>(matrix, 2);
                    }

                    var dets = net.Operator(matrix);

                    // Scale the detection locations back to the original image size
                    // if the image was upscaled.
                    foreach (var d in dets.First())
                    {
                        var drect = pyr.RectDown(new DRectangle(d.Rect), (uint)upsampleNumTimes);
                        d.Rect = new Rectangle((int)drect.Left, (int)drect.Top, (int)drect.Right, (int)drect.Bottom);
                        rects.Add(d);
                    }

                    return rects;
                }
            }
        }

        public static IEnumerable<IEnumerable<MModRect>> DetectMulti(LossMmod net, IEnumerable<Image> images, int upsampleNumTimes, int batchSize = 128)
        {
            var destImages = new List<Matrix<RgbPixel>>();
            var allRects = new List<IEnumerable<MModRect>>();

            try
            {
                using (var pyr = new PyramidDown(2))
                {
                    // Copy the data into dlib based objects
                    foreach (var image in images)
                    {
                        var matrix = new Matrix<RgbPixel>();
                        var type = image.Mode;
                        switch (type)
                        {
                            case Mode.Greyscale:
                            case Mode.Rgb:
                                DlibDotNet.Dlib.AssignImage(image.Matrix, matrix);
                                break;
                            default:
                                throw new NotSupportedException("Unsupported image type, must be 8bit gray or RGB image.");
                        }

                        for (var i = 0; i < upsampleNumTimes; i++)
                            DlibDotNet.Dlib.PyramidUp(matrix);

                        destImages.Add(matrix);
                    }

                    for (var i = 1; i < destImages.Count; i++)
                        if (destImages[i - 1].Columns != destImages[i].Columns || destImages[i - 1].Rows != destImages[i].Rows)
                            throw new ArgumentException("Images in list must all have the same dimensions.");

                    var dets = net.Operator(destImages, (ulong)batchSize);
                    foreach (var det in dets)
                    {
                        var rects = new List<MModRect>();
                        foreach (var d in det)
                        {
                            var drect = pyr.RectDown(new DRectangle(d.Rect), (uint)upsampleNumTimes);
                            d.Rect = new Rectangle((int)drect.Left, (int)drect.Top, (int)drect.Right, (int)drect.Bottom);
                            rects.Add(d);
                        }

                        allRects.Add(rects);
                    }
                }
            }
            finally
            {
                foreach (var matrix in destImages)
                    matrix.Dispose();
            }

            return allRects;
        }

        #endregion

    }

}
