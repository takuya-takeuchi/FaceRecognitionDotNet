using System.Collections.Generic;
using System.IO;
using System.Linq;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    public sealed class HelenFaceLandmarkDetector : FaceLandmarkDetector
    {

        #region Fields

        private readonly ShapePredictor _Predictor;

        #endregion

        #region Constructors

        public HelenFaceLandmarkDetector(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException(modelPath);

            this._Predictor = ShapePredictor.Deserialize(modelPath);
        }

        #endregion

        #region Methods

        #region Overrides

        protected override IEnumerable<FullObjectDetection> RawDetect(MatrixBase matrix, IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                var rect = new Rectangle(location.Left, location.Top, location.Right, location.Top);
                var ret = this._Predictor.Detect(matrix, rect);
                yield return ret;
            }
        }

        protected override IEnumerable<Dictionary<FacePart, IEnumerable<Point>>> RawGetLandmarks(IEnumerable<Point[]> landmarkTuples)
        {
            return landmarkTuples.Select(landmarkTuple => new Dictionary<FacePart, IEnumerable<Point>>
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