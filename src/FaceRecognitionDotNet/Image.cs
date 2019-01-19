using System;
using DlibDotNet;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Represents a image data. This class cannot be inherited.
    /// </summary>
    public sealed class Image : DisposableObject
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
                this.ThrowIfDisposed();
                return this._Matrix.Rows;
            }
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
                this.ThrowIfDisposed();
                return this._Matrix.Columns;
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

    }

}