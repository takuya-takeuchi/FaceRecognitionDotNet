using System;
using System.Collections.Generic;
using System.IO;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The face detector which was trained by custom dataset. This class cannot be inherited.
    /// </summary>
    public sealed class SimpleFaceDetector : FaceDetector
    {

        #region Fields

        private readonly ScanFHogPyramid<PyramidDown, DefaultFHogFeatureExtractor> _Scanner;

        private readonly ObjectDetector<ScanFHogPyramid<PyramidDown, DefaultFHogFeatureExtractor>> _ObjectDetector;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleFaceDetector"/> class with the model file path that this detector uses.
        /// </summary>
        /// <param name="modelPath">The model file path that this detector uses.</param>
        /// <exception cref="FileNotFoundException">The model file is not found.</exception>
        public SimpleFaceDetector(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException(modelPath);
            
            this._Scanner = new ScanFHogPyramid<PyramidDown, DefaultFHogFeatureExtractor>(6);
            this._ObjectDetector = new ObjectDetector<ScanFHogPyramid<PyramidDown, DefaultFHogFeatureExtractor>>(this._Scanner);
            this._ObjectDetector.Deserialize(modelPath);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns an enumerable collection of face location correspond to all faces in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="numberOfTimesToUpsample">The number of times to up-sample the image when finding faces.</param>
        /// <returns>An enumerable collection of face location correspond to all faces.</returns>
        protected override IEnumerable<Location> RawDetect(MatrixBase matrix, int numberOfTimesToUpsample)
        {
            if (!(matrix is Matrix<RgbPixel> mat))
                throw new ArgumentException();

            this._ObjectDetector.Operator(mat, out IEnumerable<Tuple<double, Rectangle>> tuples);

            foreach (var (confidence, rect) in tuples)
                yield return new Location(rect, confidence);
        }

        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            this._Scanner?.Dispose();
            this._ObjectDetector?.Dispose();
        }

        #endregion

    }

}