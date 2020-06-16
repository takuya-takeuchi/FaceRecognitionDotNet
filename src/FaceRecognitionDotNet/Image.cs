using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using DlibDotNet;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Represents a image data. This class cannot be inherited.
    /// </summary>
    public sealed class Image : DisposableObject
    {
        #region Constructors

        internal Image(MatrixBase matrix, Mode mode)
        {
            this.Matrix = matrix;
            this.Mode = mode;
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified existing bitmap image.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> from which to create the new <see cref="Image"/>.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified <see cref="PixelFormat"/> is not supported.</exception>
        public static Image Load(Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            var rect = new System.Drawing.Rectangle(0, 0, width, height);
            var format = bitmap.PixelFormat;

            Mode mode;
            int srcChannel;
            int dstChannel;
            switch (format)
            {
                case PixelFormat.Format8bppIndexed:
                    mode = Mode.Greyscale;
                    srcChannel = 1;
                    dstChannel = 1;
                    break;
                case PixelFormat.Format24bppRgb:
                    mode = Mode.Rgb;
                    srcChannel = 3;
                    dstChannel = 3;
                    break;
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                    mode = Mode.Rgb;
                    srcChannel = 4;
                    dstChannel = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(bitmap)}", $"The specified {nameof(PixelFormat)} is not supported.");
            }

            BitmapData data = null;

            try
            {
                data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, format);

                unsafe
                {
                    var array = new byte[width * height * dstChannel];
                    fixed (byte* pArray = &array[0])
                    {
                        var dst = pArray;

                        switch (srcChannel)
                        {
                            case 1:
                                {
                                    var src = data.Scan0;
                                    var stride = data.Stride;

                                    for (var h = 0; h < height; h++)
                                        Marshal.Copy(IntPtr.Add(src, h * stride), array, h * width, width * dstChannel);
                                }
                                break;
                            case 3:
                            case 4:
                                {
                                    var src = (byte*)data.Scan0;
                                    var stride = data.Stride;

                                    for (var h = 0; h < height; h++)
                                    {
                                        var srcOffset = h * stride;
                                        var dstOffset = h * width * dstChannel;

                                        for (var w = 0; w < width; w++)
                                        {
                                            // BGR order to RGB order
                                            dst[dstOffset + w * dstChannel + 0] = src[srcOffset + w * srcChannel + 2];
                                            dst[dstOffset + w * dstChannel + 1] = src[srcOffset + w * srcChannel + 1];
                                            dst[dstOffset + w * dstChannel + 2] = src[srcOffset + w * srcChannel + 0];
                                        }
                                    }
                                }
                                break;
                        }

                        var ptr = (IntPtr)pArray;
                        switch (mode)
                        {
                            case Mode.Rgb:
                                return new Image(new Matrix<RgbPixel>(ptr, height, width, width * 3), Mode.Rgb);
                            case Mode.Greyscale:
                                return new Image(new Matrix<byte>(ptr, height, width, width), Mode.Greyscale);
                        }
                    }
                }
            }
            finally
            {
                if (data != null) bitmap.UnlockBits(data);
            }

            return null;
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the <see cref="byte"/> array.
        /// </summary>
        /// <param name="array">The <see cref="byte"/> array contains image data.</param>
        /// <param name="row">The number of rows in a image data.</param>
        /// <param name="column">The number of columns in a image data.</param>
        /// <param name="stride">The stride width in bytes.</param>
        /// <param name="mode">A image color mode.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="column"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stride"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stride"/> is less than <paramref name="column"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> x <paramref name="stride"/> is less than <see cref="Array.Length"/>.</exception>
        public static Image Load(byte[] array, int row, int column, int stride, Mode mode)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (row < 0)
                throw new ArgumentOutOfRangeException($"{nameof(row)}", $"{nameof(row)} is less than 0.");
            if (column < 0)
                throw new ArgumentOutOfRangeException($"{nameof(column)}", $"{nameof(column)} is less than 0.");
            if (stride < 0)
                throw new ArgumentOutOfRangeException($"{nameof(stride)}", $"{nameof(stride)} is less than 0.");
            if (stride < column)
                throw new ArgumentOutOfRangeException($"{nameof(stride)}", $"{nameof(stride)} is less than {nameof(column)}.");
            var min = row * stride;
            if (!(array.Length >= min))
                throw new ArgumentOutOfRangeException("", $"{nameof(row)} x {nameof(stride)} is less than {nameof(Array)}.{nameof(Array.Length)}.");

            unsafe
            {
                fixed (byte* p = &array[0])
                {
                    var ptr = (IntPtr)p;
                    switch (mode)
                    {
                        case Mode.Rgb:
                            return new Image(new Matrix<RgbPixel>(ptr, row, column, stride), Mode.Rgb);
                        case Mode.Greyscale:
                            return new Image(new Matrix<byte>(ptr, row, column, stride), Mode.Greyscale);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the unmanaged memory pointer indicates <see cref="byte"/> array image data.
        /// </summary>
        /// <param name="array">The unmanaged memory pointer indicates <see cref="byte"/> array image data.</param>
        /// <param name="row">The number of rows in a image data.</param>
        /// <param name="column">The number of columns in a image data.</param>
        /// <param name="stride">The stride width in bytes.</param>
        /// <param name="mode">A image color mode.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentException"><paramref name="array"/> is <see cref="IntPtr.Zero"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="column"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stride"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stride"/> is less than <paramref name="column"/>.</exception>
        public static Image Load(IntPtr array, int row, int column, int stride, Mode mode)
        {
            if (array == IntPtr.Zero)
                throw new ArgumentException($"{nameof(array)} is {nameof(IntPtr)}.{nameof(IntPtr.Zero)}", nameof(array));
            if (row < 0)
                throw new ArgumentOutOfRangeException($"{nameof(row)}", $"{nameof(row)} is less than 0.");
            if (column < 0)
                throw new ArgumentOutOfRangeException($"{nameof(column)}", $"{nameof(column)} is less than 0.");
            if (stride < 0)
                throw new ArgumentOutOfRangeException($"{nameof(stride)}", $"{nameof(stride)} is less than 0.");
            if (stride < column)
                throw new ArgumentOutOfRangeException($"{nameof(stride)}", $"{nameof(stride)} is less than {nameof(column)}.");

            switch (mode)
            {
                case Mode.Rgb:
                    return new Image(new Matrix<RgbPixel>(array, row, column, stride), mode);
                case Mode.Greyscale:
                    return new Image(new Matrix<byte>(array, row, column, stride), mode);
            }

            return null;
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified path.
        /// </summary>
        /// <param name="file">A string that contains the path of the file from which to create the <see cref="Image"/>.</param>
        /// <param name="mode">A image color mode.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="FileNotFoundException">The specified path does not exist.</exception>
        public static Image Load(string file, Mode mode = Mode.Rgb)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(file);

            switch (mode)
            {
                case Mode.Rgb:
                    return new Image(DlibDotNet.Dlib.LoadImageAsMatrix<RgbPixel>(file), mode);
                case Mode.Greyscale:
                    return new Image(DlibDotNet.Dlib.LoadImageAsMatrix<byte>(file), mode);
            }

            return null;
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
                return this.Matrix.Rows;
            }
        }

        public MatrixBase Matrix { get; }

        public Mode Mode { get; }

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width
        {
            get
            {
                this.ThrowIfDisposed();
                return this.Matrix.Columns;
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
                    DlibDotNet.Dlib.SaveBmp(this.Matrix, filename);
                    break;
                case ImageFormat.Jpeg:
                    DlibDotNet.Dlib.SaveJpeg(this.Matrix, filename);
                    break;
                case ImageFormat.Png:
                    DlibDotNet.Dlib.SavePng(this.Matrix, filename);
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
            this.Matrix?.Dispose();
        }

        #endregion

        #endregion

    }

}