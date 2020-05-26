using System;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Represents an coordinate and index of face parts.
    /// </summary>
    public class FacePoint : IEquatable<FacePoint>
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FacePoint"/> class with the specified coordinates and index.
        /// </summary>
        /// <param name="point">The coordinate of face parts.</param>
        /// <param name="index">The index of face parts.</param>
        public FacePoint(Point point, int index)
        {
            this.Point = point;
            this.Index = index;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the coordinate of this <see cref="FacePoint"/>.
        /// </summary>
        public Point Point
        {
            get;
        }

        /// <summary>
        /// Gets the index of this <see cref="FacePoint"/>.
        /// </summary>
        public int Index
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compares two <see cref="FacePoint"/> class for equality.
        /// </summary>
        /// <param name="other">The point to compare to this instance.</param>
        /// <returns><code>true</code> if both <see cref="FacePoint"/> class contain the same <see cref="Point"/> and <see cref="Index"/> values; otherwise, <code>false</code>.</returns>
        public bool Equals(FacePoint other)
        {
            return this.Point == other.Point &&
                   this.Index == other.Index;
        }

        #region Overrids

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is a <see cref="FacePoint"/> and whether it contains the same data as this <see cref="FacePoint"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><code>true</code> if <paramref name="obj"/> is a <see cref="FacePoint"/> and contains the same <see cref="Point"/> and <see cref="Index"/> values as this <see cref="FacePoint"/>; otherwise, <code>false</code>.</returns>
        public override bool Equals(object obj)
        {
            return obj is FacePoint && Equals((FacePoint)obj);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="FacePoint"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="FacePoint"/> class.</returns>
        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + this.Point.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Index.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="FacePoint"/> class for equality.
        /// </summary>
        /// <param name="point1">The first <see cref="FacePoint"/> class to compare.</param>
        /// <param name="point2">The second <see cref="FacePoint"/> class to compare.</param>
        /// <returns><code>true</code> if both the <see cref="Point"/> and <see cref="Index"/> of <paramref name="point1"/> and <paramref name="point2"/> are equal; otherwise, <code>false</code>.</returns>
        public static bool operator ==(FacePoint point1, FacePoint point2)
        {
            return point1.Equals(point2);
        }

        /// <summary>
        /// Compares two <see cref="FacePoint"/> class for inequality.
        /// </summary>
        /// <param name="point1">The first <see cref="FacePoint"/> class to compare.</param>
        /// <param name="point2">The second <see cref="FacePoint"/> class to compare.</param>
        /// <returns><code>true</code> if <paramref name="point1"/> and <paramref name="point2"/> have different <see cref="Point"/> or <see cref="Index"/>; <code>false</code> if <paramref name="point1"/> and <paramref name="point2"/> have the same <see cref="Point"/> and <see cref="Index"/>.</returns>
        public static bool operator !=(FacePoint point1, FacePoint point2)
        {
            return !(point1 == point2);
        }

        #endregion

        #endregion

    }

}
