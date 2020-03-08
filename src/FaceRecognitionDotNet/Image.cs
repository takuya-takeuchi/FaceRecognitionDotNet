using System;
using System.IO;
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

        /// <summary>
        /// Saves this <see cref="Image"/> to the specified file.
        /// </summary>
        /// <param name="filename">A string that contains the name of the file to which to save this <see cref="Image"/>.</param>
        /// <param name="format">The <see cref="ImageFormat"/> for this <see cref="Image"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filename"/> is null.</exception>
        public void Save(string filename, ImageFormat format)
        {
            if (filename == null) 
                throw new ArgumentNullException(nameof(filename));

            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory) && !string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            switch (format)
            {
                case ImageFormat.Bmp:
                    DlibDotNet.Dlib.SaveBmp(this._Matrix, filename);
                    break;
                case ImageFormat.Jpeg:
                    DlibDotNet.Dlib.SaveJpeg(this._Matrix, filename);
                    break;
                case ImageFormat.Png:
                    DlibDotNet.Dlib.SavePng(this._Matrix, filename);
                    break;
            }
        }

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