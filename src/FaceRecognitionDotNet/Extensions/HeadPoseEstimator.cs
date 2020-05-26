using System;
using System.Collections.Generic;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to estimate human head pose from face landmark.
    /// </summary>
    public abstract class HeadPoseEstimator : DisposableObject
    {

        #region Methods

        internal HeadPose Predict(IDictionary<FacePart, IEnumerable<FacePoint>> landmark)
        {
            return this.RawPredict(landmark);
        }

        /// <summary>
        /// Returns a head pose estimated from face parts locations.
        /// </summary>
        /// <param name="landmark">The dictionary of face parts locations (eyes, nose, etc).</param>
        /// <returns>A head pose estimated from face parts locations.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="landmark"/> is null.</exception>
        protected abstract HeadPose RawPredict(IDictionary<FacePart, IEnumerable<FacePoint>> landmark);

        #endregion

    }

}