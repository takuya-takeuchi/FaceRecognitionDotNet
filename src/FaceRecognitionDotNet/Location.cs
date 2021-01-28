using System;
using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Describes the left, top, right and bottom location of a face. This class cannot be inherited.
    /// </summary>
    public sealed class Location : IEquatable<Location>
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> structure with the specified left, top, right and bottom.
        /// </summary>
        /// <param name="left">The x-axis value of the left side of the rectangle of face.</param>
        /// <param name="top">The y-axis value of the top of the rectangle of face.</param>
        /// <param name="right">The x-axis value of the right side of the rectangle of face.</param>
        /// <param name="bottom">The y-axis value of the bottom of the rectangle of face.</param>
        public Location(int left, int top, int right, int bottom) :
            this(left, top, right, bottom, -1.0d)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> structure with the specified left, top, right, bottom and confidence.
        /// </summary>
        /// <param name="left">The x-axis value of the left side of the rectangle of face.</param>
        /// <param name="top">The y-axis value of the top of the rectangle of face.</param>
        /// <param name="right">The x-axis value of the right side of the rectangle of face.</param>
        /// <param name="bottom">The y-axis value of the bottom of the rectangle of face.</param>
        /// <param name="confidence">The confidence of detected face.</param>
        public Location(int left, int top, int right, int bottom, double confidence)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.Confidence = confidence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> structure with the specified rectangle and confidence.
        /// </summary>
        /// <param name="rectangle">The rectangle of face.</param>
        /// <param name="confidence">The confidence of detected face.</param>
        internal Location(Rectangle rectangle, double confidence) :
            this(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom, confidence)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> structure with the specified location and confidence.
        /// </summary>
        /// <param name="location">The location of face.</param>
        /// <param name="confidence">The confidence of detected face.</param>
        internal Location(Location location, double confidence) :
            this(location.Left, location.Top, location.Right, location.Bottom, confidence)
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
        /// Gets the confidence of detected face.
        /// </summary>
        public double Confidence
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

        #region Methods

        /// <summary>
        /// Compares two <see cref="Location"/> class for equality.
        /// </summary>
        /// <param name="other">The face location to compare to this instance.</param>
        /// <returns><code>true</code> if both <see cref="Location"/> class contain the same <see cref="Left"/>, <see cref="Top"/>, <see cref="Right"/> and <see cref="Bottom"/> values; otherwise, <code>false</code>.</returns>
        public bool Equals(Location other)
        {
            return other != null &&
                   this.Bottom == other.Bottom &&
                   this.Left == other.Left &&
                   this.Right == other.Right &&
                   this.Top == other.Top;
        }

        #region Overrids

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is a <see cref="Location"/> and whether it contains the same face location as this <see cref="Location"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><code>true</code> if <paramref name="obj"/> is a <see cref="Location"/> and contains the same <see cref="Left"/>, <see cref="Top"/>, <see cref="Right"/> and <see cref="Bottom"/> values as this <see cref="Location"/>; otherwise, <code>false</code>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Location);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Location"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="Location"/> class.</returns>
        public override int GetHashCode()
        {
            var hashCode = -773114317;
            hashCode = hashCode * -1521134295 + this.Bottom.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Left.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Right.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Top.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="Location"/> class for equality.
        /// </summary>
        /// <param name="location1">The first <see cref="Location"/> class to compare.</param>
        /// <param name="location2">The second <see cref="Location"/> class to compare.</param>
        /// <returns><code>true</code> if both the <see cref="Left"/>, <see cref="Top"/>, <see cref="Right"/> and <see cref="Bottom"/> face location of <paramref name="location1"/> and <paramref name="location2"/> are equal; otherwise, <code>false</code>.</returns>
        public static bool operator ==(Location location1, Location location2)
        {
            return EqualityComparer<Location>.Default.Equals(location1, location2);
        }

        /// <summary>
        /// Compares two <see cref="Location"/> class for inequality.
        /// </summary>
        /// <param name="location1">The first <see cref="Location"/> class to compare.</param>
        /// <param name="location2">The second <see cref="Location"/> class to compare.</param>
        /// <returns><code>true</code> if <paramref name="location1"/> and <paramref name="location2"/> have different <see cref="Left"/>, <see cref="Top"/>, <see cref="Right"/> or <see cref="Bottom"/> coordinates; <code>false</code> if <paramref name="location1"/> and <paramref name="location2"/> have the same <see cref="Left"/>, <see cref="Top"/>, <see cref="Right"/> and <see cref="Bottom"/> face location.</returns>
        public static bool operator !=(Location location1, Location location2)
        {
            return !(location1 == location2);
        }

        #endregion

        #endregion

    }

}
