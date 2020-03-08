namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// Represents a range that has start and end age.
    /// </summary>
    public struct AgeRange
    {

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="AgeRange"/> instance with the specified starting and ending ages.
        /// </summary>
        /// <param name="start">The inclusive age index of the range.</param>
        /// <param name="end">The exclusive end age of the range.</param>
        public AgeRange(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an age that represents the exclusive end of the range.
        /// </summary>
        public int End
        {
            get;
        }

        /// <summary>
        /// Gets the inclusive start of the <see cref="T:FaceRecognitionDotNet.Extensions.AgeRange"/>.
        /// </summary>
        public int Start
        {
            get;
        }

        #endregion

    }

}