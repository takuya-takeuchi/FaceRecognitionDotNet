using System.Collections.Generic;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to detect human eye's blink from face landmark.
    /// </summary>
    public abstract class EyeBlinkDetector : DisposableObject
    {

        #region Methods

        internal void Detect(IDictionary<FacePart, IEnumerable<FacePoint>> landmark, out bool leftBlink, out bool rightBlink)
        {
            this.RawDetect(landmark, out leftBlink, out rightBlink);
        }

        /// <summary>
        /// Detects the values whether human eye's blink or not from face landmark.
        /// </summary>
        /// <param name="landmark">The dictionary of face parts locations (eyes, nose, etc).</param>
        /// <param name="leftBlink">When this method returns, contains <value>true</value>, if the left eye blinks; otherwise, <value>false</value>.</param>
        /// <param name="rightBlink">When this method returns, contains <value>true</value>, if the right eye blinks; otherwise, <value>false</value>.</param>
        protected abstract void RawDetect(IDictionary<FacePart, IEnumerable<FacePoint>> landmark, out bool leftBlink, out bool rightBlink);

        #endregion

    }

}