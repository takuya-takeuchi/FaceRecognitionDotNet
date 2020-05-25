using System;
using System.Collections.Generic;
using System.Linq;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to detect human eye's blink base on eye aspect ratio (EAR).
    /// </summary>
    public abstract class EyeAspectRatioBaseEyeBlinkDetector : EyeBlinkDetector
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeAspectRatioBaseEyeBlinkDetector"/> class with the threshold and eye point indices for left and right eyes.
        /// </summary>
        /// <param name="leftRatioThreshold">The threshold to decide that left eye blinks or not.</param>
        /// <param name="rightRatioThreshold">The threshold to decide that right eye blinks or not.</param>
        /// <param name="leftEyePointIndices">The indices of left eye location to calculate eye aspect ratio.</param>
        /// <param name="rightEyePointIndices">The indices of right eye location to calculate eye aspect ratio.</param>
        /// <exception cref="ArgumentNullException"><paramref name="leftEyePointIndices"/> or <paramref name="rightEyePointIndices"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="leftEyePointIndices"/> or <paramref name="rightEyePointIndices"/> does not contain 6 elements.</exception>
        protected EyeAspectRatioBaseEyeBlinkDetector(double leftRatioThreshold,
                                                     double rightRatioThreshold,
                                                     int[] leftEyePointIndices,
                                                     int[] rightEyePointIndices)
        {
            if (leftEyePointIndices == null)
                throw new ArgumentException(nameof(leftEyePointIndices));
            if (rightEyePointIndices == null)
                throw new ArgumentException(nameof(rightEyePointIndices));

            if (leftEyePointIndices.Length != 6)
                throw new ArgumentException($"{nameof(leftEyePointIndices)} does not contain 6 elements.", nameof(leftEyePointIndices));
            if (rightEyePointIndices.Length != 6)
                throw new ArgumentException($"{nameof(rightEyePointIndices)} does not contain 6 elements.", nameof(rightEyePointIndices));

            this.LeftRatioThreshold = leftRatioThreshold;
            this.RightRatioThreshold = rightRatioThreshold;
            this.LeftEyePointIndices = leftEyePointIndices;
            this.RightEyePointIndices = rightEyePointIndices;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the array of the indices of left eye location to be used calculating eye aspect ratio.
        /// </summary>
        protected int[] LeftEyePointIndices
        {
            get;
        }

        /// <summary>
        /// Gets the threshold to decide that left eye blinks or not.
        /// </summary>
        public double LeftRatioThreshold
        {
            get;
        }

        /// <summary>
        /// Gets the array of the indices of right eye location to be used calculating eye aspect ratio.
        /// </summary>
        protected int[] RightEyePointIndices
        {
            get;
        }

        /// <summary>
        /// Gets the threshold to decide that right eye blinks or not.
        /// </summary>
        public double RightRatioThreshold
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Detects the values whether human eye's blink or not from face landmark.
        /// </summary>
        /// <param name="landmark">The dictionary of face parts locations (eyes, nose, etc).</param>
        /// <param name="leftBlink">When this method returns, contains <value>true</value>, if the left eye blinks; otherwise, <value>false</value>.</param>
        /// <param name="rightBlink">When this method returns, contains <value>true</value>, if the right eye blinks; otherwise, <value>false</value>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="landmark"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="landmark"/> does not contain <see cref="FacePart.LeftEye"/> or <see cref="FacePart.RightEye"/>.</exception>
        protected override void RawDetect(IDictionary<FacePart, IEnumerable<FacePoint>> landmark, out bool leftBlink, out bool rightBlink)
        {
            if (landmark == null) 
                throw new ArgumentNullException(nameof(landmark));
            if (!landmark.TryGetValue(FacePart.LeftEye, out var leftEye))
                throw new ArgumentException($"{nameof(landmark)} does not contain FacePart.LeftEye.", nameof(landmark));
            if (!landmark.TryGetValue(FacePart.RightEye, out var rightEye))
                throw new ArgumentException($"{nameof(landmark)} does not contain FacePart.RightEye.", nameof(landmark));

            var earLeft = this.GetEar(leftEye.ToArray(), this.LeftEyePointIndices);
            var earRight = this.GetEar(rightEye.ToArray(), this.RightEyePointIndices);

            leftBlink = earLeft < this.LeftRatioThreshold;
            rightBlink = earRight < this.RightRatioThreshold;
        }

        /// <summary>
        /// Return an eye aspect ratio.
        /// </summary>
        /// <param name="eye">The collection of location corresponding to human eye.</param>
        /// <param name="eyePointIndices">The collection of the indices of eye location to be used calculating eye aspect ratio.</param>
        /// <returns>Eye aspect ratio.</returns>
        protected double GetEar(IList<FacePoint> eye, IList<int> eyePointIndices)
        {
            // https://www.pyimagesearch.com/2017/04/24/eye-blink-detection-opencv-python-dlib/

            // compute the euclidean distances between the two sets of
            // vertical eye landmarks (x, y)-coordinates
            var a = Euclidean(eye[eyePointIndices[1]], eye[eyePointIndices[5]]);
            var b = Euclidean(eye[eyePointIndices[2]], eye[eyePointIndices[4]]);

            // compute the euclidean distance between the horizontal
            // eye landmark (x, y)-coordinates
            var c = Euclidean(eye[eyePointIndices[0]], eye[eyePointIndices[3]]);

            // compute the eye aspect ratio
            var ear = (a + b) / (2.0 * c);

            // return the eye aspect ratio
            return ear;
        }

        #region Helpers

        private static double Euclidean(FacePoint p1, FacePoint p2)
        {
            return Math.Sqrt(Math.Pow(p1.Point.X - p2.Point.X, 2.0) + Math.Pow(p1.Point.Y - p2.Point.Y, 2.0));
        }

        #endregion

        #endregion

    }

}