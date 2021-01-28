using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to detect face locations from image.
    /// </summary>
    public abstract class FaceDetector : DisposableObject
    {

        #region Methods

        internal IEnumerable<Location> Detect(Image image, int numberOfTimesToUpsample)
        {
            return this.RawDetect(image.Matrix, numberOfTimesToUpsample);
        }

        /// <summary>
        /// Returns an enumerable collection of face location correspond to all faces in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="numberOfTimesToUpsample">The number of times to up-sample the image when finding faces.</param>
        /// <returns>An enumerable collection of face location correspond to all faces.</returns>
        protected abstract IEnumerable<Location> RawDetect(MatrixBase matrix, int numberOfTimesToUpsample);

        #endregion

    }

}