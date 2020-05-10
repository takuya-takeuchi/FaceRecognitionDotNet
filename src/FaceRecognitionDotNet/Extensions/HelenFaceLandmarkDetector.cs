using System.Collections.Generic;
using System.IO;
using System.Linq;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The face landmark detector which was trained by helen dataset. This class cannot be inherited.
    /// </summary>
    public sealed class HelenFaceLandmarkDetector : FaceLandmarkDetector
    {

        #region Fields

        private readonly ShapePredictor _Predictor;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HelenFaceLandmarkDetector"/> class with the model file path that this detector uses.
        /// </summary>
        /// <param name="modelPath">The model file path that this detector uses.</param>
        /// <exception cref="FileNotFoundException">The model file is not found.</exception>
        public HelenFaceLandmarkDetector(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException(modelPath);

            this._Predictor = ShapePredictor.Deserialize(modelPath);
        }

        #endregion

        #region Methods

        #region Overrides

        /// <summary>
        /// Returns an object contains information of face parts corresponds to specified location in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An object contains information of face parts.</returns>
        protected override FullObjectDetection RawDetect(MatrixBase matrix, Location location)
        {
            var rect = new Rectangle(location.Left, location.Top, location.Right, location.Top);
            return this._Predictor.Detect(matrix, rect);
        }

        /// <summary>
        /// Returns an enumerable collection of dictionary of face parts locations (eyes, nose, etc).
        /// </summary>
        /// <param name="landmarkTuples">The enumerable collection of face parts location.</param>
        /// <returns>An enumerable collection of dictionary of face parts locations (eyes, nose, etc).</returns>
        protected override IEnumerable<Dictionary<FacePart, IEnumerable<FacePoint>>> RawGetLandmarks(IEnumerable<FacePoint[]> landmarkTuples)
        {
            return landmarkTuples.Select(landmarkTuple => new Dictionary<FacePart, IEnumerable<FacePoint>>
            {
                { FacePart.Chin,         Enumerable.Range(  0,41).Select(i => landmarkTuple[i]) },
                { FacePart.LeftEyebrow,  Enumerable.Range(174,20).Select(i => landmarkTuple[i]) },
                { FacePart.RightEyebrow, Enumerable.Range(154,20).Select(i => landmarkTuple[i]) },
                { FacePart.Nose,         Enumerable.Range( 41,17).Select(i => landmarkTuple[i]) },
                { FacePart.LeftEye,      Enumerable.Range(134,20).Select(i => landmarkTuple[i]) },
                { FacePart.RightEye,     Enumerable.Range(114,20).Select(i => landmarkTuple[i]) },
                { FacePart.TopLip,       Enumerable.Range( 58,14).Select(i => landmarkTuple[i])
                                                   .Concat( Enumerable.Range(86,15).Reverse().Select(i => landmarkTuple[i])) },
                { FacePart.BottomLip,    Enumerable.Range(100,14).Select(i => landmarkTuple[i])
                                                   .Concat( new [] { landmarkTuple[86] })
                                                   .Concat( new [] { landmarkTuple[58] })
                                                   .Concat( Enumerable.Range(71,15).Reverse().Select(i => landmarkTuple[i])) }
            });
        }
        
        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            this._Predictor?.Dispose();
        }

        #endregion

        #endregion

    }

}