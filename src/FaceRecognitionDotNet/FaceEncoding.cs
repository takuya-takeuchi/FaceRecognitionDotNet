using System;
using System.Runtime.Serialization;
using DlibDotNet;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Represents a feature data of face. This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class FaceEncoding : DisposableObject, ISerializable
    {

        #region Fields

        [NonSerialized]
        private readonly Matrix<double> _Encoding;

        #endregion

        #region Constructors

        internal FaceEncoding(Matrix<double> encoding)
        {
            this._Encoding = encoding;
        }

        private FaceEncoding(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            var array = (double[])info.GetValue(nameof(this._Encoding), typeof(double[]));
            var row = (int)info.GetValue(nameof(this._Encoding.Rows), typeof(int));
            var column = (int)info.GetValue(nameof(this._Encoding.Columns), typeof(int));
            this._Encoding = new Matrix<double>(array, row, column);
        }

        #endregion

        #region Properties

        internal Matrix<double> Encoding => this._Encoding;

        /// <summary>
        /// Gets the size of feature data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This object is disposed.</exception>
        public int Size
        {
            get
            {
                this.ThrowIfDisposed();
                return this._Encoding.Size;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a feature data of face as raw format.
        /// </summary>
        /// <returns>A <see cref="double"/> array that represents a feature data.</returns>
        /// <remarks><see cref="FaceEncoding"/> class supports serialization. This method is for interoperability between FaceRecognitionDotNet and dlib.</remarks>
        /// <exception cref="ObjectDisposedException">This object is disposed.</exception>
        public double[] GetRawEncoding()
        {
            this.ThrowIfDisposed();
            return this._Encoding.ToArray();
        }

        #region Overrides 

        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            this._Encoding?.Dispose();
        }

        #endregion

        #endregion

        #region ISerializable Members

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this._Encoding), this._Encoding.ToArray());
            info.AddValue(nameof(this._Encoding.Rows), this._Encoding.Rows);
            info.AddValue(nameof(this._Encoding.Columns), this._Encoding.Columns);
        }

        #endregion

    }

}
