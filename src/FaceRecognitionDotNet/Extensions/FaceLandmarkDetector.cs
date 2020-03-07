using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    public abstract class FaceLandmarkDetector : DisposableObject, IExtension
    {

        #region Methods

        public IEnumerable<FullObjectDetection> Detect(Image image, IEnumerable<Location> locations)
        {
            return this.RawDetect(image.Matrix, locations);
        }

        public IEnumerable<IDictionary<FacePart, IEnumerable<Point>>> GetLandmarks(IEnumerable<Point[]> landmarkTuples)
        {
            return this.RawGetLandmarks(landmarkTuples);
        }

        protected abstract IEnumerable<FullObjectDetection> RawDetect(MatrixBase matrix, IEnumerable<Location> locations);
        
        protected abstract IEnumerable<IDictionary<FacePart, IEnumerable<Point>>> RawGetLandmarks(IEnumerable<Point[]> landmarkTuples);

        #endregion

    }

}