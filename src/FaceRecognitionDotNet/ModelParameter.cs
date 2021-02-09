namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Describes the model binary datum. This class cannot be inherited.
    /// </summary>
    public sealed class ModelParameter
    {

        #region Properties

        /// <summary>
        /// Gets or sets the binary data of model for 68 points face landmarks.
        /// </summary>
        public byte[] PosePredictor68FaceLandmarksModel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the binary data of model for 5 points face landmarks.
        /// </summary>
        public byte[] PosePredictor5FaceLandmarksModel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the binary data of model for face encoding.
        /// </summary>
        public byte[] FaceRecognitionModel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the binary data of model for face detector by using CNN.
        /// </summary>
        public byte[] CnnFaceDetectorModel
        {
            get;
            set;
        }

        #endregion

    }

}
