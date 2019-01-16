using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DlibDotNet;
using DlibDotNet.Dnn;
using FaceRecognitionDotNet.Dlib.Python;
using Rectangle = DlibDotNet.Rectangle;

namespace FaceRecognitionDotNet
{

    /// <summary>
    /// Provides the method to find and recognize face methods. This class cannot be inherited.
    /// </summary>
    public sealed class FaceRecognition : DisposableObject
    {

        #region Fields

        private readonly ShapePredictor _PosePredictor68Point;

        private readonly ShapePredictor _PosePredictor5Point;

        private readonly LossMmod _CnnFaceDetector;

        private readonly LossMetric _FaceEncoder;

        private readonly FrontalFaceDetector _FaceDetector;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceRecognition"/> class with the directory path that stores model files.
        /// </summary>
        /// <param name="directory">The directory path that stores model files.</param>
        /// <exception cref="FileNotFoundException">The model file is not found.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified directory path is not found.</exception>
        private FaceRecognition(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(directory);

            var predictor68PointModel = Path.Combine(directory, FaceRecognitionModels.GetPosePredictorModelLocation());
            if (!File.Exists(predictor68PointModel))
                throw new FileNotFoundException(predictor68PointModel);

            var predictor5PointModel = Path.Combine(directory, FaceRecognitionModels.GetPosePredictorFivePointModelLocation());
            if (!File.Exists(predictor5PointModel))
                throw new FileNotFoundException(predictor5PointModel);

            var cnnFaceDetectionModel = Path.Combine(directory, FaceRecognitionModels.GetCnnFaceDetectorModelLocation());
            if (!File.Exists(cnnFaceDetectionModel))
                throw new FileNotFoundException(cnnFaceDetectionModel);

            var faceRecognitionModel = Path.Combine(directory, FaceRecognitionModels.GetFaceRecognitionModelLocation());
            if (!File.Exists(faceRecognitionModel))
                throw new FileNotFoundException(faceRecognitionModel);

            this._FaceDetector?.Dispose();
            this._FaceDetector = DlibDotNet.Dlib.GetFrontalFaceDetector();

            this._PosePredictor68Point?.Dispose();
            this._PosePredictor68Point = ShapePredictor.Deserialize(predictor68PointModel);

            this._PosePredictor5Point?.Dispose();
            this._PosePredictor5Point = ShapePredictor.Deserialize(predictor5PointModel);

            this._CnnFaceDetector?.Dispose();
            this._CnnFaceDetector = LossMmod.Deserialize(cnnFaceDetectionModel);

            this._FaceEncoder?.Dispose();
            this._FaceEncoder = LossMetric.Deserialize(faceRecognitionModel);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the character encoding to convert System.String to byte[] for internal library.
        /// </summary>
        public static Encoding InternalEncoding
        {
            get => DlibDotNet.Dlib.Encoding;
            set => DlibDotNet.Dlib.Encoding = value ?? Encoding.UTF8;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns an enumerable collection of array of bounding boxes of human faces in a image using the cnn face detector.
        /// </summary>
        /// <param name="images">An enumerable collection of images.</param>
        /// <param name="numberOfTimesToUpsample">The number of image looking for faces. Higher numbers find smaller faces.</param>
        /// <param name="batchSize">The number of images to include in each GPU processing batch.</param>
        /// <returns>An enumerable collection of array of found face locations.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="images"/> is null.</exception>
        public IEnumerable<Location[]> BatchFaceLocations(IEnumerable<Image> images, int numberOfTimesToUpsample = 1, int batchSize = 128)
        {
            var imagesArray = images.ToArray();
            if (!imagesArray.Any())
                yield break;

            var rawDetectionsBatched = this.RawFaceLocationsBatched(imagesArray, numberOfTimesToUpsample, batchSize).ToArray();

            var image = imagesArray[0];
            for (var index = 0; index < rawDetectionsBatched.Length; index++)
            {
                var faces = rawDetectionsBatched[index];
                yield return faces.Select(rect => TrimBound(rect.Rect, image.Width, image.Height)).ToArray();
            }
        }

        /// <summary>
        /// Compare a known face encoding against a candidate encoding to see if they match.
        /// </summary>
        /// <param name="knownFaceEncoding">A known face encodings.</param>
        /// <param name="faceEncodingToCheck">A single face encoding to compare against a known face encoding.</param>
        /// <param name="tolerance">The distance between faces to consider it a match. Lower is more strict. The default value is 0.6.</param>
        /// <returns>A True/False value indicating which known a face encoding matches the face encoding to check.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="knownFaceEncoding"/> or <paramref name="faceEncodingToCheck"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="knownFaceEncoding"/> or <paramref name="faceEncodingToCheck"/>.</exception>
        public static bool CompareFace(FaceEncoding knownFaceEncoding, FaceEncoding faceEncodingToCheck, double tolerance = 0.6d)
        {
            if (knownFaceEncoding == null)
                throw new ArgumentNullException(nameof(knownFaceEncoding));
            if (faceEncodingToCheck == null)
                throw new ArgumentNullException(nameof(faceEncodingToCheck));

            knownFaceEncoding.ThrowIfDisposed();
            faceEncodingToCheck.ThrowIfDisposed();

            return FaceDistance(knownFaceEncoding, faceEncodingToCheck) <= tolerance;
        }

        /// <summary>
        /// Compare an enumerable collection of face encodings against a candidate encoding to see if they match.
        /// </summary>
        /// <param name="knownFaceEncodings">An enumerable collection of known face encodings.</param>
        /// <param name="faceEncodingToCheck">A single face encoding to compare against the enumerable collection.</param>
        /// <param name="tolerance">The distance between faces to consider it a match. Lower is more strict. The default value is 0.6.</param>
        /// <returns>An enumerable collection of True/False values indicating which known face encodings match the face encoding to check.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="knownFaceEncodings"/> or <paramref name="faceEncodingToCheck"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="faceEncodingToCheck"/> is disposed. Or <paramref name="knownFaceEncodings"/> contains disposed object.</exception>
        public static IEnumerable<bool> CompareFaces(IEnumerable<FaceEncoding> knownFaceEncodings, FaceEncoding faceEncodingToCheck, double tolerance = 0.6d)
        {
            if (knownFaceEncodings == null)
                throw new ArgumentNullException(nameof(knownFaceEncodings));
            if (faceEncodingToCheck == null)
                throw new ArgumentNullException(nameof(faceEncodingToCheck));

            faceEncodingToCheck.ThrowIfDisposed();

            var array = knownFaceEncodings.ToArray();
            if (array.Any(encoding => encoding.IsDisposed))
                throw new ObjectDisposedException($"{nameof(knownFaceEncodings)} contains disposed object.");

            if (array.Length == 0)
                yield break;

            foreach (var faceEncoding in array)
                yield return FaceDistance(faceEncoding, faceEncodingToCheck) <= tolerance;
        }

        /// <summary>
        /// Create a new instance of the <see cref="FaceRecognition"/> class.
        /// </summary>
        /// <param name="directory">The directory path that stores model files.</param>
        /// <exception cref="FileNotFoundException">The model file is not found.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified directory path is not found.</exception>
        public static FaceRecognition Create(string directory)
        {
            return new FaceRecognition(directory);
        }

        /// <summary>
        /// Compare a face encoding to a known face encoding and get a euclidean distance for comparison face.
        /// </summary>
        /// <param name="faceEncoding">The face encoding to compare.</param>
        /// <param name="faceToCompare">The face encoding to compare against.</param>
        /// <returns>The euclidean distance for comparison face. If 0, faces are completely equal.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="faceEncoding"/> or <paramref name="faceToCompare"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="faceEncoding"/> or <paramref name="faceToCompare"/> is disposed.</exception>
        public static double FaceDistance(FaceEncoding faceEncoding, FaceEncoding faceToCompare)
        {
            if (faceEncoding == null)
                throw new ArgumentNullException(nameof(faceEncoding));
            if (faceToCompare == null)
                throw new ArgumentNullException(nameof(faceToCompare));

            faceEncoding.ThrowIfDisposed();
            faceToCompare.ThrowIfDisposed();

            if (faceEncoding.Encoding.Size == 0)
                return 0;

            using (var diff = faceEncoding.Encoding - faceToCompare.Encoding)
                return DlibDotNet.Dlib.Length(diff);
        }

        /// <summary>
        /// Compare an enumerable collection of face encoding to a known face encoding and get an enumerable collection of euclidean distance for comparison face.
        /// </summary>
        /// <param name="faceEncodings">The enumerable collection of face encoding to compare.</param>
        /// <param name="faceToCompare">The face encoding to compare against.</param>
        /// <returns>The enumerable collection of euclidean distance for comparison face. If 0, faces are completely equal.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="faceEncodings"/> or <paramref name="faceToCompare"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="faceToCompare"/> is disposed. Or <paramref name="faceEncodings"/> contains disposed object.</exception>
        public static IEnumerable<double> FaceDistances(IEnumerable<FaceEncoding> faceEncodings, FaceEncoding faceToCompare)
        {
            if (faceEncodings == null)
                throw new ArgumentNullException(nameof(faceEncodings));
            if (faceToCompare == null)
                throw new ArgumentNullException(nameof(faceToCompare));

            faceToCompare.ThrowIfDisposed();

            var array = faceEncodings.ToArray();
            if (array.Any(encoding => encoding.IsDisposed))
                throw new ObjectDisposedException($"{nameof(faceEncodings)} contains disposed object.");

            if (array.Length == 0)
                yield break;

            foreach (var faceEncoding in array)
                using (var diff = faceEncoding.Encoding - faceToCompare.Encoding)
                    yield return DlibDotNet.Dlib.Length(diff);
        }

        /// <summary>
        /// Returns an enumerable collection of face feature data corresponds to all faces in specified image.
        /// </summary>
        /// <param name="image">The image contains faces. The image can contain multiple faces.</param>
        /// <param name="knownFaceLocation">The enumerable collection of location rectangle for faces. If specified null, method will find face locations.</param>
        /// <param name="numJitters">The number of times to re-sample the face when calculating encoding.</param>
        /// <returns>An enumerable collection of face feature data corresponds to all faces in specified image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> or this object is disposed.</exception>
        public IEnumerable<FaceEncoding> FaceEncodings(Image image, IEnumerable<Location> knownFaceLocation = null, int numJitters = 1)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            image.ThrowIfDisposed();
            this.ThrowIfDisposed();

            var rawLandmarks = this.RawFaceLandmarks(image, knownFaceLocation, PredictorModel.Small);
            foreach (var landmark in rawLandmarks)
            {
                var ret = new FaceEncoding(FaceRecognitionModelV1.ComputeFaceDescriptor(this._FaceEncoder, image, landmark, numJitters));
                landmark.Dispose();
                yield return ret;
            }
        }

        /// <summary>
        /// Returns an enumerable collection of dictionary of face parts locations (eyes, nose, etc) for each face in the image.
        /// </summary>
        /// <param name="faceImage">The image contains faces. The image can contain multiple faces.</param>
        /// <param name="faceLocations">The enumerable collection of location rectangle for faces. If specified null, method will find face locations.</param>
        /// <param name="model">The model of face detector.</param>
        /// <returns>An enumerable collection of dictionary of face parts locations (eyes, nose, etc).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="faceImage"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="faceImage"/> or this object is disposed.</exception>
        public IEnumerable<IDictionary<FacePart, IEnumerable<Point>>> FaceLandmark(Image faceImage, IEnumerable<Location> faceLocations = null, PredictorModel model = PredictorModel.Large)
        {
            if (faceImage == null)
                throw new ArgumentNullException(nameof(faceImage));

            faceImage.ThrowIfDisposed();
            this.ThrowIfDisposed();

            var landmarks = this.RawFaceLandmarks(faceImage, faceLocations, model).ToArray();
            var landmarkTuples = landmarks.Select(landmark => Enumerable.Range(0, (int)landmark.Parts)
                                          .Select(index => new Point(landmark.GetPart((uint)index))).ToArray());

            var results = new List<Dictionary<FacePart, IEnumerable<Point>>>();

            try
            {

                // For a definition of each point index, see https://cdn-images-1.medium.com/max/1600/1*AbEg31EgkbXSQehuNJBlWg.png
                switch (model)
                {
                    case PredictorModel.Large:
                        results.AddRange(landmarkTuples.Select(landmarkTuple => new Dictionary<FacePart, IEnumerable<Point>>
                        {
                            { FacePart.Chin,         Enumerable.Range(0,17).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.LeftEyebrow,  Enumerable.Range(17,5).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.RightEyebrow, Enumerable.Range(22,5).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.NoseBridge,   Enumerable.Range(27,5).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.NoseTip,      Enumerable.Range(31,5).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.LeftEye,      Enumerable.Range(36,6).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.RightEye,     Enumerable.Range(42,6).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.TopLip,       Enumerable.Range(48,7).Select(i => landmarkTuple[i])
                                                                           .Concat( new [] { landmarkTuple[64] })
                                                                           .Concat( new [] { landmarkTuple[63] })
                                                                           .Concat( new [] { landmarkTuple[62] })
                                                                           .Concat( new [] { landmarkTuple[61] })
                                                                           .Concat( new [] { landmarkTuple[60] }) },
                            { FacePart.BottomLip,    Enumerable.Range(54,6).Select(i => landmarkTuple[i])
                                                                           .Concat( new [] { landmarkTuple[48] })
                                                                           .Concat( new [] { landmarkTuple[60] })
                                                                           .Concat( new [] { landmarkTuple[67] })
                                                                           .Concat( new [] { landmarkTuple[66] })
                                                                           .Concat( new [] { landmarkTuple[65] })
                                                                           .Concat( new [] { landmarkTuple[64] }) }
                        }));
                        break;
                    case PredictorModel.Small:
                        results.AddRange(landmarkTuples.Select(landmarkTuple => new Dictionary<FacePart, IEnumerable<Point>>
                        {
                            { FacePart.NoseTip,  Enumerable.Range(4,1).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.LeftEye,  Enumerable.Range(2,2).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.RightEye, Enumerable.Range(0,2).Select(i => landmarkTuple[i]).ToArray() }
                        }));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(model), model, null);
                }
            }
            finally
            {
                foreach (var landmark in landmarks)
                    landmark.Dispose();
            }

            return results.ToArray();
        }

        /// <summary>
        /// Returns an enumerable collection of face location correspond to all faces in specified image.
        /// </summary>
        /// <param name="image">The image contains faces. The image can contain multiple faces.</param>
        /// <param name="numberOfTimesToUpsample">The number of times to up-sample the image when finding faces.</param>
        /// <param name="model">The model of face detector to detect in image.</param>
        /// <returns>An enumerable collection of face location correspond to all faces in specified image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> or this object is disposed.</exception>
        public IEnumerable<Location> FaceLocations(Image image, int numberOfTimesToUpsample = 1, Model model = Model.Hog)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            image.ThrowIfDisposed();
            this.ThrowIfDisposed();

            switch (model)
            {
                case Model.Cnn:
                    foreach (var face in this.RawFaceLocations(image, numberOfTimesToUpsample, Model.Cnn))
                    {
                        var ret = TrimBound(face.Rect, image.Width, image.Height);
                        face.Dispose();
                        yield return ret;
                    }
                    break;
                default:
                    foreach (var face in this.RawFaceLocations(image, numberOfTimesToUpsample, model))
                    {
                        var ret = TrimBound(face.Rect, image.Width, image.Height);
                        face.Dispose();
                        yield return ret;
                    }
                    break;
            }
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the <see cref="byte"/> array.
        /// </summary>
        /// <param name="array">The <see cref="byte"/> array contains image data.</param>
        /// <param name="row">The number of rows in a image data.</param>
        /// <param name="column">The number of columns in a image data.</param>
        /// <param name="elementSize">The image element size in bytes.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="elementSize"/> must be 3 or 1.</exception>
        public static Image LoadImage(byte[] array, int row, int column, int elementSize)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            switch (elementSize)
            {
                case 3:
                    return new Image(new Matrix<RgbPixel>(array, row, column, elementSize), Mode.Rgb);
                case 1:
                    return new Image(new Matrix<byte>(array, row, column, elementSize), Mode.Greyscale);
            }

            throw new ArgumentOutOfRangeException($"{nameof(elementSize)} must be 3 or 1.");
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified path.
        /// </summary>
        /// <param name="file">A string that contains the path of the file from which to create the <see cref="Image"/>.</param>
        /// <param name="mode">A image color mode.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="FileNotFoundException">The specified path does not exist.</exception>
        public static Image LoadImageFile(string file, Mode mode = Mode.Rgb)
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

        #region Helpers

        private IEnumerable<FullObjectDetection> RawFaceLandmarks(Image faceImage, IEnumerable<Location> faceLocations = null, PredictorModel model = PredictorModel.Large)
        {
            IEnumerable<MModRect> tmp;

            if (faceLocations == null)
                tmp = this.RawFaceLocations(faceImage);
            else
                tmp = faceLocations.Select(l => new MModRect { Rect = new Rectangle { Bottom = l.Bottom, Left = l.Left, Top = l.Top, Right = l.Right } });

            var posePredictor = this._PosePredictor68Point;
            if (model == PredictorModel.Small)
                posePredictor = this._PosePredictor5Point;

            foreach (var rect in tmp)
            {
                var ret = posePredictor.Detect(faceImage.Matrix, rect);
                rect.Dispose();
                yield return ret;
            }
        }

        private IEnumerable<MModRect> RawFaceLocations(Image faceImage, int numberOfTimesToUpsample = 1, Model model = Model.Hog)
        {
            switch (model)
            {
                case Model.Cnn:
                    return CnnFaceDetectionModelV1.Detect(this._CnnFaceDetector, faceImage, numberOfTimesToUpsample);
                default:
                    var locations = SimpleObjectDetector.RunDetectorWithUpscale2(this._FaceDetector, faceImage, (uint)numberOfTimesToUpsample);
                    return locations.Select(rectangle => new MModRect { Rect = rectangle });
            }
        }

        private IEnumerable<IEnumerable<MModRect>> RawFaceLocationsBatched(IEnumerable<Image> faceImages, int numberOfTimesToUpsample = 1, int batchSize = 128)
        {
            return CnnFaceDetectionModelV1.DetectMulti(this._CnnFaceDetector, faceImages, numberOfTimesToUpsample, batchSize);
        }

        private static Location TrimBound(Rectangle location, int width, int height)
        {
            return new Location(Math.Max(location.Left, 0), Math.Max(location.Top, 0), Math.Min(location.Right, width), Math.Min(location.Bottom, height));
        }

        #endregion

        #endregion
        
        #region Methods

        #region Overrides 

        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            this._PosePredictor68Point?.Dispose();
            this._PosePredictor5Point?.Dispose();
            this._CnnFaceDetector?.Dispose();
            this._FaceEncoder?.Dispose();
            this._FaceDetector?.Dispose();
        }

        #endregion

        #endregion

    }

}
