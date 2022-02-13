using System.Collections.Generic;
using System.Collections.ObjectModel;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to estimate emotion from face image.
    /// </summary>
    public abstract class EmotionEstimator : DisposableObject
    {

        #region Properties

        /// <summary>
        /// Gets the collection of emotion label this estimator returns in derived classes.
        /// </summary>
        public abstract ReadOnlyCollection<string> Labels
        {
            get;
        }

        #endregion

        #region Methods

        internal string Predict(Image image, Location location)
        {
            return this.RawPredict(image.Matrix, location);
        }

        internal IDictionary<string, float> PredictProbability(Image image, Location location)
        {
            return this.RawPredictProbability(image.Matrix, location);
        }

        /// <summary>
        /// Returns an emotion of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An emotion of face image correspond to specified location in specified image.</returns>
        protected abstract string RawPredict(MatrixBase matrix, Location location);

        /// <summary>
        /// Returns probabilities of emotion of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>Probabilities of emotion of face image correspond to specified location in specified image.</returns>
        protected abstract IDictionary<string, float> RawPredictProbability(MatrixBase matrix, Location location);

        #endregion

    }

}