using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to estimate human age from face image.
    /// </summary>
    public abstract class AgeEstimator : DisposableObject
    {

        #region Properties

        /// <summary>
        /// Gets the collection of age group this estimator returns in derived classes.
        /// </summary>
        public abstract AgeRange[] Groups
        {
            get;
        }

        #endregion

        #region Methods

        internal uint Predict(Image image, Location location)
        {
            return this.RawPredict(image.Matrix, location);
        }

        internal IDictionary<uint, float> PredictProbability(Image image, Location location)
        {
            return this.RawPredictProbability(image.Matrix, location);
        }

        /// <summary>
        /// Returns an index of age group of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An index of age group of face image correspond to specified location in specified image.</returns>
        protected abstract uint RawPredict(MatrixBase matrix, Location location);

        /// <summary>
        /// Returns probabilities of age group of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>Probabilities of age group of face image correspond to specified location in specified image.</returns>
        protected abstract IDictionary<uint, float> RawPredictProbability(MatrixBase matrix, Location location);

        #endregion

    }

}