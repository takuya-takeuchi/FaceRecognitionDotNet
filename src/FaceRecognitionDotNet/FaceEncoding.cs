using System;
using DlibDotNet;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Represents a feature data of face. This class cannot be inherited.
    /// </summary>
    public sealed class FaceEncoding : IDisposable
    {

        #region Fields

        private readonly Matrix<double> _Encoding;

        #endregion

        #region Constructors

        internal FaceEncoding(Matrix<double> encoding)
        {
            this._Encoding = encoding;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this object has been disposed of.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        internal Matrix<double> Encoding => this._Encoding;

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by this <see cref="FaceEncoding"/>.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        /// <summary>
        /// Releases all resources used by this <see cref="FaceEncoding"/>.
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
                this._Encoding?.Dispose();
            }

        }

        #endregion

    }

}
