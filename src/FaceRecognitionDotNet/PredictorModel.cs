namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Specifies the dimension of vector which be returned from detector.
    /// </summary>
    public enum PredictorModel
    {

        /// <summary>
        /// Specifies that the large scale detector. The detector returns 68 points for represent face. 
        /// </summary>
        Large,

        /// <summary>
        /// Specifies that the small scale detector. The detector returns 5 points for represent face. 
        /// </summary>
        Small,
        
        /// <summary>
        /// Specifies that the custom detector.
        /// </summary>
        Custom

    }

}