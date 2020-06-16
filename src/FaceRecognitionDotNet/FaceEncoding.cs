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
        private readonly Matrix<double> _Matrix;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an <see cref="FaceEncoding"/> from the <see cref="double"/> array.
        /// </summary>
        /// <param name="encoding">The <see cref="double"/> array contains face encoding data.</param>
        /// <returns>The <see cref="FaceEncoding"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="encoding"/> must be 128.</exception>
        public FaceEncoding(double[] encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (encoding.Length != 128)
                throw new ArgumentOutOfRangeException($"{nameof(encoding)}.{nameof(encoding.Length)} must be 128.");

            _Matrix = Matrix<double>.CreateTemplateParameterizeMatrix(0, 1);
            _Matrix.SetSize(128);
            _Matrix.Assign(encoding);
        }

        public FaceEncoding(Matrix<double> matrix)
        {
            this._Matrix = matrix;
        }

        private FaceEncoding(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            var array = (double[])info.GetValue(nameof(this._Matrix), typeof(double[]));
            var row = (int)info.GetValue(nameof(this._Matrix.Rows), typeof(int));
            var column = (int)info.GetValue(nameof(this._Matrix.Columns), typeof(int));
            this._Matrix = new Matrix<double>(array, row, column);
        }

        #endregion

        #region Properties

        public Matrix<double> Matrix => this._Matrix;

        public double[] Torray => _Matrix.ToArray();

        /// <summary>
        /// Gets the size of feature data.
        /// </summary>
        public int Size
        {
            get
            {
                this.ThrowIfDisposed();
                return this._Matrix.Size;
            }
        }

        #endregion

        #region Methods

        #region Overrides 

        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            this._Matrix?.Dispose();
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
            info.AddValue(nameof(this._Matrix), this._Matrix.ToArray());
            info.AddValue(nameof(this._Matrix.Rows), this._Matrix.Rows);
            info.AddValue(nameof(this._Matrix.Columns), this._Matrix.Columns);
        }

        #endregion
    }

}
