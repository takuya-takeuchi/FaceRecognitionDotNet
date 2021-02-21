using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The age estimator which was trained by Adience dataset. This class cannot be inherited.
    /// </summary>
    public sealed class SimpleAgeEstimator : AgeEstimator
    {

        #region Fields

        private readonly LossMulticlassLog _Network;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAgeEstimator"/> class with the model file path that this estimator uses.
        /// </summary>
        /// <param name="modelPath">The model file path that this estimator uses.</param>
        /// <exception cref="FileNotFoundException">The model file is not found.</exception>
        public SimpleAgeEstimator(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException(modelPath);

            var ret = NativeMethods.LossMulticlassLog_age_train_type_create();
            var networkId = LossMulticlassLogRegistry.GetId(ret);
            if (LossMulticlassLogRegistry.Contains(networkId))
                NativeMethods.LossMulticlassLog_age_train_type_delete(ret);
            else
                LossMulticlassLogRegistry.Add(ret);

            this._Network = LossMulticlassLog.Deserialize(modelPath, networkId);
            NativeMethods.LossMulticlassLog_age_train_type_eval(this._Network.NativePtr);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of age group this estimator returns in derived classes.
        /// </summary>
        public override AgeRange[] Groups
        {
            get
            {
                return new[]
                {
                    new AgeRange(0,2),
                    new AgeRange(4,6),
                    new AgeRange(8,13),
                    new AgeRange(15,20),
                    new AgeRange(25,32),
                    new AgeRange(38,43),
                    new AgeRange(48,53),
                    new AgeRange(60,100)
                };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns an index of age group of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An index of age group of face image correspond to specified location in specified image.</returns>
        protected override uint RawPredict(MatrixBase matrix, Location location)
        {
            if (!(matrix is Matrix<RgbPixel> mat))
                throw new ArgumentException();

            var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);
            var dPoint = new[]
            {
                new DPoint(rect.Left, rect.Top),
                new DPoint(rect.Right, rect.Top),
                new DPoint(rect.Left, rect.Bottom),
                new DPoint(rect.Right, rect.Bottom),
            };
            using (var img = DlibDotNet.Dlib.ExtractImage4Points(mat, dPoint, 227, 227))
            using (var results = this._Network.Operator(new[] { img }, 1))
                return results[0];
        }

        /// <summary>
        /// Returns probabilities of age group of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>Probabilities of age group of face image correspond to specified location in specified image.</returns>
        protected override IDictionary<uint, float> RawPredictProbability(MatrixBase matrix, Location location)
        {
            if (!(matrix is Matrix<RgbPixel> mat))
                throw new ArgumentException();

            var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);
            var dPoint = new[]
            {
                new DPoint(rect.Left, rect.Top),
                new DPoint(rect.Right, rect.Top),
                new DPoint(rect.Left, rect.Bottom),
                new DPoint(rect.Right, rect.Bottom),
            };
            using (var img = DlibDotNet.Dlib.ExtractImage4Points(mat, dPoint, 227, 227))
            {
                var results = this._Network.Probability(img, 1).ToArray();
                var predict = results[0];
                return predict.Select((n, index) => new { index, n }).ToDictionary(n => (uint)n.index, n => n.n);
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

        #endregion

    }

}