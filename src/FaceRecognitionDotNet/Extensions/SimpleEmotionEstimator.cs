using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The age estimator which was trained by Corrective re-annotation of FER - CK+ - KDEF dataset. This class cannot be inherited.
    /// </summary>
    public sealed class SimpleEmotionEstimator : EmotionEstimator
    {

        #region Fields

        private readonly LossMulticlassLog _Network;

        //private readonly ShapePredictor _PosePredictor68Point;

        private readonly string[] _Labels;

        private const int Size = 227;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleEmotionEstimator"/> class with the model file path that this estimator uses.
        /// </summary>
        /// <param name="modelPath">The model file path that this estimator uses.</param>
        /// <exception cref="FileNotFoundException">The <paramref name="modelPath"/> file is not found.</exception>
        public SimpleEmotionEstimator(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException(modelPath);

            var ret = NativeMethods.LossMulticlassLog_emotion_train_type6_create();
            var networkId = LossMulticlassLogRegistry.GetId(ret);
            if (LossMulticlassLogRegistry.Contains(networkId))
                NativeMethods.LossMulticlassLog_emotion_train_type_delete(ret);
            else
                LossMulticlassLogRegistry.Add(ret);

            this._Network = LossMulticlassLog.Deserialize(modelPath, networkId);
            NativeMethods.LossMulticlassLog_emotion_train_type_eval(networkId, ret);

            this._Labels = this._Network.GetLabels();
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SimpleEmotionEstimator"/> class with the model file path that this estimator uses.
        ///// </summary>
        ///// <param name="modelPath">The model file path that this estimator uses.</param>
        ///// <param name="predictor68PointModel">The model file path to get 68 points face landmarks.</param>
        ///// <exception cref="FileNotFoundException">The <paramref name="modelPath"/> or <paramref name="predictor68PointModel"/> file is not found.</exception>
        //public SimpleEmotionEstimator(string modelPath, string predictor68PointModel)
        //{
        //    if (!File.Exists(modelPath))
        //        throw new FileNotFoundException(modelPath);
        //    if (!File.Exists(predictor68PointModel))
        //        throw new FileNotFoundException(predictor68PointModel);

        //    var ret = NativeMethods.LossMulticlassLog_emotion_train_type_create();
        //    var networkId = LossMulticlassLogRegistry.GetId(ret);
        //    if (LossMulticlassLogRegistry.Contains(networkId))
        //        NativeMethods.LossMulticlassLog_emotion_train_type_delete(ret);
        //    else
        //        LossMulticlassLogRegistry.Add(ret);

        //    this._Network = LossMulticlassLog.Deserialize(modelPath, networkId);
        //    NativeMethods.LossMulticlassLog_emotion_train_type_eval(networkId, ret);

        //    this._Labels = this._Network.GetLabels();

        //    this._PosePredictor68Point = ShapePredictor.Deserialize(predictor68PointModel);
        //}

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of emotion label this estimator returns in derived classes.
        /// </summary>
        public override ReadOnlyCollection<string> Labels
        {
            get
            {
                return new ReadOnlyCollection<string>(this._Labels);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns an emotion of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An emotion of face image correspond to specified location in specified image.</returns>
        protected override string RawPredict(MatrixBase matrix, Location location)
        {
            //if (!(matrix is Matrix<RgbPixel> mat))
            //    throw new ArgumentException();

            //var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);

            //using (var ret = this._PosePredictor68Point.Detect(matrix, new Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom)))
            //{
            //    var vector = GetFeatureVector(ret);
            //    using (var input = new Matrix<double>(vector.ToArray(), vector.Length, 1))
            //    using (var results = this._Network.Operator(new[] { input }, 1))
            //        return this._Labels[results[0]];
            //}
            var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);
            var dPoint = new[]
            {
                new DPoint(rect.Left, rect.Top),
                new DPoint(rect.Right, rect.Top),
                new DPoint(rect.Left, rect.Bottom),
                new DPoint(rect.Right, rect.Bottom),
            };
            switch (matrix)
            {
                case Matrix<RgbPixel> mat:
                {
                    using (var grayscale = new Matrix<byte>(Size, Size))
                    using (var face = DlibDotNet.Dlib.ExtractImage4Points(mat, dPoint, Size, Size))
                    {
                        DlibDotNet.Dlib.AssignImage(face, grayscale);
                        using (var results = this._Network.Operator(grayscale, 1))
                            return this._Labels[results[0]];
                    }
                }
                case Matrix<byte> grayscale:
                {
                    using (var face = DlibDotNet.Dlib.ExtractImage4Points(grayscale, dPoint, Size, Size))
                    using (var results = this._Network.Operator(face, 1))
                        return this._Labels[results[0]];
                }
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Returns probabilities of emotion of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>Probabilities of emotion of face image correspond to specified location in specified image.</returns>
        protected override IDictionary<string, float> RawPredictProbability(MatrixBase matrix, Location location)
        {
            //if (!(matrix is Matrix<RgbPixel> mat))
            //    throw new ArgumentException();

            //var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);

            //using (var ret = this._PosePredictor68Point.Detect(matrix, new Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom)))
            //{
            //    var vector = GetFeatureVector(ret);
            //    using (var input = new Matrix<double>(vector.ToArray(), vector.Length, 1))
            //    {
            //        var results = this._Network.Probability(input, 1).ToArray();

            //        var dictionary = new Dictionary<string, float>();
            //        for (var index = 0; index < this._Labels.Length; index++)
            //            dictionary.Add(this._Labels[index], results[0][index]);

            //        return dictionary;
            //    }
            //}
            var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);
            var dPoint = new[]
            {
                new DPoint(rect.Left, rect.Top),
                new DPoint(rect.Right, rect.Top),
                new DPoint(rect.Left, rect.Bottom),
                new DPoint(rect.Right, rect.Bottom),
            };
            switch (matrix)
            {
                case Matrix<RgbPixel> mat:
                {
                    using (var grayscale = new Matrix<byte>(Size, Size))
                    using (var face = DlibDotNet.Dlib.ExtractImage4Points(mat, dPoint, Size, Size))
                    {
                        DlibDotNet.Dlib.AssignImage(face, grayscale);
                        var results = this._Network.Probability(grayscale, 1).ToArray();

                        var dictionary = new Dictionary<string, float>();
                        for (var index = 0; index < this._Labels.Length; index++)
                            dictionary.Add(this._Labels[index], results[0][index]);

                        return dictionary;
                    }
                }
                case Matrix<byte> grayscale:
                {
                    using (var face = DlibDotNet.Dlib.ExtractImage4Points(grayscale, dPoint, Size, Size))
                    {
                        var results = this._Network.Probability(face, 1).ToArray();

                        var dictionary = new Dictionary<string, float>();
                        for (var index = 0; index < this._Labels.Length; index++)
                            dictionary.Add(this._Labels[index], results[0][index]);

                        return dictionary;
                    }
                }
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            this._Network?.Dispose();
        }

        #region Helpers

        //private static double GetEuclideanDistance(Point p1, Point p2)
        //{
        //    return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        //}

        //private static Point CenterOf(Point p1, Point p2)
        //{
        //    var x = (p1.X + p2.X) / 2;
        //    var y = (p1.Y + p2.Y) / 2;
        //    return new Point(x, y);
        //}

        //private static double[] GetFeatureVector(FullObjectDetection landmark)
        //{
        //    var landmarkPoints = Enumerable.Range(0, (int)landmark.Parts)
        //                                   .Select(index => new FacePoint(new Point(landmark.GetPart((uint)index)), index)).ToArray();

        //    // For a definition of each point index, see https://cdn-images-1.medium.com/max/1600/1*AbEg31EgkbXSQehuNJBlWg.png
        //    // convert 68 points to 18 points
        //    // left eye
        //    var leftEyeTop1 = landmarkPoints.FirstOrDefault(point => point.Index == 37).Point;
        //    var leftEyeTop2 = landmarkPoints.FirstOrDefault(point => point.Index == 38).Point;
        //    var leftEyeBottom1 = landmarkPoints.FirstOrDefault(point => point.Index == 41).Point;
        //    var leftEyeBottom2 = landmarkPoints.FirstOrDefault(point => point.Index == 40).Point;
        //    var f1 = CenterOf(leftEyeTop1, leftEyeTop2);
        //    var f2 = CenterOf(leftEyeBottom1, leftEyeBottom2);
        //    var f3 = landmarkPoints.FirstOrDefault(point => point.Index == 36).Point;
        //    var f4 = landmarkPoints.FirstOrDefault(point => point.Index == 39).Point;

        //    // left eye
        //    var rightEyeTop1 = landmarkPoints.FirstOrDefault(point => point.Index == 43).Point;
        //    var rightEyeTop2 = landmarkPoints.FirstOrDefault(point => point.Index == 44).Point;
        //    var rightEyeBottom1 = landmarkPoints.FirstOrDefault(point => point.Index == 46).Point;
        //    var rightEyeBottom2 = landmarkPoints.FirstOrDefault(point => point.Index == 47).Point;
        //    var f5 = CenterOf(rightEyeTop1, rightEyeTop2);
        //    var f6 = CenterOf(rightEyeBottom1, rightEyeBottom2);
        //    var f7 = landmarkPoints.FirstOrDefault(point => point.Index == 42).Point;
        //    var f8 = landmarkPoints.FirstOrDefault(point => point.Index == 45).Point;

        //    // nose
        //    var f9 = landmarkPoints.FirstOrDefault(point => point.Index == 30).Point;

        //    // left eyebrow
        //    var f10 = landmarkPoints.FirstOrDefault(point => point.Index == 17).Point;
        //    var f11 = landmarkPoints.FirstOrDefault(point => point.Index == 21).Point;
        //    var f12 = landmarkPoints.FirstOrDefault(point => point.Index == 19).Point;

        //    // right eyebrow
        //    var f13 = landmarkPoints.FirstOrDefault(point => point.Index == 22).Point;
        //    var f14 = landmarkPoints.FirstOrDefault(point => point.Index == 26).Point;
        //    var f15 = landmarkPoints.FirstOrDefault(point => point.Index == 24).Point;

        //    // lip
        //    var f16 = landmarkPoints.FirstOrDefault(point => point.Index == 48).Point;
        //    var f17 = landmarkPoints.FirstOrDefault(point => point.Index == 54).Point;
        //    var top = landmarkPoints.FirstOrDefault(point => point.Index == 62).Point;
        //    var bottom = landmarkPoints.FirstOrDefault(point => point.Index == 66).Point;
        //    var f18 = CenterOf(top, bottom);

        //    // Left eye Height V1 = F1-F2
        //    var v1 = GetEuclideanDistance(f1, f2);

        //    // Left eye Width V2 = F4 - F3
        //    var v2 = GetEuclideanDistance(f4, f3);

        //    // Right eye Height V3 = F5 - F6
        //    var v3 = GetEuclideanDistance(f5, f6);

        //    // Right eye Width V4 = F8- F7
        //    var v4 = GetEuclideanDistance(f8, f7);

        //    // Left eyebrow width V5 = F11 - F10
        //    var v5 = GetEuclideanDistance(f11, f10);

        //    // Right eyebrow width V6 = F14 - F13
        //    var v6 = GetEuclideanDistance(f14, f13);

        //    // Lip width V7 = F17 - F16
        //    var v7 = GetEuclideanDistance(f17, f16);

        //    // Left eye upper corner and left eyebrow center dist. V8 = F12 - F1
        //    var v8 = GetEuclideanDistance(f12, f11);

        //    // Right eye upper corner and right eyebrow center dist. V9 = F15 - F5
        //    var v9 = GetEuclideanDistance(f15, f5);

        //    // Nose centre and lips centre dist. V10 = F9 - F18
        //    var v10 = GetEuclideanDistance(f9, f18);

        //    // Left eye lower corner and lips left corner dist. V11 = F2 - F16
        //    var v11 = GetEuclideanDistance(f2, f16);

        //    // Right eye lower corner and lips right corner dist. V12 = F6 - F17
        //    var v12 = GetEuclideanDistance(f6, f17);

        //    return new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12 };
        //}

        #endregion

        #endregion

    }

}