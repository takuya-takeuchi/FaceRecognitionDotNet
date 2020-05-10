using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to detect face parts locations from face image.
    /// </summary>
    public abstract class FaceLandmarkDetector : DisposableObject
    {

        #region Methods

        internal FullObjectDetection Detect(Image image, Location location)
        {
            return this.RawDetect(image.Matrix, location);
        }

        internal IEnumerable<Dictionary<FacePart, IEnumerable<FacePoint>>> GetLandmarks(IEnumerable<FacePoint[]> landmarkTuples)
        {
            return this.RawGetLandmarks(landmarkTuples);
        }

        /// <summary>
        /// Returns an object contains information of face parts corresponds to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An object contains information of face parts.</returns>
        protected abstract FullObjectDetection RawDetect(MatrixBase matrix, Location location);

        /// <summary>
        /// Returns an enumerable collection of dictionary of face parts locations (eyes, nose, etc).
        /// </summary>
        /// <param name="landmarkTuples">The enumerable collection of face parts location.</param>
        /// <returns>An enumerable collection of dictionary of face parts locations (eyes, nose, etc).</returns>
        protected abstract IEnumerable<Dictionary<FacePart, IEnumerable<FacePoint>>> RawGetLandmarks(IEnumerable<FacePoint[]> landmarkTuples);

        #endregion

    }

}