using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DlibDotNet;
using DlibDotNet.Dnn;
using FaceRecognitionDotNet.Dlib.Python;
using FaceRecognitionDotNet.Extensions;
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

        private FaceLandmarkDetector _CustomFaceLandmarkDetector;

        private FaceDetector _CustomFaceDetector;

        private AgeEstimator _CustomAgeEstimator;

        private EmotionEstimator _CustomEmotionEstimator;

        private GenderEstimator _CustomGenderEstimator;

        private EyeBlinkDetector _CustomEyeBlinkDetector;

        private HeadPoseEstimator _CustomHeadPoseEstimator;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceRecognition"/> class with the instance that contains model binary datum.
        /// </summary>
        /// <param name="parameter">The instance that contains model binary datum.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is null.</exception>
        /// <exception cref="NullReferenceException">The model data is null.</exception>
        private FaceRecognition(ModelParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (parameter.PosePredictor5FaceLandmarksModel == null)
                throw new NullReferenceException(nameof(parameter.PosePredictor5FaceLandmarksModel));

            if (parameter.PosePredictor68FaceLandmarksModel == null)
                throw new NullReferenceException(nameof(parameter.PosePredictor68FaceLandmarksModel));

            if (parameter.CnnFaceDetectorModel == null)
                throw new NullReferenceException(nameof(parameter.CnnFaceDetectorModel));

            if (parameter.FaceRecognitionModel == null)
                throw new NullReferenceException(nameof(parameter.FaceRecognitionModel));

            this._FaceDetector?.Dispose();
            this._FaceDetector = DlibDotNet.Dlib.GetFrontalFaceDetector();

            this._PosePredictor68Point?.Dispose();
            this._PosePredictor68Point = ShapePredictor.Deserialize(parameter.PosePredictor68FaceLandmarksModel);

            this._PosePredictor5Point?.Dispose();
            this._PosePredictor5Point = ShapePredictor.Deserialize(parameter.PosePredictor5FaceLandmarksModel);

            this._CnnFaceDetector?.Dispose();
            this._CnnFaceDetector = LossMmod.Deserialize(parameter.CnnFaceDetectorModel);

            this._FaceEncoder?.Dispose();
            this._FaceEncoder = LossMetric.Deserialize(parameter.FaceRecognitionModel);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the custom age estimator that user defined.
        /// </summary>
        public AgeEstimator CustomAgeEstimator
        {
            get => this._CustomAgeEstimator;
            set => this._CustomAgeEstimator = value;
        }

        /// <summary>
        /// Gets or sets the custom emotion estimator that user defined.
        /// </summary>
        public EmotionEstimator CustomEmotionEstimator
        {
            get => this._CustomEmotionEstimator;
            set => this._CustomEmotionEstimator = value;
        }

        /// <summary>
        /// Gets or sets the custom eye blink detector that user defined.
        /// </summary>
        public EyeBlinkDetector CustomEyeBlinkDetector
        {
            get => this._CustomEyeBlinkDetector;
            set => this._CustomEyeBlinkDetector = value;
        }

        /// <summary>
        /// Gets or sets the custom gender estimator that user defined.
        /// </summary>
        public GenderEstimator CustomGenderEstimator
        {
            get => this._CustomGenderEstimator;
            set => this._CustomGenderEstimator = value;
        }

        /// <summary>
        /// Gets or sets the custom face detector that user defined.
        /// </summary>
        public FaceDetector CustomFaceDetector
        {
            get => this._CustomFaceDetector;
            set => this._CustomFaceDetector = value;
        }

        /// <summary>
        /// Gets or sets the custom face landmark detector that user defined.
        /// </summary>
        public FaceLandmarkDetector CustomFaceLandmarkDetector
        {
            get => this._CustomFaceLandmarkDetector;
            set => this._CustomFaceLandmarkDetector = value;
        }

        /// <summary>
        /// Gets or sets the custom head pose estimator that user defined.
        /// </summary>
        public HeadPoseEstimator CustomHeadPoseEstimator
        {
            get => this._CustomHeadPoseEstimator;
            set => this._CustomHeadPoseEstimator = value;
        }

        /// <summary>
        /// Gets or sets the character encoding to convert <see cref="System.String"/> to array of <see cref="byte"/> for internal library.
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
            if (images == null)
                throw new ArgumentNullException(nameof(images));

            var results = new List<Location[]>();

            var imagesArray = images.ToArray();
            if (!imagesArray.Any())
                return results;

            var rawDetectionsBatched = this.RawFaceLocationsBatched(imagesArray, numberOfTimesToUpsample, batchSize).ToArray();

            var image = imagesArray[0];
            for (var index = 0; index < rawDetectionsBatched.Length; index++)
            {
                var faces = rawDetectionsBatched[index].ToArray();
                var locations = faces.Select(rect => new Location(TrimBound(rect.Rect, image.Width, image.Height), rect.DetectionConfidence)).ToArray();
                foreach (var face in faces)
                    face.Dispose();                
                results.Add(locations);
            }

            return results;
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

            var results = new List<bool>();
            if (array.Length == 0)
                return results;

            foreach (var faceEncoding in array)
                results.Add(FaceDistance(faceEncoding, faceEncodingToCheck) <= tolerance);

            return results;
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
        /// Create a new instance of the <see cref="FaceRecognition"/> class.
        /// </summary>
        /// <param name="parameter">The instance that contains model binary datum.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is null.</exception>
        /// <exception cref="NullReferenceException">The model data is null.</exception>
        public static FaceRecognition Create(ModelParameter parameter)
        {
            return new FaceRecognition(parameter);
        }

        /// <summary>
        /// Crop a specified image with enumerable collection of face locations.
        /// </summary>
        /// <param name="image">The image contains a face.</param>
        /// <param name="locations">The enumerable collection of location rectangle for faces.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="locations"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> is disposed.</exception>
        public static IEnumerable<Image> CropFaces(Image image, IEnumerable<Location> locations)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (locations == null)
                throw new ArgumentNullException(nameof(locations));

            image.ThrowIfDisposed();

            var results = new List<Image>();
            foreach (var location in locations)
            {
                var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);
                var dPoint = new[]
                {
                    new DPoint(rect.Left, rect.Top),
                    new DPoint(rect.Right, rect.Top),
                    new DPoint(rect.Left, rect.Bottom),
                    new DPoint(rect.Right, rect.Bottom),
                };

                var width = (int)rect.Width;
                var height = (int)rect.Height;

                switch (image.Mode)
                {
                    case Mode.Rgb:
                        var rgb = image.Matrix as Matrix<RgbPixel>;
                        results.Add(new Image(DlibDotNet.Dlib.ExtractImage4Points(rgb, dPoint, width, height),
                                              Mode.Rgb));
                        break;
                    case Mode.Greyscale:
                        var gray = image.Matrix as Matrix<byte>;
                        results.Add(new Image(DlibDotNet.Dlib.ExtractImage4Points(gray, dPoint, width, height),
                                              Mode.Greyscale));
                        break;
                }
            }

            return results;
        }

        /// <summary>
        /// Detects the values whether human eye's blink or not from face landmark.
        /// </summary>
        /// <param name="landmark">The dictionary of face parts locations (eyes, nose, etc).</param>
        /// <param name="leftBlink">When this method returns, contains <value>true</value>, if the left eye blinks; otherwise, <value>false</value>.</param>
        /// <param name="rightBlink">When this method returns, contains <value>true</value>, if the right eye blinks; otherwise, <value>false</value>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="landmark"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="landmark"/> does not contain <see cref="FacePart.LeftEye"/> or <see cref="FacePart.RightEye"/>.</exception>
        /// <exception cref="NotSupportedException">The custom eye blink detector is not ready.</exception>
        /// <exception cref="ObjectDisposedException">This object or custom eye blink detector is disposed.</exception>
        public void EyeBlinkDetect(IDictionary<FacePart, IEnumerable<FacePoint>> landmark, out bool leftBlink, out bool rightBlink)
        {
            this.ThrowIfDisposed();

            if (this._CustomEyeBlinkDetector == null)
                throw new NotSupportedException("The custom eye blink detector is not ready.");

            if (this._CustomEyeBlinkDetector.IsDisposed)
                throw new ObjectDisposedException($"{nameof(CustomEyeBlinkDetector)}", "The custom eye blink detector is disposed.");

            this._CustomEyeBlinkDetector.Detect(landmark, out leftBlink, out rightBlink);
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

            var results = new List<double>();
            if (array.Length == 0)
                return results;

            foreach (var faceEncoding in array)
                using (var diff = faceEncoding.Encoding - faceToCompare.Encoding)
                    results.Add(DlibDotNet.Dlib.Length(diff));

            return results;
        }

        /// <summary>
        /// Returns an enumerable collection of face feature data corresponds to all faces in specified image.
        /// </summary>
        /// <param name="image">The image contains faces. The image can contain multiple faces.</param>
        /// <param name="knownFaceLocation">The enumerable collection of location rectangle for faces. If specified null, method will find face locations.</param>
        /// <param name="numJitters">The number of times to re-sample the face when calculating encoding.</param>
        /// <param name="predictorModel">The dimension of vector which be returned from detector.</param>
        /// <param name="model">The model of face detector to detect in image. If <paramref name="knownFaceLocation"/> is not null, this value is ignored.</param>
        /// <returns>An enumerable collection of face feature data corresponds to all faces in specified image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="knownFaceLocation"/> contains no elements.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> or this object or custom face landmark detector is disposed.</exception>
        /// <exception cref="NotSupportedException"><see cref="PredictorModel.Custom"/> is not supported.</exception>
        public IEnumerable<FaceEncoding> FaceEncodings(Image image,
                                                       IEnumerable<Location> knownFaceLocation = null,
                                                       int numJitters = 1,
                                                       PredictorModel predictorModel = PredictorModel.Small,
                                                       Model model = Model.Hog)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (predictorModel == PredictorModel.Custom)
                throw new NotSupportedException("FaceRecognitionDotNet.PredictorModel.Custom is not supported.");

            if (knownFaceLocation != null && !knownFaceLocation.Any())
                throw new InvalidOperationException($"{nameof(knownFaceLocation)} contains no elements.");

            image.ThrowIfDisposed();
            this.ThrowIfDisposed();

            var rawLandmarks = this.RawFaceLandmarks(image, knownFaceLocation, predictorModel, model);

            var results = new List<FaceEncoding>();
            foreach (var landmark in rawLandmarks)
            {
                var ret = new FaceEncoding(FaceRecognitionModelV1.ComputeFaceDescriptor(this._FaceEncoder, image, landmark, numJitters));
                landmark.Dispose();
                results.Add(ret);
            }

            return results;
        }

        /// <summary>
        /// Returns an enumerable collection of dictionary of face parts locations (eyes, nose, etc) for each face in the image.
        /// </summary>
        /// <param name="faceImage">The image contains faces. The image can contain multiple faces.</param>
        /// <param name="faceLocations">The enumerable collection of location rectangle for faces. If specified null, method will find face locations.</param>
        /// <param name="predictorModel">The dimension of vector which be returned from detector.</param>
        /// <param name="model">The model of face detector to detect in image. If <paramref name="faceLocations"/> is not null, this value is ignored.</param>
        /// <returns>An enumerable collection of dictionary of face parts locations (eyes, nose, etc).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="faceImage"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="faceLocations"/> contains no elements.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="faceImage"/> or this object or custom face landmark detector is disposed.</exception>
        /// <exception cref="NotSupportedException">The custom face landmark detector is not ready.</exception>
        public IEnumerable<IDictionary<FacePart, IEnumerable<FacePoint>>> FaceLandmark(Image faceImage,
                                                                                       IEnumerable<Location> faceLocations = null,
                                                                                       PredictorModel predictorModel = PredictorModel.Large,
                                                                                       Model model = Model.Hog)
        {
            if (faceImage == null)
                throw new ArgumentNullException(nameof(faceImage));

            if (faceLocations != null && !faceLocations.Any())
                throw new InvalidOperationException($"{nameof(faceLocations)} contains no elements.");

            faceImage.ThrowIfDisposed();
            this.ThrowIfDisposed();

            if (predictorModel == PredictorModel.Custom)
            {
                if (this._CustomFaceLandmarkDetector == null)
                    throw new NotSupportedException("The custom face landmark detector is not ready.");

                if (this._CustomFaceLandmarkDetector.IsDisposed)
                    throw new ObjectDisposedException($"{nameof(CustomFaceLandmarkDetector)}", "The custom face landmark detector is disposed.");
            }

            var landmarks = this.RawFaceLandmarks(faceImage, faceLocations, predictorModel, model).ToArray();
            var landmarkTuples = landmarks.Select(landmark => Enumerable.Range(0, (int)landmark.Parts)
                                          .Select(index => new FacePoint(new Point(landmark.GetPart((uint)index)), index)).ToArray());

            var results = new List<Dictionary<FacePart, IEnumerable<FacePoint>>>();

            try
            {

                // For a definition of each point index, see https://cdn-images-1.medium.com/max/1600/1*AbEg31EgkbXSQehuNJBlWg.png
                switch (predictorModel)
                {
                    case PredictorModel.Large:
                        results.AddRange(landmarkTuples.Select(landmarkTuple => new Dictionary<FacePart, IEnumerable<FacePoint>>
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
                        results.AddRange(landmarkTuples.Select(landmarkTuple => new Dictionary<FacePart, IEnumerable<FacePoint>>
                        {
                            { FacePart.NoseTip,  Enumerable.Range(4,1).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.LeftEye,  Enumerable.Range(2,2).Select(i => landmarkTuple[i]).ToArray() },
                            { FacePart.RightEye, Enumerable.Range(0,2).Select(i => landmarkTuple[i]).ToArray() }
                        }));
                        break;
                    case PredictorModel.Custom:
                        results.AddRange(this._CustomFaceLandmarkDetector.GetLandmarks(landmarkTuples));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(predictorModel), predictorModel, null);
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

            var results = new List<Location>();
            foreach (var face in this.RawFaceLocations(image, numberOfTimesToUpsample, model))
            {
                var ret = TrimBound(face.Rect, image.Width, image.Height);
                var confidence = face.DetectionConfidence;
                face.Dispose();
                results.Add(new Location(ret, confidence));
            }

            return results;
        }

        /// <summary>
        /// Creates an <see cref="FaceEncoding"/> from the <see cref="double"/> array.
        /// </summary>
        /// <param name="encoding">The <see cref="double"/> array contains face encoding data.</param>
        /// <returns>The <see cref="FaceEncoding"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="encoding"/> must be 128.</exception>
        public static FaceEncoding LoadFaceEncoding(double[] encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (encoding.Length != 128)
                throw new ArgumentOutOfRangeException($"{nameof(encoding)}.{nameof(encoding.Length)} must be 128.");

            var matrix = Matrix<double>.CreateTemplateParameterizeMatrix(0, 1);
            matrix.SetSize(128);
            matrix.Assign(encoding);
            return new FaceEncoding(matrix);
        }

        /// <summary>
        /// Creates an <see cref="Image"/> from the specified existing bitmap image.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> from which to create the new <see cref="Image"/>.</param>
        /// <returns>The <see cref="Image"/> this method creates.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified <see cref="PixelFormat"/> is not supported.</exception>
        public static Image LoadImage(Bitmap bitmap)
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
        public static Image LoadImage(byte[] array, int row, int column, int stride, Mode mode)
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
        public static Image LoadImage(IntPtr array, int row, int column, int stride, Mode mode)
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

        /// <summary>
        /// Returns an index of age group of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="image">The image contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An index of age group of face image correspond to specified location in specified image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="location"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> or this object or custom age estimator is disposed.</exception>
        /// <exception cref="NotSupportedException">The custom age estimator is not ready.</exception>
        public uint PredictAge(Image image, Location location)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            image.ThrowIfDisposed();
            this.ThrowIfDisposed();

            if (this._CustomAgeEstimator == null)
                throw new NotSupportedException("The custom age estimator is not ready.");

            if (this._CustomAgeEstimator.IsDisposed)
                throw new ObjectDisposedException($"{nameof(CustomAgeEstimator)}", "The custom age estimator is disposed.");

            return this._CustomAgeEstimator.Predict(image, location);
        }

        /// <summary>
        /// Returns an emotion of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="image">The image contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An emotion of face image correspond to specified location in specified image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="location"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> or this object or custom emotion estimator is disposed.</exception>
        /// <exception cref="NotSupportedException">The custom emotion estimator is not ready.</exception>
        public string PredictEmotion(Image image, Location location)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            image.ThrowIfDisposed();
            this.ThrowIfDisposed();

            if (this._CustomEmotionEstimator == null)
                throw new NotSupportedException("The custom emotion estimator is not ready.");

            if (this._CustomEmotionEstimator.IsDisposed)
                throw new ObjectDisposedException($"{nameof(CustomEmotionEstimator)}", "The custom emotion estimator is disposed.");

            return this._CustomEmotionEstimator.Predict(image, location);
        }

        /// <summary>
        /// Returns an gender of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="image">The image contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>An gender of face image correspond to specified location in specified image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="location"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> or this object or custom gender estimator is disposed.</exception>
        /// <exception cref="NotSupportedException">The custom gender estimator is not ready.</exception>
        public Gender PredictGender(Image image, Location location)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            image.ThrowIfDisposed();
            this.ThrowIfDisposed();

            if (this._CustomGenderEstimator == null)
                throw new NotSupportedException("The custom gender estimator is not ready.");

            if (this._CustomGenderEstimator.IsDisposed)
                throw new ObjectDisposedException($"{nameof(CustomGenderEstimator)}", "The custom gender estimator is disposed.");

            return this._CustomGenderEstimator.Predict(image, location);
        }

        /// <summary>
        /// Returns probabilities of age group of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="image">The image contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>Probabilities of age group of face image correspond to specified location in specified image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="location"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> or this object or custom age estimator is disposed.</exception>
        /// <exception cref="NotSupportedException">The custom age estimator is not ready.</exception>
        public IDictionary<uint, float> PredictProbabilityAge(Image image, Location location)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            image.ThrowIfDisposed();
            this.ThrowIfDisposed();

            if (this._CustomAgeEstimator == null)
                throw new NotSupportedException("The custom age estimator is not ready.");

            if (this._CustomAgeEstimator.IsDisposed)
                throw new ObjectDisposedException($"{nameof(CustomAgeEstimator)}", "The custom age estimator is disposed.");

            return this._CustomAgeEstimator.PredictProbability(image, location);
        }

        /// <summary>
        /// Returns probabilities of emotion of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="image">The image contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>Probabilities of emotion of face image correspond to specified location in specified image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="location"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> or this object or custom emotion estimator is disposed.</exception>
        /// <exception cref="NotSupportedException">The custom emotion estimator is not ready.</exception>
        public IDictionary<string, float> PredictProbabilityEmotion(Image image, Location location)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            image.ThrowIfDisposed();
            this.ThrowIfDisposed();

            if (this._CustomEmotionEstimator == null)
                throw new NotSupportedException("The custom emotion estimator is not ready.");

            if (this._CustomEmotionEstimator.IsDisposed)
                throw new ObjectDisposedException($"{nameof(CustomEmotionEstimator)}", "The custom emotion estimator is disposed.");

            return this._CustomEmotionEstimator.PredictProbability(image, location);
        }

        /// <summary>
        /// Returns probabilities of gender of face image correspond to specified location in specified image.
        /// </summary>
        /// <param name="image">The image contains a face.</param>
        /// <param name="location">The location rectangle for a face.</param>
        /// <returns>Probabilities of gender of face image correspond to specified location in specified image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> or <paramref name="location"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="image"/> or this object or custom gender estimator is disposed.</exception>
        /// <exception cref="NotSupportedException">The custom gender estimator is not ready.</exception>
        public IDictionary<Gender, float> PredictProbabilityGender(Image image, Location location)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            image.ThrowIfDisposed();
            this.ThrowIfDisposed();

            if (this._CustomGenderEstimator == null)
                throw new NotSupportedException("The custom gender estimator is not ready.");

            if (this._CustomGenderEstimator.IsDisposed)
                throw new ObjectDisposedException($"{nameof(CustomGenderEstimator)}", "The custom gender estimator is disposed.");

            return this._CustomGenderEstimator.PredictProbability(image, location);
        }

        /// <summary>
        /// Returns a head pose estimated from face parts locations.
        /// </summary>
        /// <param name="landmark">The dictionary of face parts locations (eyes, nose, etc).</param>
        /// <returns>A head pose estimated from face parts locations.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="landmark"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">This object or custom head pose estimator is disposed.</exception>
        /// <exception cref="NotSupportedException">The custom head pose estimator is not ready.</exception>
        public HeadPose PredictHeadPose(IDictionary<FacePart, IEnumerable<FacePoint>> landmark)
        {
            if (landmark == null)
                throw new ArgumentNullException(nameof(landmark));

            this.ThrowIfDisposed();

            if (this._CustomHeadPoseEstimator == null)
                throw new NotSupportedException("The custom head pose estimator is not ready.");

            if (this._CustomHeadPoseEstimator.IsDisposed)
                throw new ObjectDisposedException($"{nameof(CustomHeadPoseEstimator)}", "The custom head pose estimator is disposed.");

            return this._CustomHeadPoseEstimator.Predict(landmark);
        }

        #region Helpers

        private IEnumerable<FullObjectDetection> RawFaceLandmarks(Image faceImage,
                                                                  IEnumerable<Location> faceLocations = null,
                                                                  PredictorModel predictorModel = PredictorModel.Large,
                                                                  Model model = Model.Hog)
        {
            IEnumerable<Location> rects;

            if (faceLocations == null)
            {
                var list = new List<Location>();
                var tmp = this.RawFaceLocations(faceImage, 1, model);
                foreach (var rect in tmp)
                {
                    list.Add(new Location(rect.Rect, rect.DetectionConfidence));
                    rect.Dispose();
                }

                rects = list;
            }
            else
            {
                rects = faceLocations;
            }

            var results = new List<FullObjectDetection>();
            if (predictorModel == PredictorModel.Custom)
            {
                foreach (var rect in rects)
                {
                    var ret = this._CustomFaceLandmarkDetector.Detect(faceImage, rect);
                    results.Add(ret);
                }
            }
            else
            {
                var posePredictor = this._PosePredictor68Point;
                switch (predictorModel)
                {
                    case PredictorModel.Small:
                        posePredictor = this._PosePredictor5Point;
                        break;
                }

                foreach (var rect in rects)
                {
                    var ret = posePredictor.Detect(faceImage.Matrix, new Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom));
                    results.Add(ret);
                }
            }

            return results;
        }

        private IEnumerable<MModRect> RawFaceLocations(Image faceImage, int numberOfTimesToUpsample = 1, Model model = Model.Hog)
        {
            switch (model)
            {
                case Model.Custom:
                    if (this._CustomFaceDetector == null)
                        throw new NotSupportedException("The custom face detector is not ready.");
                    return this._CustomFaceDetector.Detect(faceImage, numberOfTimesToUpsample).Select(rect => new MModRect
                    {
                        Rect = new Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom),
                        DetectionConfidence = rect.Confidence
                    });
                case Model.Cnn:
                    return CnnFaceDetectionModelV1.Detect(this._CnnFaceDetector, faceImage, numberOfTimesToUpsample);
                default:
                    var locations = SimpleObjectDetector.RunDetectorWithUpscale2(this._FaceDetector, faceImage, (uint)numberOfTimesToUpsample);
                    return locations.Select(tuple => new MModRect { Rect = tuple.Item1, DetectionConfidence = tuple.Item2 });
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
