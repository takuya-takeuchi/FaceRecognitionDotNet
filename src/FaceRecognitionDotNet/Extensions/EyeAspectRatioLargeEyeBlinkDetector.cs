namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The eye blink detector for <see cref="PredictorModel.Large"/>. This class cannot be inherited.
    /// </summary>
    public sealed class EyeAspectRatioLargeEyeBlinkDetector : EyeAspectRatioBaseEyeBlinkDetector
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeAspectRatioLargeEyeBlinkDetector"/> class with the threshold for left and right eyes.
        /// </summary>
        /// <param name="leftRatioThreshold">The threshold to decide that left eye blinks or not.</param>
        /// <param name="rightRatioThreshold">The threshold to decide that right eye blinks or not.</param>
        public EyeAspectRatioLargeEyeBlinkDetector(double leftRatioThreshold,
            double rightRatioThreshold)
            : base(leftRatioThreshold, rightRatioThreshold, new[] { 0, 1, 2, 3, 4, 5 }, new[] { 0, 1, 2, 3, 4, 5 })
        {
        }

    }

}