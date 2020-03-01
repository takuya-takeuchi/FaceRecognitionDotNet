using System;
using System.Collections.Generic;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;

namespace FaceRecognitionDotNet.Dlib.Python
{

    internal sealed class FaceRecognitionModelV1
    {

        #region Methods

        public static Matrix<double> ComputeFaceDescriptor(LossMetric net, Image img, FullObjectDetection face, int numJitters)
        {
            var faces = new[] { face };
            return ComputeFaceDescriptors(net, img, faces, numJitters).First();
        }

        public static IEnumerable<Matrix<double>> ComputeFaceDescriptors(LossMetric net, Image img, IEnumerable<FullObjectDetection> faces, int numJitters)
        {
            var batchImage = new[] { img };
            var batchFaces = new[] { faces };
            return BatchComputeFaceDescriptors(net, batchImage, batchFaces, numJitters).First();
        }

        public static IEnumerable<IEnumerable<Matrix<double>>> BatchComputeFaceDescriptors(LossMetric net,
                                                                                           IList<Image> batchImages,
                                                                                           IList<IEnumerable<FullObjectDetection>> batchFaces,
                                                                                           int numJitters)
        {
            if (batchImages.Count() != batchFaces.Count())
                throw new ArgumentException("The array of images and the array of array of locations must be of the same size");

            foreach (var faces in batchFaces)
                foreach (var f in faces)
                {
                    if (f.Parts != 68 && f.Parts != 5)
                        throw new ArgumentException("The full_object_detection must use the iBUG 300W 68 point face landmark style or dlib's 5 point style.");
                }

            var faceChipsArray = new List<Array<Matrix<RgbPixel>>>(batchImages.Count);
            var faceChips = new List<Matrix<RgbPixel>>();
            for (var i = 0; i < batchImages.Count; ++i)
            {
                var faces = batchFaces[i];
                var img = batchImages[i];

                var dets = new List<ChipDetails>(faces.Count());
                foreach (var f in faces)
                    dets.Add(DlibDotNet.Dlib.GetFaceChipDetails(f, 150, 0.25));

                var thisImageFaceChips = DlibDotNet.Dlib.ExtractImageChips<RgbPixel>(img.Matrix, dets);
                foreach (var chip in thisImageFaceChips)
                    faceChips.Add(chip);
                faceChipsArray.Add(thisImageFaceChips);

                foreach (var det in dets)
                    det.Dispose();
            }

            var faceDescriptors = new List<List<Matrix<double>>>();
            for (int i = 0, count = batchImages.Count; i < count; i++)
                faceDescriptors.Add(new List<Matrix<double>>());

            if (numJitters <= 1)
            {
                // extract descriptors and convert from float vectors to double vectors
                var descriptors = net.Operator(faceChips, 16);
                var index = 0;
                var list = descriptors.Select(matrix => matrix).ToArray();
                for (var i = 0; i < batchFaces.Count(); ++i)
                    for (var j = 0; j < batchFaces[i].Count(); ++j)
                        faceDescriptors[i].Add(DlibDotNet.Dlib.MatrixCast<double>(list[index++]));

                if (index != list.Length)
                    throw new ApplicationException();
            }
            else
            {
                // extract descriptors and convert from float vectors to double vectors
                var index = 0;
                for (var i = 0; i < batchFaces.Count(); ++i)
                    for (var j = 0; j < batchFaces[i].Count(); ++j)
                    {
                        var tmp = JitterImage(faceChips[index++], numJitters).ToArray();
                        using (var tmp2 = net.Operator(tmp, 16))
                        using (var mat = DlibDotNet.Dlib.Mat(tmp2))
                        {
                            var r = DlibDotNet.Dlib.Mean<double>(mat);
                            faceDescriptors[i].Add(r);
                        }

                        foreach (var matrix in tmp)
                            matrix.Dispose();
                    }

                if (index != faceChips.Count)
                    throw new ApplicationException();
            }

            if (faceChipsArray.Any())
            {
                foreach (var array in faceChipsArray)
                {
                    foreach (var faceChip in array)
                        faceChip.Dispose();
                    array.Dispose();
                }
            }

            return faceDescriptors;
        }

        #region Helpers

        private static readonly Rand Rand = new Rand();

        private static IEnumerable<Matrix<RgbPixel>> JitterImage(Matrix<RgbPixel> img, int numJitters)
        {
            var crops = new List<Matrix<RgbPixel>>();
            for (var i = 0; i < numJitters; ++i)
                crops.Add(DlibDotNet.Dlib.JitterImage(img, Rand));

            return crops;
        }

        #endregion

        #endregion

    }

}
