using DlibDotNet;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Describes the left, top, right and bottom location of a face. This class cannot be inherited.
    /// </summary>
    public sealed class Location
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> structure with the specified left, top, right and bottom.
        /// </summary>
        /// <param name="left">The x-axis value of the left side of the rectangle of face.</param>
        /// <param name="top">The y-axis value of the top of the rectangle of face.</param>
        /// <param name="right">The x-axis value of the right side of the rectangle of face.</param>
        /// <param name="bottom">The y-axis value of the bottom of the rectangle of face.</param>
        public Location(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        internal Location(Rectangle rect) :
            this(rect.Left, rect.Top, rect.Right, rect.Bottom)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the y-axis value of the bottom of the rectangle of face.
        /// </summary>
        public int Bottom
        {
            get;
        }

        /// <summary>
        /// Gets the x-axis value of the left side of the rectangle of face.
        /// </summary>
        public int Left
        {
            get;
        }

        /// <summary>
        /// Gets the x-axis value of the right side of the rectangle of face.
        /// </summary>
        public int Right
        {
            get;
        }

        /// <summary>
        /// Gets the y-axis value of the top of the rectangle of face.
        /// </summary>
        public int Top
        {
            get;
        }

        #endregion

    }

}
