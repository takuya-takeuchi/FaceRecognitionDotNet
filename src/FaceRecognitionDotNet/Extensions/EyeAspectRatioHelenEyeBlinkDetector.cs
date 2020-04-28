namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The eye blink detector for face landmark which was trained by helen dataset. This class cannot be inherited.
    /// </summary>
    public sealed class EyeAspectRatioHelenEyeBlinkDetector : EyeAspectRatioBaseEyeBlinkDetector
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeAspectRatioHelenEyeBlinkDetector"/> class with the threshold for left and right eyes.
        /// </summary>
        /// <param name="leftRatioThreshold">The threshold to decide that left eye blinks or not.</param>
        /// <param name="rightRatioThreshold">The threshold to decide that right eye blinks or not.</param>
        public EyeAspectRatioHelenEyeBlinkDetector(double leftRatioThreshold,
            double rightRatioThreshold)
            : base(leftRatioThreshold, rightRatioThreshold, new[] { 0, 3, 7, 10, 13, 17 }, new[] { 0, 3, 7, 10, 13, 17 })
        {
        }

    }

}