using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    public abstract class FaceLandmarkDetector : DisposableObject
    {

        #region Methods

        public FullObjectDetection Detect(Image image, Location location)
        {
            return this.RawDetect(image.Matrix, location);
        }

        public IEnumerable<Dictionary<FacePart, IEnumerable<Point>>> GetLandmarks(IEnumerable<Point[]> landmarkTuples)
        {
            return this.RawGetLandmarks(landmarkTuples);
        }

        protected abstract FullObjectDetection RawDetect(MatrixBase matrix, Location location);
        
        protected abstract IEnumerable<Dictionary<FacePart, IEnumerable<Point>>> RawGetLandmarks(IEnumerable<Point[]> landmarkTuples);

        #endregion

    }

}