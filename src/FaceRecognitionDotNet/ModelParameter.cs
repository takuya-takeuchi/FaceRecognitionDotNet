using System;
using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet
{

    public sealed class ModelParameter
    {

        #region Properties

        public byte[] PosePredictor68FaceLandmarksModel
        {
            get;
            set;
        }

        public byte[] PosePredictor5FaceLandmarksModel
        {
            get;
            set;
        }

        public byte[] FaceRecognitionModel
        {
            get;
            set;
        }

        public byte[] CnnFaceDetectorModel
        {
            get;
            set;
        }

        #endregion

    }

}
