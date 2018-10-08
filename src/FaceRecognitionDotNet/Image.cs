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

        private readonly MatrixBase _Matrix;

        private readonly Mode _Mode;

        #endregion

        #region Constructors

        internal Image(MatrixBase matrix, Mode mode)
        {
            this._Matrix = matrix;
            this._Mode = mode;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height
        {
            get
            {
                if (this.IsDisposed)
                    throw new ObjectDisposedException($"{nameof(Image)}");
                return this._Matrix.Rows;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed of.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        internal MatrixBase Matrix => this._Matrix;

        internal Mode Mode => this._Mode;

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width
        {
            get
            {
                if (this.IsDisposed)
                    throw new ObjectDisposedException($"{nameof(Image)}");
                return this._Matrix.Columns;
            }
        }

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
                this._Matrix?.Dispose();
            }

        }

        #endregion

    }

}
