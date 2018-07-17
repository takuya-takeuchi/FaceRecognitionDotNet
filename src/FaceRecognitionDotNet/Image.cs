using System;
using DlibDotNet;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Represents a image data. This class cannot be inherited.
    /// </summary>
    public sealed class Image : IDisposable
    {

        #region Fields

        private readonly Matrix<RgbPixel> _RgbMatrix;

        #endregion

        #region Constructors

        internal Image(Matrix<RgbPixel> matrix)
        {
            this._RgbMatrix = matrix;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height => this._RgbMatrix.Rows;

        /// <summary>
        /// Gets a value indicating whether this object has been disposed of.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        internal Matrix<RgbPixel> Matrix => this._RgbMatrix;

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width => this._RgbMatrix.Columns;

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by this <see cref="Image"/>.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        /// <summary>
        /// Releases all resources used by this <see cref="Image"/>.
        /// </summary>
        /// <param name="disposing">Indicate value whether <see cref="IDisposable.Dispose"/> method was called.</param>
        private void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;

            if (disposing)
            {
                this._RgbMatrix?.Dispose();
            }

        }

        #endregion

    }

}
