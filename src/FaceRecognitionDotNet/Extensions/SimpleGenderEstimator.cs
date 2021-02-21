using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The age estimator which was trained by UTKFace dataset. This class cannot be inherited.
    /// </summary>
    public sealed class SimpleGenderEstimator : GenderEstimator
    {

        #region Fields

        private readonly LossMulticlassLog _Network;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleGenderEstimator"/> class with the model file path that this estimator uses.
        /// </summary>
        /// <param name="modelPath">The model file path that this estimator uses.</param>
        /// <exception cref="FileNotFoundException">The model file is not found.</exception>
        public SimpleGenderEstimator(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException(modelPath);

            var ret = NativeMethods.LossMulticlassLog_gender_train_type_create();
            var networkId = LossMulticlassLogRegistry.GetId(ret);
            if (LossMulticlassLogRegistry.Contains(networkId))
                NativeMethods.LossMulticlassLog_gender_train_type_delete(ret);
            else
                LossMulticlassLogRegistry.Add(ret);

            this._Network = LossMulticlassLog.Deserialize(modelPath, networkId);
            NativeMethods.LossMulticlassLog_gender_train_type_eval(this._Network.NativePtr);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of gender label this estimator returns in derived classes.
        /// </summary>
        public override Gender[] Labels
        {
            get
            {
                return new[]
                {
                    Gender.Male,
                    Gender.Female
                };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns an gender of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An gender of face image correspond to specified location in specified image.</returns>
        protected override Gender RawPredict(MatrixBase matrix, Location location)
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
                return results[0] == 0 ? Gender.Male : Gender.Female;
        }

        /// <summary>
        /// Returns probabilities of gender of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>Probabilities of gender of face image correspond to specified location in specified image.</returns>
        protected override IDictionary<Gender, float> RawPredictProbability(MatrixBase matrix, Location location)
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
                return new Dictionary<Gender, float>
                {
                    { Gender.Male,   results[0][0] },
                    { Gender.Female, results[0][1] }
                };
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