namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    public struct Point
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> structure with the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal position of the point.</param>
        /// <param name="y">The vertical position of the point.</param>
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        internal Point(DlibDotNet.Point point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the x-coordinate of this <see cref="Point"/>.
        /// </summary>
        public int X
        {
            get;
        }

        /// <summary>
        /// Gets the y-coordinate of this <see cref="Point"/>.
        /// </summary>
        public int Y
        {
            get;
        }

        #endregion

    }

}
