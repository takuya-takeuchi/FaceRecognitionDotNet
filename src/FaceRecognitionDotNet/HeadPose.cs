using System;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Represents an head pose.
    /// </summary>
    public class HeadPose : IEquatable<HeadPose>
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HeadPose"/> class with the specified roll, pitch and yaw.
        /// </summary>
        /// <param name="roll">The roll angle.</param>
        /// <param name="pitch">The pitch angle.</param>
        /// <param name="yaw">The yaw angle.</param>
        public HeadPose(double roll, double pitch, double yaw)
        {
            this.Roll = roll;
            this.Pitch = pitch;
            this.Yaw = yaw;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the pitch angle of this <see cref="HeadPose"/>.
        /// </summary>
        public double Pitch
        {
            get;
        }

        /// <summary>
        /// Gets the roll angle of this <see cref="HeadPose"/>.
        /// </summary>
        public double Roll
        {
            get;
        }

        /// <summary>
        /// Gets the yaw angle of this <see cref="HeadPose"/>.
        /// </summary>
        public double Yaw
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compares two <see cref="HeadPose"/> class for equality.
        /// </summary>
        /// <param name="other">The pose to compare to this instance.</param>
        /// <returns><code>true</code> if both <see cref="HeadPose"/> class contain the same <see cref="Roll"/>, <see cref="Pitch"/> and <see cref="Yaw"/> values; otherwise, <code>false</code>.</returns>
        public bool Equals(HeadPose other)
        {
            return Math.Abs(this.Roll - other.Roll) < double.Epsilon &&
                   Math.Abs(this.Pitch - other.Pitch) < double.Epsilon &&
                   Math.Abs(this.Yaw - other.Yaw) < double.Epsilon;
        }

        #region Overrids

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is a <see cref="HeadPose"/> and whether it contains the same data as this <see cref="HeadPose"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><code>true</code> if <paramref name="obj"/> is a <see cref="HeadPose"/> and contains the same <see cref="Roll"/>, <see cref="Pitch"/> and <see cref="Yaw"/> values as this <see cref="HeadPose"/>; otherwise, <code>false</code>.</returns>
        public override bool Equals(object obj)
        {
            return obj is HeadPose && Equals((HeadPose)obj);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="HeadPose"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="HeadPose"/> structure.</returns>
        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + this.Roll.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Pitch.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Yaw.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="HeadPose"/> class for equality.
        /// </summary>
        /// <param name="pose1">The first <see cref="HeadPose"/> structure to compare.</param>
        /// <param name="pose2">The second <see cref="HeadPose"/> structure to compare.</param>
        /// <returns><code>true</code> if the <see cref="Roll"/>, <see cref="Pitch"/> and <see cref="Yaw"/> of <paramref name="pose1"/> and <paramref name="pose2"/> are equal; otherwise, <code>false</code>.</returns>
        public static bool operator ==(HeadPose pose1, HeadPose pose2)
        {
            return pose1.Equals(pose2);
        }

        /// <summary>
        /// Compares two <see cref="HeadPose"/> class for inequality.
        /// </summary>
        /// <param name="pose1">The first <see cref="HeadPose"/> structure to compare.</param>
        /// <param name="pose2">The second <see cref="HeadPose"/> structure to compare.</param>
        /// <returns><code>true</code> if <paramref name="pose1"/> and <paramref name="pose2"/> have different <see cref="Roll"/> or <see cref="Yaw"/>; <code>false</code> if <paramref name="pose1"/> and <paramref name="pose2"/> have the same <see cref="Roll"/>, <see cref="Pitch"/> and <see cref="Yaw"/>.</returns>
        public static bool operator !=(HeadPose pose1, HeadPose pose2)
        {
            return !(pose1 == pose2);
        }

        #endregion

        #endregion

    }

}
