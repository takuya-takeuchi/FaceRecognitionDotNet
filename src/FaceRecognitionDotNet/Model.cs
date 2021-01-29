namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Specifies the model of face detector.
    /// </summary>
    public enum Model
    {

        /// <summary>
        /// Specifies that the model is HOG (Histograms of Oriented Gradients) based face detector.
        /// </summary>
        Hog,

        /// <summary>
        /// Specifies that the model is CNN (Convolutional Neural Network) based face detector.
        /// </summary>
        Cnn,
        
        /// <summary>
        /// Specifies that the custom face detector.
        /// </summary>
        Custom

    }

}