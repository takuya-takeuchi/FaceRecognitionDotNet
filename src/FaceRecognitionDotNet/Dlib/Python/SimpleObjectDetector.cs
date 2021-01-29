using System;
using System.Collections.Generic;
using System.Linq;
using DlibDotNet;

namespace FaceRecognitionDotNet.Dlib.Python
{

    internal sealed class SimpleObjectDetector
    {

        #region Methods

        public static IEnumerable<Rectangle> RunDetectorWithUpscale1(FrontalFaceDetector detector,
                                                                     Image img,
                                                                     uint upsamplingAmount,
                                                                     double adjustThreshold,
                                                                     List<double> detectionConfidences,
                                                                     List<ulong> weightIndices)
        {
            var rectangles = new List<Rectangle>();

            if (img.Mode == Mode.Greyscale)
            {
                var greyscaleMatrix = img.Matrix as Matrix<byte>;
                if (upsamplingAmount == 0)
                {
                    detector.Operator(greyscaleMatrix, out var rectDetections, adjustThreshold);

                    var dets = rectDetections.ToArray();
                    SplitRectDetections(dets, rectangles, detectionConfidences, weightIndices);

                    foreach (var rectDetection in dets)
                        rectDetection.Dispose();
                }
                else
                {
                    using (var pyr = new PyramidDown(2))
                    {
                        Matrix<byte> temp = null;

                        try
                        {
                            DlibDotNet.Dlib.PyramidUp(greyscaleMatrix, pyr, out temp);

                            var levels = upsamplingAmount - 1;
                            while (levels > 0)
                            {
                                levels--;
                                DlibDotNet.Dlib.PyramidUp(temp);
                            }

                            detector.Operator(temp, out var rectDetections, adjustThreshold);

                            var dets = rectDetections.ToArray();
                            foreach (var t in dets)
                                t.Rect = pyr.RectDown(t.Rect, upsamplingAmount);

                            SplitRectDetections(dets, rectangles, detectionConfidences, weightIndices);

                            foreach (var rectDetection in dets)
                                rectDetection.Dispose();
                        }
                        finally
                        {
                            temp?.Dispose();
                        }
                    }
                }

                return rectangles;
            }
            else
            {
                var rgbMatrix = img.Matrix as Matrix<RgbPixel>;
                if (upsamplingAmount == 0)
                {
                    detector.Operator(rgbMatrix, out var rectDetections, adjustThreshold);

                    var dets = rectDetections.ToArray();
                    SplitRectDetections(dets, rectangles, detectionConfidences, weightIndices);

                    foreach (var rectDetection in dets)
                        rectDetection.Dispose();
                }
                else
                {
                    using (var pyr = new PyramidDown(2))
                    {
                        Matrix<RgbPixel> temp = null;

                        try
                        {
                            DlibDotNet.Dlib.PyramidUp(rgbMatrix, pyr, out temp);

                            var levels = upsamplingAmount - 1;
                            while (levels > 0)
                            {
                                levels--;
                                DlibDotNet.Dlib.PyramidUp(temp);
                            }

                            detector.Operator(temp, out var rectDetections, adjustThreshold);

                            var dets = rectDetections.ToArray();
                            foreach (var t in dets)
                                t.Rect = pyr.RectDown(t.Rect, upsamplingAmount);

                            SplitRectDetections(dets, rectangles, detectionConfidences, weightIndices);

                            foreach (var rectDetection in dets)
                                rectDetection.Dispose();
                        }
                        finally
                        {
                            temp?.Dispose();
                        }
                    }
                }

                return rectangles;
            }
        }

        public static IEnumerable<Tuple<Rectangle, double>> RunDetectorWithUpscale2(FrontalFaceDetector detector,
                                                                                    Image image,
                                                                                    uint upsamplingAmount)
        {
            if (detector == null)
                throw new ArgumentNullException(nameof(detector));
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            detector.ThrowIfDisposed();
            image.ThrowIfDisposed();

            var detectionConfidences = new List<double>();
            var weightIndices = new List<ulong>();
            const double adjustThreshold = 0.0;

            var rects = RunDetectorWithUpscale1(detector,
                                                image,
                                                upsamplingAmount,
                                                adjustThreshold,
                                                detectionConfidences,
                                                weightIndices).ToArray();

            
            var index = 0;
            foreach(var rect in rects)
                yield return new Tuple<Rectangle, double>(rect, detectionConfidences[index++]);
        }

        #region Helpers

        private static void SplitRectDetections(RectDetection[] rectDetections,
                                                List<Rectangle> rectangles,
                                                List<double> detectionConfidences,
                                                List<ulong> weightIndices)
        {
            rectangles.Clear();
            detectionConfidences.Clear();
            weightIndices.Clear();

            foreach (var rectDetection in rectDetections)
            {
                rectangles.Add(rectDetection.Rect);
                detectionConfidences.Add(rectDetection.DetectionConfidence);
                weightIndices.Add(rectDetection.WeightIndex);
            }
        }

        #endregion

        #endregion

    }

}
