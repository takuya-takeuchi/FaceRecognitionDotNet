using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to estimate human gender from face image.
    /// </summary>
    public abstract class GenderEstimator : DisposableObject
    {

        #region Properties

        /// <summary>
        /// Gets the collection of gender label this estimator returns in derived classes.
        /// </summary>
        public abstract Gender[] Labels
        {
            get;
        }

        #endregion

        #region Methods

        internal Gender Predict(Image image, Location location)
        {
            return this.RawPredict(image.Matrix, location);
        }

        internal IDictionary<Gender, float> PredictProbability(Image image, Location location)
        {
            return this.RawPredictProbability(image.Matrix, location);
        }

        /// <summary>
        /// Returns an gender of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An gender of face image correspond to specified location in specified image.</returns>
        protected abstract Gender RawPredict(MatrixBase matrix, Location location);

        /// <summary>
        /// Returns probabilities of gender of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>Probabilities of gender of face image correspond to specified location in specified image.</returns>
        protected abstract IDictionary<Gender, float> RawPredictProbability(MatrixBase matrix, Location location);

        #endregion

    }

}