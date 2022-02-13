using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using DlibDotNet;
using FaceRecognitionDotNet.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace FaceRecognitionDotNet.Tests
{

    public class FaceRecognitionTest : IDisposable
    {

        #region Fields

        private readonly ITestOutputHelper _TestOutputHelper;

        private FaceRecognition _FaceRecognition;

        private const string TestImageDirectory = "TestImages";

        private readonly string ModelDirectory = "Models";

        private const string ModelTempDirectory = "TempModels";

        private readonly IList<string> ModelFiles = new List<string>();

        private const string ModelBaseUrl = "https://github.com/ageitgey/face_recognition_models/raw/master/face_recognition_models/models";

        private const string ResultDirectory = "Result";

        private const string TwoPersonFile = "419px-Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

        private readonly string _HelenModelFile;

        private readonly string _AgeEstimatorModelFile;

        private readonly string _EmotionEstimatorModelFile;

        private readonly string _GenderEstimatorModelFile;

        private readonly string _RollEstimateorModelFile;

        private readonly string _PitchEstimateorModelFile;

        private readonly string _YawEstimateorModelFile;

        private readonly string _SimpleFaceDetectorModelFile;

        private readonly string _PosePredictor68PointModelFile;

        #endregion

        #region Constructors

        public FaceRecognitionTest(ITestOutputHelper testOutputHelper)
        {
            this._TestOutputHelper = testOutputHelper;

            var dir = Environment.GetEnvironmentVariable("FaceRecognitionDotNetModelDir");
            if (Directory.Exists(dir))
            {
                ModelDirectory = dir;
            }

            this._RollEstimateorModelFile = Path.Combine(ModelDirectory, "300w-lp-roll-krls_0.001_0.1.dat");
            this._PitchEstimateorModelFile = Path.Combine(ModelDirectory, "300w-lp-pitch-krls_0.001_0.1.dat");
            this._YawEstimateorModelFile = Path.Combine(ModelDirectory, "300w-lp-yaw-krls_0.001_0.1.dat");
            this._SimpleFaceDetectorModelFile = Path.Combine(ModelDirectory, "face_detector.svm");
            this._PosePredictor68PointModelFile = Path.Combine(ModelDirectory, "shape_predictor_68_face_landmarks.dat");

            var faceRecognition = typeof(FaceRecognition);
            var type = faceRecognition.Assembly.GetTypes().FirstOrDefault(t => t.Name == "FaceRecognitionModels");
            if (type == null)
                Assert.True(false, "FaceRecognition.FaceRecognitionModels is not found.");

            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

            var models = new List<string>();
            foreach (var method in methods)
            {
                var result = method.Invoke(null, BindingFlags.Public | BindingFlags.Static, null, null, null) as string;
                if (string.IsNullOrWhiteSpace(result))
                    Assert.True(false, $"{method.Name} does not return {typeof(string).FullName} value or return null or whitespace value.");

                switch (method.Name)
                {
                    case "GetPosePredictor194PointModelLocation":
                        this._HelenModelFile = Path.Combine(ModelDirectory, result);
                        break;
                    case "GetEmotionNetworkModelLocation":
                        this._EmotionEstimatorModelFile = Path.Combine(ModelDirectory, result);
                        break;
                    case "GetGenderNetworkModelLocation":
                        this._GenderEstimatorModelFile = Path.Combine(ModelDirectory, result);
                        break;
                    case "GetAgeNetworkModelLocation":
                        this._AgeEstimatorModelFile = Path.Combine(ModelDirectory, result);
                        break;
                    default:
                        models.Add(result);

                        var path = Path.Combine(ModelDirectory, result);
                        if (File.Exists(path))
                            continue;

                        var binary = new HttpClient().GetByteArrayAsync($"{ModelBaseUrl}/{result}").Result;
                        Directory.CreateDirectory(ModelDirectory);
                        File.WriteAllBytes(path, binary);
                        break;
                }
            }

            foreach (var model in models)
                this.ModelFiles.Add(model);

            this._FaceRecognition = FaceRecognition.Create(ModelDirectory);
        }

        #endregion

        #region Methods

        [Fact]
        public void CompareFacesFalse()
        {
            var bidenFile = "480px-Biden_2013.jpg";
            var obamaFile = "480px-President_Barack_Obama.jpg";

            var path1 = Path.Combine(TestImageDirectory, bidenFile);
            var path2 = Path.Combine(TestImageDirectory, obamaFile);

            bool atLeast1Time = false;

            using (var image1 = FaceRecognition.LoadImageFile(path1))
            using (var image2 = FaceRecognition.LoadImageFile(path2))
            {
                foreach (var numJitters in new[] { 1, 2 })
                    foreach (var model in Enum.GetValues(typeof(PredictorModel)).Cast<PredictorModel>())
                    {
                        if (model == PredictorModel.Custom)
                            continue;

                        var encodings1 = this._FaceRecognition.FaceEncodings(image1, null, numJitters, model).ToArray();
                        var encodings2 = this._FaceRecognition.FaceEncodings(image2, null, numJitters, model).ToArray();

                        foreach (var encoding in encodings1)
                            foreach (var compareFace in FaceRecognition.CompareFaces(encodings2, encoding))
                            {
                                atLeast1Time = true;
                                Assert.False(compareFace, $"{nameof(numJitters)}: {numJitters}");
                            }

                        foreach (var encoding in encodings1)
                            encoding.Dispose();
                        foreach (var encoding in encodings2)
                            encoding.Dispose();
                    }
            }

            if (!atLeast1Time)
                Assert.True(false, "Assert check did not execute");
        }

        [Fact]
        public void CompareFacesTrue()
        {
            var obamaFile1 = "Barack_Obama_addresses_LULAC_7-8-08.JPG";
            var obamaFile2 = "480px-President_Barack_Obama.jpg";

            var path1 = Path.Combine(TestImageDirectory, obamaFile1);
            var path2 = Path.Combine(TestImageDirectory, obamaFile2);

            bool atLeast1Time = false;

            using (var image1 = FaceRecognition.LoadImageFile(path1))
            using (var image2 = FaceRecognition.LoadImageFile(path2))
            {
                foreach (var numJitters in new[] { 1, 2 })
                    foreach (var model in Enum.GetValues(typeof(PredictorModel)).Cast<PredictorModel>())
                    {
                        if (model == PredictorModel.Custom)
                            continue;

                        var endodings1 = this._FaceRecognition.FaceEncodings(image1, null, numJitters, model).ToArray();
                        var endodings2 = this._FaceRecognition.FaceEncodings(image2, null, numJitters, model).ToArray();

                        foreach (var encoding in endodings1)
                            foreach (var compareFace in FaceRecognition.CompareFaces(endodings2, encoding))
                            {
                                atLeast1Time = true;
                                Assert.True(compareFace, $"{nameof(numJitters)}: {numJitters}");
                            }

                        foreach (var encoding in endodings1)
                            encoding.Dispose();
                        foreach (var encoding in endodings2)
                            encoding.Dispose();
                    }
            }

            if (!atLeast1Time)
                Assert.True(false, "Assert check did not execute");
        }

        [Fact]
        public void CropFaces()
        {
            const string testName = nameof(this.CropFaces);

            var path = Path.Combine(TestImageDirectory, TwoPersonFile);

            foreach (var mode in new[] { Mode.Rgb, Mode.Greyscale })
            {
                using (var image = FaceRecognition.LoadImageFile(path, mode))
                {
                    var locations = this._FaceRecognition.FaceLocations(image).ToArray();
                    Assert.True(locations.Length == 2, $"{mode}");

                    var images = FaceRecognition.CropFaces(image, locations).ToArray();
                    for (var index = 1; index <= images.Length; index++)
                    {
                        var croppedImage = images[index - 1];

                        var directory = Path.Combine(ResultDirectory, testName);
                        Directory.CreateDirectory(directory);

                        var dst = Path.Combine(directory, $"{mode}-{index}.jpg");
                        croppedImage.Save(dst, ImageFormat.Jpeg);

                        croppedImage.Dispose();
                    }
                }
            }
        }

        [Fact]
        public void CropFacesException()
        {
            try
            {
                _ = FaceRecognition.CropFaces(null, new Location[0]).ToArray();
                Assert.True(false, $"{nameof(FaceRecognition.CropFaces)} method should throw exception.");
            }
            catch (ArgumentNullException)
            {
            }

            var path = Path.Combine(TestImageDirectory, TwoPersonFile);

            try
            {
                using (var image = FaceRecognition.LoadImageFile(path))
                    _ = FaceRecognition.CropFaces(image, null).ToArray();
                Assert.True(false, $"{nameof(FaceRecognition.CropFaces)} method should throw exception.");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [Fact]
        public void CustomFaceDetector()
        {
            if (!File.Exists(this._SimpleFaceDetectorModelFile))
                return;

            try
            {
                using (var detector = new SimpleFaceDetector(this._SimpleFaceDetectorModelFile))
                {
                    this._FaceRecognition.CustomFaceDetector = detector;
                    Assert.Equal(this._FaceRecognition.CustomFaceDetector, detector);

                    var groundTruth = new[]
                    {
                        new { Path = Path.Combine(TestImageDirectory, "obama.jpg"), Model = Model.Cnn,    Confidence = 1.1056d, Bottom = 379, Left = 354, Right = 598, Top = 134 },
                        new { Path = Path.Combine(TestImageDirectory, "obama.jpg"), Model = Model.Hog,    Confidence = 1.9854d, Bottom = 409, Left = 349, Right = 617, Top = 142 },
                        new { Path = Path.Combine(TestImageDirectory, "obama.jpg"), Model = Model.Custom, Confidence = 1.4475d, Bottom = 394, Left = 366, Right = 624, Top = 136 }
                    };

                    foreach (var gt in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(gt.Path))
                        {
                            var location = this._FaceRecognition.FaceLocations(image, 1, gt.Model).ToArray()[0];
                            Assert.True(Math.Abs(gt.Confidence - location.Confidence) < 0.0001d, $"Failed to calc confidence '{gt.Path}'");
                            Assert.True(gt.Bottom == location.Bottom, $"Failed to get Bottom '{gt.Path}'");
                            Assert.True(gt.Left == location.Left, $"Failed to get Left '{gt.Path}'");
                            Assert.True(gt.Right == location.Right, $"Failed to get Right '{gt.Path}'");
                            Assert.True(gt.Top == location.Top, $"Failed to get Top '{gt.Path}'");
                        }
                }
            }
            finally
            {
                this._FaceRecognition.CustomAgeEstimator = null;
            }
        }

        [Fact]
        public void CustomFaceDetectorException()
        {
            try
            {
                new SimpleFaceDetector("not_found");
                Assert.True(false, $"{nameof(SimpleFaceDetector)} method should throw exception.");
            }
            catch (FileNotFoundException)
            {
            }
        }

        [Fact]
        public void Create()
        {
            var type = typeof(FaceRecognition).Assembly.GetTypes().FirstOrDefault(t => t.Name == "FaceRecognitionModels");
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

            var modelParameter = new ModelParameter();
            foreach (var method in methods)
            {
                var result = method.Invoke(null, BindingFlags.Public | BindingFlags.Static, null, null, null) as string;
                if (string.IsNullOrWhiteSpace(result))
                    Assert.True(false, $"{method.Name} does not return {typeof(string).FullName} value or return null or whitespace value.");

                switch (method.Name)
                {
                    case "GetPosePredictorModelLocation":
                        modelParameter.PosePredictor68FaceLandmarksModel = File.ReadAllBytes(Path.Combine(ModelDirectory, result));
                        break;
                    case "GetPosePredictorFivePointModelLocation":
                        modelParameter.PosePredictor5FaceLandmarksModel = File.ReadAllBytes(Path.Combine(ModelDirectory, result));
                        break;
                    case "GetFaceRecognitionModelLocation":
                        modelParameter.FaceRecognitionModel = File.ReadAllBytes(Path.Combine(ModelDirectory, result));
                        break;
                    case "GetCnnFaceDetectorModelLocation":
                        modelParameter.CnnFaceDetectorModel = File.ReadAllBytes(Path.Combine(ModelDirectory, result));
                        break;
                }
            }

            var fr = FaceRecognition.Create(modelParameter);
            fr.Dispose();
        }

        [Fact]
        public void CreateFail1()
        {
            Directory.CreateDirectory(ModelTempDirectory);

            var array = this.ModelFiles.ToArray();
            for (var j = 0; j < array.Length; j++)
            {
                // Remove all files
                foreach (var file in array)
                {
                    var path = Path.Combine(ModelTempDirectory, file);
                    if (File.Exists(path))
                        File.Delete(path);
                }

                for (var i = 0; i < array.Length; i++)
                {
                    if (i == j)
                        continue;

                    File.Copy(Path.Combine(ModelDirectory, array[i]), Path.Combine(ModelTempDirectory, array[i]));
                }

                FaceRecognition faceRecognition = null;
                try
                {
                    faceRecognition = FaceRecognition.Create(ModelTempDirectory);
                    Assert.True(false, $"{Path.Combine(ModelTempDirectory, array[j])} is missing and Create method should throw exception.");
                }
                catch (FileNotFoundException)
                {
                }
                finally
                {
                    faceRecognition?.Dispose();
                }
            }
        }

        [Fact]
        public void CreateFail2()
        {
            var tempModelDirectory = "Temp";
            FaceRecognition faceRecognition = null;

            try
            {
                faceRecognition = FaceRecognition.Create(tempModelDirectory);
                Assert.True(false, $"{tempModelDirectory} directory is missing and Create method should throw exception.");
            }
            catch (DirectoryNotFoundException)
            {
            }
            finally
            {
                faceRecognition?.Dispose();
            }
        }

        [Fact]
        public void CreateFail3()
        {
            var type = typeof(FaceRecognition).Assembly.GetTypes().FirstOrDefault(t => t.Name == "FaceRecognitionModels");
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

            var modelParameter = new ModelParameter();
            foreach (var method in methods)
            {
                var result = method.Invoke(null, BindingFlags.Public | BindingFlags.Static, null, null, null) as string;
                if (string.IsNullOrWhiteSpace(result))
                    Assert.True(false, $"{method.Name} does not return {typeof(string).FullName} value or return null or whitespace value.");

                switch (method.Name)
                {
                    case "GetPosePredictorModelLocation":
                        modelParameter.PosePredictor68FaceLandmarksModel = File.ReadAllBytes(Path.Combine(ModelDirectory, result));
                        break;
                    case "GetPosePredictorFivePointModelLocation":
                        modelParameter.PosePredictor5FaceLandmarksModel = File.ReadAllBytes(Path.Combine(ModelDirectory, result));
                        break;
                    case "GetFaceRecognitionModelLocation":
                        modelParameter.FaceRecognitionModel = File.ReadAllBytes(Path.Combine(ModelDirectory, result));
                        break;
                    case "GetCnnFaceDetectorModelLocation":
                        modelParameter.CnnFaceDetectorModel = File.ReadAllBytes(Path.Combine(ModelDirectory, result));
                        break;
                }
            }

            try
            {
                ModelParameter tmp = null;
                FaceRecognition.Create(tmp);
                Assert.True(false, $"{nameof(FaceRecognition.Create)} should throw {nameof(ArgumentNullException)}");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                FaceRecognition.Create(new ModelParameter
                {
                    PosePredictor5FaceLandmarksModel = null,
                    PosePredictor68FaceLandmarksModel = modelParameter.PosePredictor68FaceLandmarksModel,
                    FaceRecognitionModel = modelParameter.FaceRecognitionModel,
                    CnnFaceDetectorModel = modelParameter.CnnFaceDetectorModel,
                });
                Assert.True(false, $"{nameof(modelParameter.PosePredictor5FaceLandmarksModel)} should throw {nameof(NullReferenceException)}");
            }
            catch (NullReferenceException)
            {
            }

            try
            {
                FaceRecognition.Create(new ModelParameter
                {
                    PosePredictor5FaceLandmarksModel = modelParameter.PosePredictor5FaceLandmarksModel,
                    PosePredictor68FaceLandmarksModel = null,
                    FaceRecognitionModel = modelParameter.FaceRecognitionModel,
                    CnnFaceDetectorModel = modelParameter.CnnFaceDetectorModel,
                });
                Assert.True(false, $"{nameof(modelParameter.PosePredictor68FaceLandmarksModel)} should throw {nameof(NullReferenceException)}");
            }
            catch (NullReferenceException)
            {
            }

            try
            {
                FaceRecognition.Create(new ModelParameter
                {
                    PosePredictor5FaceLandmarksModel = modelParameter.PosePredictor5FaceLandmarksModel,
                    PosePredictor68FaceLandmarksModel = modelParameter.PosePredictor68FaceLandmarksModel,
                    FaceRecognitionModel = null,
                    CnnFaceDetectorModel = modelParameter.CnnFaceDetectorModel,
                });
                Assert.True(false, $"{nameof(modelParameter.FaceRecognitionModel)} should throw {nameof(NullReferenceException)}");
            }
            catch (NullReferenceException)
            {
            }

            try
            {
                FaceRecognition.Create(new ModelParameter
                {
                    PosePredictor5FaceLandmarksModel = modelParameter.PosePredictor5FaceLandmarksModel,
                    PosePredictor68FaceLandmarksModel = modelParameter.PosePredictor68FaceLandmarksModel,
                    FaceRecognitionModel = modelParameter.FaceRecognitionModel,
                    CnnFaceDetectorModel = null,
                });
                Assert.True(false, $"{nameof(modelParameter.CnnFaceDetectorModel)} should throw {nameof(NullReferenceException)}");
            }
            catch (NullReferenceException)
            {
            }
        }

        [Fact]
        public void Encoding()
        {
            try
            {
                FaceRecognition.InternalEncoding = System.Text.Encoding.ASCII;
                Assert.Equal(FaceRecognition.InternalEncoding, System.Text.Encoding.ASCII);

                FaceRecognition.InternalEncoding = System.Text.Encoding.UTF8;
                Assert.Equal(FaceRecognition.InternalEncoding, System.Text.Encoding.UTF8);
            }
            finally
            {
                FaceRecognition.InternalEncoding = null;
            }
        }

        [Fact]
        public void EyeBlinkLargeDetect()
        {
            using (var detector = new EyeAspectRatioLargeEyeBlinkDetector(0.2, 0.2))
                this.EyeBlinkDetect(detector, PredictorModel.Large);
        }

        [Fact]
        public void EyeBlinkLargeDetectException()
        {
            try
            {
                using (var detector = new EyeAspectRatioLargeEyeBlinkDetector(0.2, 0.2))
                {
                    this._FaceRecognition.CustomEyeBlinkDetector = detector;
                    this._FaceRecognition.EyeBlinkDetect(null, out _, out _);
                }
                Assert.True(false, $"{nameof(FaceRecognition.EyeBlinkDetect)} method should throw exception.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                this._FaceRecognition.CustomEyeBlinkDetector = null;
                this._FaceRecognition.EyeBlinkDetect(null, out _, out _);
                Assert.True(false, $"{nameof(FaceRecognition.EyeBlinkDetect)} method should throw exception.");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                using (var detector = new EyeAspectRatioLargeEyeBlinkDetector(0.2, 0.2))
                {
                    this._FaceRecognition.CustomEyeBlinkDetector = detector;
                    detector.Dispose();
                    this._FaceRecognition.EyeBlinkDetect(null, out _, out _);
                }
                Assert.True(false, $"{nameof(FaceRecognition.EyeBlinkDetect)} method should throw exception.");
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        public void EyeBlinkHelenDetect()
        {
            if (!File.Exists(this._HelenModelFile))
                return;

            try
            {
                using (var faceLandmarkDetector = new HelenFaceLandmarkDetector(this._HelenModelFile))
                {
                    this._FaceRecognition.CustomFaceLandmarkDetector = faceLandmarkDetector;
                    Assert.Equal(this._FaceRecognition.CustomFaceLandmarkDetector, faceLandmarkDetector);

                    using (var detector = new EyeAspectRatioHelenEyeBlinkDetector(0.05, 0.05))
                        this.EyeBlinkDetect(detector, PredictorModel.Custom);
                }
            }
            finally
            {
                this._FaceRecognition.CustomFaceLandmarkDetector = null;
            }
        }

        [Fact]
        public void FaceDistance()
        {
            var bidenFile = "480px-Biden_2013.jpg";
            var obamaFile = "480px-President_Barack_Obama.jpg";

            var path1 = Path.Combine(TestImageDirectory, bidenFile);
            var path2 = Path.Combine(TestImageDirectory, obamaFile);

            bool atLeast1Time = false;

            foreach (var mode in new[] { Mode.Rgb, Mode.Greyscale })
            {
                using (var image1 = FaceRecognition.LoadImageFile(path1, mode))
                using (var image2 = FaceRecognition.LoadImageFile(path2, mode))
                {
                    var endodings1 = this._FaceRecognition.FaceEncodings(image1).ToArray();
                    var endodings2 = this._FaceRecognition.FaceEncodings(image2).ToArray();
                    Assert.True(endodings1.Length >= 1, $"{nameof(endodings1)} has {endodings1.Length} faces");
                    Assert.True(endodings2.Length >= 1, $"{nameof(endodings2)} has {endodings2.Length} faces");

                    foreach (var e1 in endodings1)
                        foreach (var e2 in endodings2)
                        {
                            atLeast1Time = true;
                            var distance = FaceRecognition.FaceDistance(e1, e2);
                            Assert.True(distance > 0.6d);
                        }

                    foreach (var encoding in endodings1)
                        encoding.Dispose();
                    foreach (var encoding in endodings2)
                        encoding.Dispose();
                }
            }

            if (!atLeast1Time)
                Assert.True(false, "Assert check did not execute");
        }

        [Fact]
        public void FaceDistanceDeserialized()
        {
            var bidenFile = "480px-Biden_2013.jpg";
            var obamaFile = "480px-President_Barack_Obama.jpg";

            var path1 = Path.Combine(TestImageDirectory, bidenFile);
            var path2 = Path.Combine(TestImageDirectory, obamaFile);

            bool atLeast1Time = false;

            foreach (var mode in new[] { Mode.Rgb, Mode.Greyscale })
            {
                using (var image1 = FaceRecognition.LoadImageFile(path1, mode))
                using (var image2 = FaceRecognition.LoadImageFile(path2, mode))
                {
                    var endodings1 = this._FaceRecognition.FaceEncodings(image1).ToArray();
                    var endodings2 = this._FaceRecognition.FaceEncodings(image2).ToArray();
                    Assert.True(endodings1.Length >= 1, $"{nameof(endodings1)} has {endodings1.Length} faces");
                    Assert.True(endodings2.Length >= 1, $"{nameof(endodings2)} has {endodings2.Length} faces");

                    foreach (var e1 in endodings1)
                        foreach (var e2 in endodings2)
                        {
                            atLeast1Time = true;
                            var distance = FaceRecognition.FaceDistance(e1, e2);
                            Assert.True(distance > 0.6d, $"distance should be greater than 0.6 but {distance}.");

                            var bf = new BinaryFormatter();
                            using (var ms1 = new MemoryStream())
                            {
                                bf.Serialize(ms1, e1);
                                ms1.Flush();

                                var array = ms1.ToArray();
                                using (var ms2 = new MemoryStream(array))
                                {
                                    var de1 = bf.Deserialize(ms2) as FaceEncoding;
                                    var distance2 = FaceRecognition.FaceDistance(de1, e2);
                                    Assert.True(Math.Abs(distance - distance2) < double.Epsilon);
                                }
                            }
                        }

                    foreach (var encoding in endodings1)
                        encoding.Dispose();
                    foreach (var encoding in endodings2)
                        encoding.Dispose();
                }
            }

            if (!atLeast1Time)
                Assert.True(false, "Assert check did not execute");
        }

        [Fact]
        public void FaceEncodings()
        {
            var path = Path.Combine(TestImageDirectory, TwoPersonFile);

            foreach (var mode in new[] { Mode.Rgb, Mode.Greyscale })
                foreach (var model in new[] { PredictorModel.Small, PredictorModel.Large })
                {
                    using (var image = FaceRecognition.LoadImageFile(path, mode))
                    {
                        var encodings = this._FaceRecognition.FaceEncodings(image, predictorModel: model).ToArray();
                        Assert.True(encodings.Length > 1, "");

                        foreach (var encoding in encodings)
                        {
                            encoding.Dispose();

                            try
                            {
                                encoding.Dispose();
                            }
                            catch
                            {
                                Assert.True(false, $"{typeof(FaceEncoding)} must not throw exception even though {nameof(FaceEncoding.Dispose)} method is called again.");
                            }

                            try
                            {
                                var _ = encoding.Size;
                                Assert.True(false, $"{nameof(FaceEncoding.Size)} must throw {typeof(ObjectDisposedException)} after object is disposed.");
                            }
                            catch
                            {
                            }
                        }

                        foreach (var encoding in encodings)
                            Assert.True(encoding.IsDisposed, $"{typeof(FaceEncoding)} should be already disposed.");
                    }
                }
        }

        [Fact]
        public void FaceEncodingsException()
        {
            try
            {
                var _ = this._FaceRecognition.FaceEncodings(null).ToArray();
                Assert.True(false, $"{nameof(FaceRecognition.FaceEncodings)} must throw {typeof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                var path = Path.Combine(TestImageDirectory, TwoPersonFile);

                using (var image = FaceRecognition.LoadImageFile(path))
                {
                    var _ = this._FaceRecognition.FaceEncodings(image, new Location[0]).ToArray();
                    Assert.True(false, $"{nameof(FaceRecognition.FaceEncodings)} must throw {typeof(InvalidOperationException)}.");
                }
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                var path = Path.Combine(TestImageDirectory, TwoPersonFile);

                using (var image = FaceRecognition.LoadImageFile(path))
                {
                    var _ = this._FaceRecognition.FaceEncodings(image, null, 1, PredictorModel.Custom).ToArray();
                    Assert.True(false, $"{nameof(FaceRecognition.FaceEncodings)} must throw {typeof(NotSupportedException)}.");
                }
            }
            catch (NotSupportedException)
            {
            }
        }

        [Fact]
        public void FaceLandmarkLarge()
        {
            const string testName = nameof(this.FaceLandmarkLarge);
            this.FaceLandmark(testName, PredictorModel.Large, true);
            this.FaceLandmark(testName, PredictorModel.Large, false);
        }

        [Fact]
        public void FaceLandmarkSmall()
        {
            const string testName = nameof(this.FaceLandmarkSmall);
            this.FaceLandmark(testName, PredictorModel.Small, true);
            this.FaceLandmark(testName, PredictorModel.Small, false);
        }

        [Fact]
        public void FaceLandmarkException()
        {
            try
            {
                var _ = this._FaceRecognition.FaceLandmark(null).ToArray();
                Assert.True(false, $"{nameof(FaceRecognition.FaceLandmark)} must throw {typeof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            var path = Path.Combine(TestImageDirectory, TwoPersonFile);

            try
            {

                using (var image = FaceRecognition.LoadImageFile(path))
                {
                    var _ = this._FaceRecognition.FaceLandmark(image, new Location[0]).ToArray();
                    Assert.True(false, $"{nameof(FaceRecognition.FaceLandmark)} must throw {typeof(InvalidOperationException)}.");
                }
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                using (var image = FaceRecognition.LoadImageFile(path))
                {
                    var _ = this._FaceRecognition.FaceLandmark(image, null, PredictorModel.Custom, Model.Cnn).ToArray();
                    Assert.True(false, $"{nameof(FaceRecognition.FaceLandmark)} must throw {typeof(NotSupportedException)}.");
                }
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                if (!File.Exists(this._HelenModelFile))
                    return;

                var faceLandmarkDetector = new HelenFaceLandmarkDetector(this._HelenModelFile);
                this._FaceRecognition.CustomFaceLandmarkDetector = faceLandmarkDetector;
                faceLandmarkDetector.Dispose();
                using (var image = FaceRecognition.LoadImageFile(path))
                {
                    var _ = this._FaceRecognition.FaceLandmark(image, null, PredictorModel.Custom, Model.Cnn).ToArray();
                    Assert.True(false, $"{nameof(FaceRecognition.FaceLandmark)} must throw {typeof(ObjectDisposedException)}.");
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        public void FaceLandmarkEmpty()
        {
            Path.Combine(TestImageDirectory, TwoPersonFile);

            // empty image should return empty result
            using (var bitmap = new Bitmap(640, 480, PixelFormat.Format24bppRgb))
            using (var image = FaceRecognition.LoadImage(bitmap))
            {
                var landmarks = this._FaceRecognition.FaceLandmark(image).ToArray();
                Assert.True(!landmarks.Any(), $"{nameof(FaceRecognition.FaceLandmark)} should return empty elements.");
            }
        }

        [Fact]
        public void FaceLandmarkHelen()
        {
            if (!File.Exists(this._HelenModelFile))
                return;

            const string testName = nameof(this.FaceLandmarkHelen);

            try
            {
                using (var detector = new HelenFaceLandmarkDetector(this._HelenModelFile))
                {
                    this._FaceRecognition.CustomFaceLandmarkDetector = detector;
                    Assert.Equal(this._FaceRecognition.CustomFaceLandmarkDetector, detector);

                    this.FaceLandmark(testName, PredictorModel.Custom, true);
                    this.FaceLandmark(testName, PredictorModel.Custom, false);
                }
            }
            finally
            {
                this._FaceRecognition.CustomFaceLandmarkDetector = null;
            }
        }

        [Fact]
        public void FaceLocationCnn()
        {
            const string testName = nameof(this.FaceLocationCnn);
            this.FaceLocation(testName, 2, Model.Cnn);
            this.FaceLocation(testName, 1, Model.Cnn);
            this.FaceLocation(testName, 0, Model.Cnn);
        }

        [Fact]
        public void FaceLocationHog()
        {
            const string testName = nameof(this.FaceLocationHog);
            this.FaceLocation(testName, 2, Model.Hog);
            this.FaceLocation(testName, 1, Model.Hog);
            this.FaceLocation(testName, 0, Model.Hog);
        }

        [Fact]
        public void FaceLocationsException()
        {
            try
            {
                var _ = this._FaceRecognition.FaceLocations(null).ToArray();
                Assert.True(false, $"{nameof(FaceRecognition.FaceLocations)} must throw {typeof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [Fact]
        public void LoadFaceEncoding()
        {
            var bidenFile = "480px-Biden_2013.jpg";
            var path1 = Path.Combine(TestImageDirectory, bidenFile);
            bool atLeast1Time = false;

            foreach (var mode in new[] { Mode.Rgb, Mode.Greyscale })
            {
                using (var image1 = FaceRecognition.LoadImageFile(path1, mode))
                {
                    var encodings = this._FaceRecognition.FaceEncodings(image1).ToArray();
                    foreach (var e1 in encodings)
                    {
                        atLeast1Time = true;

                        var fe = e1.GetRawEncoding();

                        using (var e2 = FaceRecognition.LoadFaceEncoding(fe))
                        {
                            var distance = FaceRecognition.FaceDistance(e1, e2);
                            this._TestOutputHelper.WriteLine($"Original: {distance}");
                            Assert.True(Math.Abs(distance) < double.Epsilon);
                        }

                        fe[0] = 1;
                        using (var e2 = FaceRecognition.LoadFaceEncoding(fe))
                        {
                            var distance = FaceRecognition.FaceDistance(e1, e2);
                            this._TestOutputHelper.WriteLine($"Modified: {distance}");
                            Assert.True(Math.Abs(distance) > double.Epsilon);
                        }
                    }

                    foreach (var encoding in encodings)
                        encoding.Dispose();
                }
            }

            if (!atLeast1Time)
                Assert.True(false, "Assert check did not execute");
        }

        [Fact]
        public void LoadFaceEncodingFail()
        {
            try
            {
                FaceRecognition.LoadFaceEncoding(null);
                Assert.True(false, $"{nameof(this.FaceEncodings)}.{nameof(FaceRecognition.LoadFaceEncoding)} should throw {nameof(ArgumentNullException)}");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                FaceRecognition.LoadFaceEncoding(new double[129]);
                Assert.True(false, $"{nameof(this.FaceEncodings)}.{nameof(FaceRecognition.LoadFaceEncoding)} should throw {nameof(ArgumentOutOfRangeException)}");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                FaceRecognition.LoadFaceEncoding(new double[127]);
                Assert.True(false, $"{nameof(this.FaceEncodings)}.{nameof(FaceRecognition.LoadFaceEncoding)} should throw {nameof(ArgumentOutOfRangeException)}");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [Fact]
        public void LoadImage()
        {
            const string file = "419px-Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

            var path = Path.Combine(TestImageDirectory, file);

            using (var array2D = DlibDotNet.Dlib.LoadImage<RgbPixel>(path))
            {
                var bytes = array2D.ToBytes();

                var image = FaceRecognition.LoadImage(bytes, array2D.Rows, array2D.Columns, array2D.Columns * 3, Mode.Rgb);
                Assert.True(image.Width == 419, $"Width of {path} is wrong");
                Assert.True(image.Height == 600, $"Height of {path} is wrong");

                image.Dispose();
                Assert.True(image.IsDisposed, $"{typeof(Image)} should be already disposed.");

                try
                {
                    image.Dispose();
                }
                catch
                {
                    Assert.True(false, $"{typeof(Image)} must not throw exception even though {nameof(Image.Dispose)} method is called again.");
                }

                try
                {
                    var _ = image.Width;
                    Assert.True(false, $"{nameof(Image.Width)} must throw {typeof(ObjectDisposedException)} after object is disposed.");
                }
                catch (ObjectDisposedException)
                {
                }

                try
                {
                    var _ = image.Height;
                    Assert.True(false, $"{nameof(Image.Height)} must throw {typeof(ObjectDisposedException)} after object is disposed.");
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        [Fact]
        public void LoadImage2()
        {
            const string file = "419px-Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

            var path = Path.Combine(TestImageDirectory, file);

            using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(path))
            {
                BitmapData bitmapData = null;

                try
                {
                    var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    var array = bitmapData.Scan0;
                    var stride = bitmapData.Stride;

                    // ToDo: Windows Bitmap is BGR so it should convert it. But this test case does not take care of accuracy.
                    var image = FaceRecognition.LoadImage(array, bitmapData.Height, bitmapData.Width, stride, Mode.Rgb);
                    Assert.True(image.Width == 419, $"Width of {path} is wrong");
                    Assert.True(image.Height == 600, $"Height of {path} is wrong");

                    image.Dispose();
                    Assert.True(image.IsDisposed, $"{typeof(Image)} should be already disposed.");

                    try
                    {
                        image.Dispose();
                    }
                    catch
                    {
                        Assert.True(false, $"{typeof(Image)} must not throw exception even though {nameof(Image.Dispose)} method is called again.");
                    }

                    try
                    {
                        var _ = image.Width;
                        Assert.True(false, $"{nameof(Image.Width)} must throw {typeof(ObjectDisposedException)} after object is disposed.");
                    }
                    catch (ObjectDisposedException)
                    {
                    }

                    try
                    {
                        var _ = image.Height;
                        Assert.True(false, $"{nameof(Image.Height)} must throw {typeof(ObjectDisposedException)} after object is disposed.");
                    }
                    catch (ObjectDisposedException)
                    {
                    }

                }
                finally
                {
                    if (bitmapData != null)
                        bitmap.UnlockBits(bitmapData);
                }
            }
        }

        [Fact]
        public void LoadImageRgba()
        {
            const string file = "419px-Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

            var path = Path.Combine(TestImageDirectory, file);

            using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(path))
            {
                using (var rgba = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb))
                using (var g = Graphics.FromImage(rgba))
                {
                    g.DrawImage(bitmap,
                        new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        GraphicsUnit.Pixel);

                    var image = FaceRecognition.LoadImage(rgba);
                    Assert.True(image.Width == 419, $"Width of {path} is wrong");
                    Assert.True(image.Height == 600, $"Height of {path} is wrong");
                    image.Dispose();
                    Assert.True(image.IsDisposed, $"{typeof(Image)} should be already disposed.");
                }
            }
        }

        [Fact]
        public void LoadImageGrayscale()
        {
            const string file = "419px-Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

            var path = Path.Combine(TestImageDirectory, file);

            using (var array2D = DlibDotNet.Dlib.LoadImage<RgbPixel>(path))
            using (var array2DGray = new Array2D<byte>(array2D.Rows, array2D.Columns))
            {
                DlibDotNet.Dlib.AssignImage(array2DGray, array2D);
                var bytes = array2DGray.ToBytes();

                using (var image = FaceRecognition.LoadImage(bytes, array2DGray.Rows, array2DGray.Columns, array2D.Columns * 1, Mode.Greyscale))
                {
                    Assert.True(image.Width == 419, $"Width of {path} is wrong");
                    Assert.True(image.Height == 600, $"Height of {path} is wrong");

                    image.Dispose();
                    Assert.True(image.IsDisposed, $"{typeof(Image)} should be already disposed.");

                    try
                    {
                        image.Dispose();
                    }
                    catch
                    {
                        Assert.True(false, $"{typeof(Image)} must not throw exception even though {nameof(Image.Dispose)} method is called again.");
                    }

                    try
                    {
                        var _ = image.Width;
                        Assert.True(false, $"{nameof(Image.Width)} must throw {typeof(ObjectDisposedException)} after object is disposed.");
                    }
                    catch (ObjectDisposedException)
                    {
                    }

                    try
                    {
                        var _ = image.Height;
                        Assert.True(false, $"{nameof(Image.Height)} must throw {typeof(ObjectDisposedException)} after object is disposed.");
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
            }
        }

        [Fact]
        public void LoadImageException()
        {
            try
            {
                var _ = FaceRecognition.LoadImage(null, 100, 100, 300, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                var _ = FaceRecognition.LoadImage(new byte[100 * 100 * 3], 100, 100, 400, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentOutOfRangeException)}.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                var _ = FaceRecognition.LoadImage(new byte[100 * 100 * 3], -1, 100, 50, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentOutOfRangeException)}.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                var _ = FaceRecognition.LoadImage(new byte[100 * 100 * 3], 100, -1, 50, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentOutOfRangeException)}.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                var _ = FaceRecognition.LoadImage(new byte[100 * 100 * 3], 100, 50, -1, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentOutOfRangeException)}.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                var _ = FaceRecognition.LoadImage(new byte[100 * 100 * 3], 100, 50, 20, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentOutOfRangeException)}.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [Fact]
        public void LoadImage2Exception()
        {
            try
            {
                var _ = FaceRecognition.LoadImage(IntPtr.Zero, 100, 100, 300, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentException)}.");
            }
            catch (ArgumentException)
            {
            }

            try
            {
                var dummy = (IntPtr)10;
                var _ = FaceRecognition.LoadImage(dummy, 100, 100, 50, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentOutOfRangeException)}.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                var dummy = (IntPtr)10;
                var _ = FaceRecognition.LoadImage(dummy, -1, 100, 50, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentOutOfRangeException)}.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                var dummy = (IntPtr)10;
                var _ = FaceRecognition.LoadImage(dummy, 100, -1, 50, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentOutOfRangeException)}.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                var dummy = (IntPtr)10;
                var _ = FaceRecognition.LoadImage(dummy, 100, 50, -1, Mode.Rgb);
                Assert.True(false, $"{nameof(FaceRecognition.LoadImage)} must throw {typeof(ArgumentOutOfRangeException)}.");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [Fact]
        public void LoadImageCheckIdentity()
        {
            const string file = "419px-Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

            var path = Path.Combine(TestImageDirectory, file);

            var image1 = FaceRecognition.LoadImageFile(path);

            var array2D = DlibDotNet.Dlib.LoadImage<RgbPixel>(path);
            var bytes = array2D.ToBytes();
            var image2 = FaceRecognition.LoadImage(bytes, array2D.Rows, array2D.Columns, array2D.Columns * 3, Mode.Rgb);

            var location1 = this._FaceRecognition.FaceLocations(image1).ToArray();
            var location2 = this._FaceRecognition.FaceLocations(image2).ToArray();

            Assert.True(location1.Length == location2.Length, $"FaceRecognition.FaceLocations returns different results for {nameof(location1)} and {nameof(location2)}.");

            for (var index = 0; index < location1.Length; index++)
            {
                Assert.True(location1[index] == location2[index],
                    $"{nameof(location1)}[{nameof(index)}] does not equal to {nameof(location2)}[{nameof(index)}].");
            }

            image2.Dispose();
            image1.Dispose();
            Assert.True(image2.IsDisposed, $"{nameof(image2)} should be already disposed.");
            Assert.True(image1.IsDisposed, $"{nameof(image1)} should be already disposed.");
        }

        [Fact]
        public void LoadImageFile()
        {
            const string file = "419px-Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

            var path = Path.Combine(TestImageDirectory, file);

            var image = FaceRecognition.LoadImageFile(path);
            Assert.True(image.Width == 419, $"Width of {path} is wrong");
            Assert.True(image.Height == 600, $"Height of {path} is wrong");

            image.Dispose();
            Assert.True(image.IsDisposed, $"{typeof(Image)} should be already disposed.");
        }

        [Fact]
        public void LoadImageFail()
        {
            Image image = null;
            try
            {
                image = FaceRecognition.LoadImageFile("test.bmp");
                Assert.True(false, "test.bmp directory is missing and LoadImageFile method should throw exception.");
            }
            catch (FileNotFoundException)
            {
            }
            finally
            {
                image?.Dispose();
            }
        }

        [Fact]
        public void SerializeDeserializeBinaryFormatter()
        {
            const string testName = nameof(this.SerializeDeserializeBinaryFormatter);
            var directory = Path.Combine(ResultDirectory, testName);
            Directory.CreateDirectory(directory);

            var path = Path.Combine(TestImageDirectory, TwoPersonFile);

            using (var image = FaceRecognition.LoadImageFile(path))
            {
                var encodings = this._FaceRecognition.FaceEncodings(image).ToArray();
                Assert.True(encodings.Length > 1, "");

                var dest = $"{path}.dat";
                if (File.Exists(dest))
                    File.Delete(dest);

                var bf = new BinaryFormatter();
                using (var fs = new FileStream(dest, FileMode.OpenOrCreate))
                    bf.Serialize(fs, encodings.First());

                using (var fs = new FileStream(dest, FileMode.OpenOrCreate))
                {
                    var encoding = (FaceEncoding)bf.Deserialize(fs);
                    var distance = FaceRecognition.FaceDistance(encodings.First(), encoding);
                    Assert.True(Math.Abs(distance) < double.Epsilon);
                }

                foreach (var encoding in encodings)
                    encoding.Dispose();

                foreach (var encoding in encodings)
                    Assert.True(encoding.IsDisposed, $"{typeof(FaceEncoding)} should be already disposed.");
            }
        }

        [Fact]
        public void PredictAge()
        {
            if (!File.Exists(this._AgeEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleAgeEstimator(this._AgeEstimatorModelFile))
                {
                    this._FaceRecognition.CustomAgeEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomAgeEstimator, estimator);

                    var range = estimator.Groups.ToArray();
                    var answer = new[]
                    {
                        new []{0,2 },
                        new []{4,6 },
                        new []{8,13},
                        new []{15,20},
                        new []{25,32},
                        new []{38,43},
                        new []{48,53},
                        new []{60,100}
                    };
                    for (var index = 0; index < answer.Length; index++)
                    {
                        Assert.True(range[index].Start == answer[index][0], $"{nameof(AgeRange.Start)} does not equal to {answer[index][0]}");
                        Assert.True(range[index].End == answer[index][1], $"{nameof(AgeRange.End)} does not equal to {answer[index][1]}");
                    }

                    // 0: (0, 2)
                    // 1: (4, 6)
                    // 2: (8, 13)
                    // 3: (15, 20)
                    // 4: (25, 32)
                    // 5: (38, 43)
                    // 6: (48, 53)
                    // 7: (60, 100)
                    var groundTruth = new[]
                    {
                        new { Path = Path.Combine(TestImageDirectory, "Age", "NelsonMandela_2008_90.jpg"),        Age = new uint[]{ 7 } },
                        new { Path = Path.Combine(TestImageDirectory, "Age", "MacaulayCulkin_1991_11.jpg"),       Age = new uint[]{ 2, 3 } },
                        new { Path = Path.Combine(TestImageDirectory, "Age", "DianaPrincessOfWales_1997_36.jpg"), Age = new uint[]{ 4, 5 } },
                        new { Path = Path.Combine(TestImageDirectory, "Age", "MaoAsada_2014_24.jpg"),             Age = new uint[]{ 3, 4 } }
                    };

                    foreach (var gt in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(gt.Path))
                        {
                            var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                            var age = this._FaceRecognition.PredictAge(image, location);
                            Assert.True(gt.Age.Contains(age), $"Failed to classify '{gt.Path}'");
                        }
                }
            }
            finally
            {
                this._FaceRecognition.CustomAgeEstimator = null;
            }
        }

        [Fact]
        public void PredictAgeRepeat()
        {
            if (!File.Exists(this._AgeEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleAgeEstimator(this._AgeEstimatorModelFile))
                {
                    this._FaceRecognition.CustomAgeEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomAgeEstimator, estimator);

                    // 0: (0, 2)
                    // 1: (4, 6)
                    // 2: (8, 13)
                    // 3: (15, 20)
                    // 4: (25, 32)
                    // 5: (38, 43)
                    // 6: (48, 53)
                    // 7: (60, 100)
                    var groundTruth = new[]
                    {
                        new { Path = Path.Combine(TestImageDirectory, "Age", "MaoAsada_2014_24.jpg"),             Age = new uint[]{ 3, 4 } }
                    };

                    foreach (var gt in groundTruth)
                        foreach (var index in Enumerable.Range(0, 10))
                            using (var image = FaceRecognition.LoadImageFile(gt.Path))
                            {
                                var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                                var age = this._FaceRecognition.PredictAge(image, location);
                                Assert.True(gt.Age.Contains(age), $"Failed to classify '{gt.Path}' for repeat {index + 1}");
                            }
                }
            }
            finally
            {
                this._FaceRecognition.CustomAgeEstimator = null;
            }
        }

        [Fact]
        public void PredictAgeException()
        {
            try
            {
                new SimpleAgeEstimator("not_found");
                Assert.True(false, $"{nameof(SimpleAgeEstimator)} method should throw exception.");
            }
            catch (FileNotFoundException)
            {
            }

            try
            {
                this._FaceRecognition.PredictAge(null, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictAge)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictAge(image, null);
                Assert.True(false, $"{nameof(PredictAge)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictAge(image, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictAge)} method should throw {nameof(NotSupportedException)}.");
            }
            catch (NotSupportedException)
            {
            }

            if (!File.Exists(this._AgeEstimatorModelFile))
                return;

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                using (var estimator = new SimpleAgeEstimator(this._AgeEstimatorModelFile))
                {
                    this._FaceRecognition.CustomAgeEstimator = estimator;
                    estimator.Dispose();
                    this._FaceRecognition.PredictAge(image, new Location(0, 0, 0, 0));
                }
                Assert.True(false, $"{nameof(PredictAge)} method should throw {nameof(ObjectDisposedException)}.");
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        public void PredictEmotion()
        {
            if (!File.Exists(this._EmotionEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleEmotionEstimator(this._EmotionEstimatorModelFile))
                {
                    this._FaceRecognition.CustomEmotionEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomEmotionEstimator, estimator);

                    var groundTruth = estimator.Labels.Select(s => new KeyValuePair<string, string>(Path.Combine(TestImageDirectory, "Emotion", $"{s}.png"), s))
                                                      .Where(pair => File.Exists(pair.Key));
                    foreach (var gt in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(gt.Key))
                        {
                            var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                            var emotion = this._FaceRecognition.PredictEmotion(image, location);
                            Assert.True(gt.Value == emotion, $"Failed to classify '{gt.Value}'");
                        }
                }
            }
            finally
            {
                this._FaceRecognition.CustomEmotionEstimator = null;
            }
        }

        [Fact]
        public void PredictEmotionRepeat()
        {
            if (!File.Exists(this._EmotionEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleEmotionEstimator(this._EmotionEstimatorModelFile))
                {
                    this._FaceRecognition.CustomEmotionEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomEmotionEstimator, estimator);

                    var groundTruth = estimator.Labels.Select(s => new KeyValuePair<string, string>(Path.Combine(TestImageDirectory, "Emotion", $"{s}.png"), s))
                                                      .Where(pair => File.Exists(pair.Key));
                    foreach (var gt in groundTruth)
                        foreach (var index in Enumerable.Range(0, 10))
                            using (var image = FaceRecognition.LoadImageFile(gt.Key))
                            {
                                var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                                var emotion = this._FaceRecognition.PredictEmotion(image, location);
                                Assert.True(gt.Value == emotion, $"Failed to classify '{gt.Value}' for repeat {index + 1}");
                            }
                }
            }
            finally
            {
                this._FaceRecognition.CustomEmotionEstimator = null;
            }
        }

        [Fact]
        public void PredictEmotionException()
        {
            try
            {
                new SimpleEmotionEstimator("not_found");
                Assert.True(false, $"{nameof(SimpleEmotionEstimator)} method should throw exception.");
            }
            catch (FileNotFoundException)
            {
            }

            try
            {
                this._FaceRecognition.PredictEmotion(null, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictEmotion)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictEmotion(image, null);
                Assert.True(false, $"{nameof(PredictEmotion)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictEmotion(image, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictEmotion)} method should throw {nameof(NotSupportedException)}.");
            }
            catch (NotSupportedException)
            {
            }

            if (!File.Exists(this._EmotionEstimatorModelFile))
                return;

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                using (var estimator = new SimpleEmotionEstimator(this._EmotionEstimatorModelFile))
                {
                    this._FaceRecognition.CustomEmotionEstimator = estimator;
                    estimator.Dispose();
                    this._FaceRecognition.PredictEmotion(image, new Location(0, 0, 0, 0));
                }
                Assert.True(false, $"{nameof(PredictEmotion)} method should throw {nameof(ObjectDisposedException)}.");
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        public void PredictGender()
        {
            if (!File.Exists(this._GenderEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleGenderEstimator(this._GenderEstimatorModelFile))
                {
                    this._FaceRecognition.CustomGenderEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomGenderEstimator, estimator);

                    var range = estimator.Labels.ToArray();
                    var answer = new[]
                    {
                        Gender.Male,
                        Gender.Female
                    };
                    for (var index = 0; index < answer.Length; index++)
                        Assert.True(range[index] == answer[index], $"{nameof(Gender)} does not equal to {answer[index]}");

                    var groundTruth = new[]
                    {
                        new { Path = Path.Combine(TestImageDirectory, "Gender", "BarackObama_male.jpg"),            Gender = Gender.Male },
                        //new { Path = Path.Combine(TestImageDirectory, "Gender", "DianaPrincessOfWales_female.jpg"), Gender = Gender.Female },
                        new { Path = Path.Combine(TestImageDirectory, "Gender", "MaoAsada_female.jpg"),             Gender = Gender.Female },
                        new { Path = Path.Combine(TestImageDirectory, "Gender", "ShinzoAbe_male.jpg"),              Gender = Gender.Male },
                        new { Path = Path.Combine(TestImageDirectory, "Gender", "WhitneyHouston_female.jpg"),       Gender = Gender.Female },
                    };

                    foreach (var gt in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(gt.Path))
                        {
                            var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                            var gender = this._FaceRecognition.PredictGender(image, location);
                            Assert.True(gt.Gender == gender, $"Failed to classify '{gt.Path}'");
                        }
                }
            }
            finally
            {
                this._FaceRecognition.CustomGenderEstimator = null;
            }
        }

        [Fact]
        public void PredictGenderRepeat()
        {
            if (!File.Exists(this._GenderEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleGenderEstimator(this._GenderEstimatorModelFile))
                {
                    this._FaceRecognition.CustomGenderEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomGenderEstimator, estimator);

                    var groundTruth = new[]
                    {
                        new { Path = Path.Combine(TestImageDirectory, "Gender", "MaoAsada_female.jpg"),             Gender = Gender.Female }
                    };

                    foreach (var gt in groundTruth)
                        foreach (var index in Enumerable.Range(0, 10))
                            using (var image = FaceRecognition.LoadImageFile(gt.Path))
                            {
                                var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                                var gender = this._FaceRecognition.PredictGender(image, location);
                                Assert.True(gt.Gender == gender, $"Failed to classify '{gt.Path}' for repeat {index + 1}");
                            }
                }
            }
            finally
            {
                this._FaceRecognition.CustomGenderEstimator = null;
            }
        }

        [Fact]
        public void PredictGenderException()
        {
            try
            {
                new SimpleGenderEstimator("not_found");
                Assert.True(false, $"{nameof(SimpleGenderEstimator)} method should throw exception.");
            }
            catch (FileNotFoundException)
            {
            }

            try
            {
                this._FaceRecognition.PredictGender(null, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictGender)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictGender(image, null);
                Assert.True(false, $"{nameof(PredictGender)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictGender(image, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictGender)} method should throw {nameof(NotSupportedException)}.");
            }
            catch (NotSupportedException)
            {
            }

            if (!File.Exists(this._GenderEstimatorModelFile))
                return;

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                using (var estimator = new SimpleGenderEstimator(this._GenderEstimatorModelFile))
                {
                    this._FaceRecognition.CustomGenderEstimator = estimator;
                    estimator.Dispose();
                    this._FaceRecognition.PredictGender(image, new Location(0, 0, 0, 0));
                }
                Assert.True(false, $"{nameof(PredictGender)} method should throw {nameof(ObjectDisposedException)}.");
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        public void PredictProbabilityAge()
        {
            if (!File.Exists(this._AgeEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleAgeEstimator(this._AgeEstimatorModelFile))
                {
                    this._FaceRecognition.CustomAgeEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomAgeEstimator, estimator);

                    // 0: (0, 2)
                    // 1: (4, 6)
                    // 2: (8, 13)
                    // 3: (15, 20)
                    // 4: (25, 32)
                    // 5: (38, 43)
                    // 6: (48, 53)
                    // 7: (60, 100)
                    var groundTruth = new[]
                    {
                        new {Path = Path.Combine(TestImageDirectory, "Age", "NelsonMandela_2008_90.jpg"),        Age = new uint[] {7}},
                        new {Path = Path.Combine(TestImageDirectory, "Age", "MacaulayCulkin_1991_11.jpg"),       Age = new uint[] {2, 3}},
                        new {Path = Path.Combine(TestImageDirectory, "Age", "DianaPrincessOfWales_1997_36.jpg"), Age = new uint[] {4, 5}},
                        new {Path = Path.Combine(TestImageDirectory, "Age", "MaoAsada_2014_24.jpg"),             Age = new uint[] {3, 4}}
                    };

                    foreach (var gt in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(gt.Path))
                        {
                            var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                            var probability = this._FaceRecognition.PredictProbabilityAge(image, location);

                            // Take top 2
                            var order = probability.OrderByDescending(x => x.Value).Take(2).ToArray();
                            var any = order.Select(pair => pair.Key).Any(u => gt.Age.Contains(u));
                            Assert.True(any, $"Failed to classify '{gt.Path}'. Probability: 1 ({order[0].Key}-{order[0].Value}), 2 ({order[1].Key}-{order[1].Value})");
                        }
                }
            }
            finally
            {
                this._FaceRecognition.CustomAgeEstimator = null;
            }
        }

        [Fact]
        public void PredictProbabilityAgeRepeat()
        {
            if (!File.Exists(this._AgeEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleAgeEstimator(this._AgeEstimatorModelFile))
                {
                    this._FaceRecognition.CustomAgeEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomAgeEstimator, estimator);

                    var list = new List<IDictionary<uint, float>>();
                    foreach (var index in Enumerable.Range(0, 10))
                        using (var image = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "Age", "MaoAsada_2014_24.jpg")))
                        {
                            var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                            var probability = this._FaceRecognition.PredictProbabilityAge(image, location);
                            list.Add(probability);
                        }

                    var first = list.First();
                    foreach (var results in list)
                    {
                        var keys1 = first.Keys;
                        foreach (var key in keys1)
                        {
                            var value1 = first[key];
                            var value2 = results[key];
                            Assert.True(Math.Abs(value1 - value2) < float.Epsilon, "Estimator should return same results");
                        }
                    }
                }
            }
            finally
            {
                this._FaceRecognition.CustomAgeEstimator = null;
            }
        }

        [Fact]
        public void PredictProbabilityAgeException()
        {

            try
            {
                this._FaceRecognition.PredictProbabilityAge(null, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictProbabilityAge)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictProbabilityAge(image, null);
                Assert.True(false, $"{nameof(PredictProbabilityAge)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictProbabilityAge(image, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictProbabilityAge)} method should throw {nameof(NotSupportedException)}.");
            }
            catch (NotSupportedException)
            {
            }

            if (!File.Exists(this._AgeEstimatorModelFile))
                return;

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                using (var estimator = new SimpleAgeEstimator(this._AgeEstimatorModelFile))
                {
                    this._FaceRecognition.CustomAgeEstimator = estimator;
                    estimator.Dispose();
                    this._FaceRecognition.PredictProbabilityAge(image, new Location(0, 0, 0, 0));
                }
                Assert.True(false, $"{nameof(PredictProbabilityAge)} method should throw {nameof(ObjectDisposedException)}.");
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        public void PredictProbabilityEmotion()
        {
            if (!File.Exists(this._EmotionEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleEmotionEstimator(this._EmotionEstimatorModelFile))
                {
                    this._FaceRecognition.CustomEmotionEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomEmotionEstimator, estimator);

                    var groundTruth = estimator.Labels.Select(s => new KeyValuePair<string, string>(Path.Combine(TestImageDirectory, "Emotion", $"{s}.png"), s))
                                                      .Where(pair => File.Exists(pair.Key));
                    foreach (var gt in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(gt.Key))
                        {
                            var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                            var probability = this._FaceRecognition.PredictProbabilityEmotion(image, location);

                            var pos = gt.Value;
                            var maxLabel = probability.Aggregate((max, working) => (max.Value > working.Value) ? max : working).Key;
                            Assert.True(pos == maxLabel, $"Failed to classify '{gt.Value}'. Probability: {probability[pos]}");
                        }
                }
            }
            finally
            {
                this._FaceRecognition.CustomEmotionEstimator = null;
            }
        }

        [Fact]
        public void PredictProbabilityEmotionRepeat()
        {
            if (!File.Exists(this._EmotionEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleEmotionEstimator(this._EmotionEstimatorModelFile))
                {
                    this._FaceRecognition.CustomEmotionEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomEmotionEstimator, estimator);

                    var groundTruth = estimator.Labels.Select(s => new KeyValuePair<string, string>(Path.Combine(TestImageDirectory, "Emotion", $"{s}.png"), s))
                                                      .Where(pair => File.Exists(pair.Key));
                    var list = new Dictionary<string, IDictionary<string, float>>();
                    foreach (var (key, value) in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(key))
                        {
                            var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                            var probability = this._FaceRecognition.PredictProbabilityEmotion(image, location);
                            list.Add(value, probability);
                        }

                    foreach (var gt in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(gt.Key))
                            foreach (var _ in Enumerable.Range(0, 10))
                            {
                                var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                                var probability = this._FaceRecognition.PredictProbabilityEmotion(image, location);

                                var first = list[gt.Value];
                                foreach (var label in estimator.Labels)
                                    Assert.True(Math.Abs(first[label] - probability[label]) < float.Epsilon, "Estimator should return same results");
                            }
                }
            }
            finally
            {
                this._FaceRecognition.CustomEmotionEstimator = null;
            }
        }

        [Fact]
        public void PredictProbabilityEmotionException()
        {
            try
            {
                this._FaceRecognition.PredictProbabilityEmotion(null, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictProbabilityEmotion)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictProbabilityEmotion(image, null);
                Assert.True(false, $"{nameof(PredictProbabilityEmotion)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictProbabilityEmotion(image, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictProbabilityEmotion)} method should throw {nameof(NotSupportedException)}.");
            }
            catch (NotSupportedException)
            {
            }

            if (!File.Exists(this._EmotionEstimatorModelFile))
                return;

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                using (var estimator = new SimpleEmotionEstimator(this._EmotionEstimatorModelFile))
                {
                    this._FaceRecognition.CustomEmotionEstimator = estimator;
                    estimator.Dispose();
                    this._FaceRecognition.PredictProbabilityEmotion(image, new Location(0, 0, 0, 0));
                }
                Assert.True(false, $"{nameof(PredictProbabilityEmotion)} method should throw {nameof(ObjectDisposedException)}.");
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        public void PredictProbabilityGender()
        {
            if (!File.Exists(this._GenderEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleGenderEstimator(this._GenderEstimatorModelFile))
                {
                    this._FaceRecognition.CustomGenderEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomGenderEstimator, estimator);

                    var groundTruth = new[]
                    {
                        new {Path = Path.Combine(TestImageDirectory, "Gender", "BarackObama_male.jpg"),            Gender = Gender.Male},
                        //new {Path = Path.Combine(TestImageDirectory, "Gender", "DianaPrincessOfWales_female.jpg"), Gender = Gender.Female},
                        new {Path = Path.Combine(TestImageDirectory, "Gender", "MaoAsada_female.jpg"),             Gender = Gender.Female},
                        new {Path = Path.Combine(TestImageDirectory, "Gender", "ShinzoAbe_male.jpg"),              Gender = Gender.Male},
                        new {Path = Path.Combine(TestImageDirectory, "Gender", "WhitneyHouston_female.jpg"),       Gender = Gender.Female},
                    };

                    foreach (var gt in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(gt.Path))
                        {
                            var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                            var probability = this._FaceRecognition.PredictProbabilityGender(image, location);

                            var pos = gt.Gender;
                            var neg = pos == Gender.Male ? Gender.Female : Gender.Male;
                            Assert.True(probability[pos] > probability[neg], $"Failed to classify '{gt.Path}'. Probability: {probability[pos]}");
                        }
                }
            }
            finally
            {
                this._FaceRecognition.CustomGenderEstimator = null;
            }
        }

        [Fact]
        public void PredictProbabilityGenderRepeat()
        {
            if (!File.Exists(this._GenderEstimatorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleGenderEstimator(this._GenderEstimatorModelFile))
                {
                    this._FaceRecognition.CustomGenderEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomGenderEstimator, estimator);

                    var groundTruth = new[]
                    {
                        new {Path = Path.Combine(TestImageDirectory, "Gender", "MaoAsada_female.jpg") }
                    };

                    var list = new List<IDictionary<Gender, float>>();
                    foreach (var gt in groundTruth)
                        foreach (var index in Enumerable.Range(0, 10))
                            using (var image = FaceRecognition.LoadImageFile(gt.Path))
                            {
                                var location = this._FaceRecognition.FaceLocations(image).ToArray()[0];
                                var probability = this._FaceRecognition.PredictProbabilityGender(image, location);
                                list.Add(probability);
                            }

                    var first = list.First();
                    foreach (var results in list)
                    {
                        var keys1 = first.Keys;
                        foreach (var key in keys1)
                        {
                            var value1 = first[key];
                            var value2 = results[key];
                            Assert.True(Math.Abs(value1 - value2) < float.Epsilon, "Estimator should return same results");
                        }
                    }
                }
            }
            finally
            {
                this._FaceRecognition.CustomGenderEstimator = null;
            }
        }

        [Fact]
        public void PredictProbabilityGenderException()
        {
            try
            {
                this._FaceRecognition.PredictProbabilityGender(null, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictProbabilityGender)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictProbabilityGender(image, null);
                Assert.True(false, $"{nameof(PredictProbabilityGender)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                    this._FaceRecognition.PredictProbabilityGender(image, new Location(0, 0, 0, 0));
                Assert.True(false, $"{nameof(PredictProbabilityGender)} method should throw {nameof(NotSupportedException)}.");
            }
            catch (NotSupportedException)
            {
            }

            if (!File.Exists(this._GenderEstimatorModelFile))
                return;

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (var image = FaceRecognition.LoadImage(bmp))
                using (var estimator = new SimpleGenderEstimator(this._GenderEstimatorModelFile))
                {
                    this._FaceRecognition.CustomGenderEstimator = estimator;
                    estimator.Dispose();
                    this._FaceRecognition.PredictProbabilityGender(image, new Location(0, 0, 0, 0));
                }
                Assert.True(false, $"{nameof(PredictProbabilityGender)} method should throw {nameof(ObjectDisposedException)}.");
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        public void PredictHeadPose()
        {
            const string testName = nameof(PredictHeadPose);

            if (!File.Exists(this._RollEstimateorModelFile))
                return;
            if (!File.Exists(this._PitchEstimateorModelFile))
                return;
            if (!File.Exists(this._YawEstimateorModelFile))
                return;

            try
            {
                using (var estimator = new SimpleHeadPoseEstimator(this._RollEstimateorModelFile,
                                                                   this._PitchEstimateorModelFile,
                                                                   this._YawEstimateorModelFile))
                {
                    this._FaceRecognition.CustomHeadPoseEstimator = estimator;
                    Assert.Equal(this._FaceRecognition.CustomHeadPoseEstimator, estimator);

                    const int pointSize = 2;
                    const double diff = 20.00;
                    var groundTruth = new[]
                    {
                        new { Path = Path.Combine(TestImageDirectory, "HeadPose", "AFW_134212_1_4.jpg"), Pose = new HeadPose(-12.5080101676021,-24.7073657941586, 46.8672622731993) },
                    };

                    foreach (var gt in groundTruth)
                        using (var image = FaceRecognition.LoadImageFile(gt.Path))
                        {
                            var landmarks = this._FaceRecognition.FaceLandmark(image, null, PredictorModel.Large).ToArray()[0];
                            var headPose = this._FaceRecognition.PredictHeadPose(landmarks);

                            using (var bitmap = System.Drawing.Image.FromFile(gt.Path))
                            {
                                using (var g = Graphics.FromImage(bitmap))
                                {
                                    foreach (var landmark in landmarks.Values)
                                        foreach (var p in landmark)
                                        {
                                            g.DrawEllipse(Pens.GreenYellow, p.Point.X - pointSize, p.Point.Y - pointSize, pointSize * 2, pointSize * 2);
                                        }

                                    DrawAxis(g, landmarks[FacePart.NoseTip], headPose.Roll, headPose.Pitch, headPose.Yaw, 150);
                                }

                                var directory = Path.Combine(ResultDirectory, testName);
                                Directory.CreateDirectory(directory);

                                var filename = Path.GetFileName(gt.Path);
                                filename = Path.ChangeExtension(filename, ".png");
                                var dst = Path.Combine(directory, filename);
                                bitmap.Save(dst, System.Drawing.Imaging.ImageFormat.Png);
                            }

                            Assert.InRange(headPose.Roll, gt.Pose.Roll - diff, gt.Pose.Roll + diff);
                            Assert.InRange(headPose.Pitch, gt.Pose.Pitch - diff, gt.Pose.Pitch + diff);
                            Assert.InRange(headPose.Yaw, gt.Pose.Yaw - diff, gt.Pose.Yaw + diff);
                        }
                }
            }
            finally
            {
                this._FaceRecognition.CustomHeadPoseEstimator = null;
            }
        }

        [Fact]
        public void PredictHeadPoseException()
        {
            if (!File.Exists(this._RollEstimateorModelFile))
                return;
            if (!File.Exists(this._PitchEstimateorModelFile))
                return;
            if (!File.Exists(this._YawEstimateorModelFile))
                return;

            try
            {
                new SimpleHeadPoseEstimator("not_found", this._PitchEstimateorModelFile, this._YawEstimateorModelFile);
                Assert.True(false, $"{nameof(SimpleHeadPoseEstimator)} method should throw exception.");
            }
            catch (FileNotFoundException)
            {
            }

            try
            {
                new SimpleHeadPoseEstimator(this._RollEstimateorModelFile, null, this._YawEstimateorModelFile);
                Assert.True(false, $"{nameof(SimpleHeadPoseEstimator)} method should throw exception.");
            }
            catch (FileNotFoundException)
            {
            }

            try
            {
                new SimpleHeadPoseEstimator(this._RollEstimateorModelFile, this._PitchEstimateorModelFile, null);
                Assert.True(false, $"{nameof(SimpleHeadPoseEstimator)} method should throw exception.");
            }
            catch (FileNotFoundException)
            {
            }

            try
            {
                this._FaceRecognition.PredictHeadPose(null);
                Assert.True(false, $"{nameof(PredictHeadPose)} method should throw {nameof(ArgumentNullException)}.");
            }
            catch (ArgumentNullException)
            {
            }

            var parts = new Dictionary<FacePart, IEnumerable<FacePoint>>();

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (FaceRecognition.LoadImage(bmp)) this._FaceRecognition.PredictHeadPose(parts);
                Assert.True(false, $"{nameof(PredictHeadPose)} method should throw {nameof(NotSupportedException)}.");
            }
            catch (NotSupportedException)
            {
            }

            try
            {
                using (var bmp = new Bitmap(100, 100))
                using (FaceRecognition.LoadImage(bmp))
                using (var estimator = new SimpleHeadPoseEstimator(this._RollEstimateorModelFile, this._PitchEstimateorModelFile, this._YawEstimateorModelFile))
                {
                    this._FaceRecognition.CustomHeadPoseEstimator = estimator;
                    estimator.Dispose();
                    this._FaceRecognition.PredictHeadPose(parts);
                }
                Assert.True(false, $"{nameof(PredictHeadPose)} method should throw {nameof(ObjectDisposedException)}.");
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        public void TestBatchedFaceLocations()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var images = new[] { img, img, img };
                var batchedDetectedFaces = this._FaceRecognition.BatchFaceLocations(images, 0).ToArray();
                Assert.Equal(3, batchedDetectedFaces.Length);

                foreach (var detectedFaces in batchedDetectedFaces)
                {
                    var tmp = detectedFaces.ToArray();
                    Assert.True(tmp.Length == 1);
                    Assert.True(tmp[0] == new Location(375, 154, 611, 390));
                }
            }
        }

        [Fact]
        public void TestBatchedFaceLocationsException()
        {
            using (var _ = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var images = new Image[] { };

                try
                {
                    var __ = this._FaceRecognition.BatchFaceLocations(images, 0).ToArray();
                }
                catch (Exception)
                {
                    Assert.True(false, $"{nameof(FaceRecognition.BatchFaceLocations)} must not throw exception even though {nameof(images)} is empty elements.");
                }

                try
                {
                    var __ = this._FaceRecognition.BatchFaceLocations(null, 0).ToArray();
                    Assert.True(false, $"{nameof(FaceRecognition.BatchFaceLocations)} must throw {typeof(ArgumentNullException)}.");
                }
                catch (ArgumentNullException)
                {
                }
            }
        }

        [Fact]
        public void TestCnnFaceLocations()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var detectedFaces = this._FaceRecognition.FaceLocations(img, 1, Model.Cnn).ToArray();
                Assert.True(detectedFaces.Length == 1);
                Assert.True(AssertAlmostEqual(detectedFaces[0].Top, 144, 25));
                Assert.True(AssertAlmostEqual(detectedFaces[0].Right, 608, 25));
                Assert.True(AssertAlmostEqual(detectedFaces[0].Bottom, 389, 25));
                Assert.True(AssertAlmostEqual(detectedFaces[0].Left, 363, 25));
            }
        }

        [Fact]
        public void TestCnnRawFaceLocations()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var detectedFaces = this.RawFaceLocations(img, 1, Model.Cnn).ToArray();
                Assert.True(detectedFaces.Length == 1);
                Assert.True(AssertAlmostEqual(detectedFaces[0].Rect.Top, 144, 25));
                Assert.True(AssertAlmostEqual(detectedFaces[0].Rect.Bottom, 389, 25));

                foreach (var detectedFace in detectedFaces)
                    detectedFace.Dispose();
            }
        }

        [Fact]
        public void TestCnnRawFaceLocations32BitImage()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "32bit.png")))
            {
                var detectedFaces = this.RawFaceLocations(img, 1, Model.Cnn).ToArray();
                Assert.True(detectedFaces.Length == 1);
                Assert.True(AssertAlmostEqual(detectedFaces[0].Rect.Top, 259, 25));
                Assert.True(AssertAlmostEqual(detectedFaces[0].Rect.Bottom, 552, 25));

                foreach (var detectedFace in detectedFaces)
                    detectedFace.Dispose();
            }
        }

        [Fact]
        public void TestCompareFaces()
        {
            using (var imgA1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            using (var imgA2 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama2.jpg")))
            using (var imgA3 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama3.jpg")))
            using (var imgB1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "biden.jpg")))
            {
                using (var faceEncodingA1 = this._FaceRecognition.FaceEncodings(imgA1).ToArray()[0])
                using (var faceEncodingA2 = this._FaceRecognition.FaceEncodings(imgA2).ToArray()[0])
                using (var faceEncodingA3 = this._FaceRecognition.FaceEncodings(imgA3).ToArray()[0])
                using (var faceEncodingB1 = this._FaceRecognition.FaceEncodings(imgB1).ToArray()[0])
                {
                    var facesToCompare = new[]
                    {
                        faceEncodingA2,
                        faceEncodingA3,
                        faceEncodingB1
                    };

                    var matchResults = facesToCompare.Select(faceToCompare => FaceRecognition.CompareFace(faceToCompare, faceEncodingA1)).ToList();

                    Assert.True(matchResults[0]);
                    Assert.True(matchResults[1]);
                    Assert.False(matchResults[2]);
                }
            }
        }

        [Fact]
        public void TestCompareFaceException()
        {
            using (var imgA1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            using (var imgA2 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama2.jpg")))
            {
                using (var faceEncodingA1 = this._FaceRecognition.FaceEncodings(imgA1).ToArray()[0])
                using (var faceEncodingA2 = this._FaceRecognition.FaceEncodings(imgA2).ToArray()[0])
                {
                    try
                    {
                        FaceRecognition.CompareFace(faceEncodingA1, null);
                        Assert.True(false, $"{nameof(FaceRecognition.CompareFace)} must throw {typeof(ArgumentNullException)}.");
                    }
                    catch (ArgumentNullException)
                    {
                    }

                    try
                    {
                        FaceRecognition.CompareFace(null, faceEncodingA2);
                        Assert.True(false, $"{nameof(FaceRecognition.CompareFace)} must throw {typeof(ArgumentNullException)}.");
                    }
                    catch (ArgumentNullException)
                    {
                    }
                }
            }
        }

        [Fact]
        public void TestCompareFacesException()
        {
            using (var imgA1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            using (var imgA2 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama2.jpg")))
            using (var imgA3 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama3.jpg")))
            using (var imgB1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "biden.jpg")))
            {
                using (var faceEncodingA1 = this._FaceRecognition.FaceEncodings(imgA1).ToArray()[0])
                using (var faceEncodingA2 = this._FaceRecognition.FaceEncodings(imgA2).ToArray()[0])
                using (var faceEncodingA3 = this._FaceRecognition.FaceEncodings(imgA3).ToArray()[0])
                using (var faceEncodingB1 = this._FaceRecognition.FaceEncodings(imgB1).ToArray()[0])
                {
                    var facesToCompare = new[]
                    {
                        faceEncodingA2,
                        faceEncodingA3,
                        faceEncodingB1
                    };

                    try
                    {
                        var _ = FaceRecognition.CompareFaces(facesToCompare, null).ToArray();
                        Assert.True(false, $"{nameof(FaceRecognition.CompareFaces)} must throw {typeof(ArgumentNullException)}.");
                    }
                    catch (ArgumentNullException)
                    {
                    }

                    try
                    {
                        var _ = FaceRecognition.CompareFaces(null, faceEncodingA1).ToArray();
                        Assert.True(false, $"{nameof(FaceRecognition.CompareFaces)} must throw {typeof(ArgumentNullException)}.");
                    }
                    catch (ArgumentNullException)
                    {
                    }

                    try
                    {
                        foreach (var encoding in facesToCompare)
                            encoding.Dispose();
                        var _ = FaceRecognition.CompareFaces(facesToCompare, faceEncodingA1).ToArray();
                        Assert.True(false, $"{nameof(FaceRecognition.CompareFaces)} must throw {typeof(ObjectDisposedException)} if knownFaceEncodings contains disposed object.");
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
            }
        }

        [Fact]
        public void TestCompareFacesEmptyLists()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "biden.jpg")))
            {
                var encoding = this._FaceRecognition.FaceEncodings(img).ToArray()[0];

                // empty list
                var facesToCompare = new FaceEncoding[0];

                var matchResult = FaceRecognition.CompareFaces(facesToCompare, encoding).ToArray();
                Assert.True(matchResult.Length == 0);

                encoding.Dispose();
            }
        }

        [Fact]
        public void TestFaceDistance()
        {
            using (var imgA1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            using (var imgA2 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama2.jpg")))
            using (var imgA3 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama3.jpg")))
            using (var imgB1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "biden.jpg")))
            {
                using (var faceEncodingA1 = this._FaceRecognition.FaceEncodings(imgA1).ToArray()[0])
                using (var faceEncodingA2 = this._FaceRecognition.FaceEncodings(imgA2).ToArray()[0])
                using (var faceEncodingA3 = this._FaceRecognition.FaceEncodings(imgA3).ToArray()[0])
                using (var faceEncodingB1 = this._FaceRecognition.FaceEncodings(imgB1).ToArray()[0])
                {
                    var facesToCompare = new[]
                    {
                        faceEncodingA2,
                        faceEncodingA3,
                        faceEncodingB1
                    };

                    var distanceResults = facesToCompare.Select(faceToCompare => FaceRecognition.FaceDistance(faceToCompare, faceEncodingA1)).ToList();

                    Assert.True(distanceResults[0] <= 0.6);
                    Assert.True(distanceResults[1] <= 0.6);
                    Assert.True(distanceResults[2] > 0.6);
                }
            }
        }

        [Fact]
        public void TestFaceDistances()
        {
            using (var imgA1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            using (var imgA2 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama2.jpg")))
            using (var imgA3 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama3.jpg")))
            using (var imgB1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "biden.jpg")))
            {
                using (var faceEncodingA1 = this._FaceRecognition.FaceEncodings(imgA1).ToArray()[0])
                using (var faceEncodingA2 = this._FaceRecognition.FaceEncodings(imgA2).ToArray()[0])
                using (var faceEncodingA3 = this._FaceRecognition.FaceEncodings(imgA3).ToArray()[0])
                using (var faceEncodingB1 = this._FaceRecognition.FaceEncodings(imgB1).ToArray()[0])
                {
                    var facesToCompare = new[]
                    {
                        faceEncodingA2,
                        faceEncodingA3,
                        faceEncodingA1
                    };

                    var results = FaceRecognition.FaceDistances(facesToCompare, faceEncodingB1).ToArray();
                    Assert.True(!results.Any(d => d < 0.8));

                    facesToCompare = new[]
                    {
                        faceEncodingA2,
                        faceEncodingA3
                    };

                    var results2 = FaceRecognition.FaceDistances(facesToCompare, faceEncodingA1).ToArray();
                    Assert.True(!results2.Any(d => d > 0.4));
                }
            }
        }

        [Fact]
        public void FaceDistanceException()
        {
            using (var imgA1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            using (var imgA2 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama2.jpg")))
            {
                using (var faceEncodingA1 = this._FaceRecognition.FaceEncodings(imgA1).ToArray()[0])
                using (var faceEncodingA2 = this._FaceRecognition.FaceEncodings(imgA2).ToArray()[0])
                {
                    try
                    {
                        var _ = FaceRecognition.FaceDistance(faceEncodingA1, null);
                        Assert.True(false, $"{nameof(FaceRecognition.FaceDistance)} must throw {typeof(ArgumentNullException)}.");
                    }
                    catch (ArgumentNullException)
                    {
                    }

                    try
                    {
                        var _ = FaceRecognition.FaceDistance(null, faceEncodingA2);
                        Assert.True(false, $"{nameof(FaceRecognition.FaceDistance)} must throw {typeof(ArgumentNullException)}.");
                    }
                    catch (ArgumentNullException)
                    {
                    }
                }
            }
        }

        [Fact]
        public void FaceDistancesException()
        {
            using (var imgA1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            using (var imgA2 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama2.jpg")))
            using (var imgA3 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama3.jpg")))
            using (var imgB1 = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "biden.jpg")))
            {
                using (var faceEncodingA1 = this._FaceRecognition.FaceEncodings(imgA1).ToArray()[0])
                using (var faceEncodingA2 = this._FaceRecognition.FaceEncodings(imgA2).ToArray()[0])
                using (var faceEncodingA3 = this._FaceRecognition.FaceEncodings(imgA3).ToArray()[0])
                using (var faceEncodingB1 = this._FaceRecognition.FaceEncodings(imgB1).ToArray()[0])
                {
                    var facesToCompare = new[]
                    {
                        faceEncodingA2,
                        faceEncodingA3,
                        faceEncodingB1
                    };

                    try
                    {
                        var _ = FaceRecognition.FaceDistances(facesToCompare, null).ToArray();
                        Assert.True(false, $"{nameof(FaceRecognition.FaceDistances)} must throw {typeof(ArgumentNullException)}.");
                    }
                    catch (ArgumentNullException)
                    {
                    }

                    try
                    {
                        var _ = FaceRecognition.FaceDistances(null, faceEncodingA1).ToArray();
                        Assert.True(false, $"{nameof(FaceRecognition.FaceDistances)} must throw {typeof(ArgumentNullException)}.");
                    }
                    catch (ArgumentNullException)
                    {
                    }

                    try
                    {
                        foreach (var encoding in facesToCompare)
                            encoding.Dispose();
                        var _ = FaceRecognition.FaceDistances(facesToCompare, faceEncodingA1).ToArray();
                        Assert.True(false, $"{nameof(FaceRecognition.FaceDistances)} must throw {typeof(ObjectDisposedException)} if faceEncodings contains disposed object.");
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
            }
        }

        [Fact]
        public void TestFaceDistanceEmptyLists()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "biden.jpg")))
            {
                var encoding = this._FaceRecognition.FaceEncodings(img).ToArray()[0];

                // empty list
                var facesToCompare = new FaceEncoding[0];

                var distanceResult = FaceRecognition.FaceDistances(facesToCompare, encoding).ToArray();
                Assert.True(distanceResult.Length == 0);

                encoding.Dispose();
            }
        }

        [Fact]
        public void TestFaceEncodings()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var encodings = this._FaceRecognition.FaceEncodings(img).ToArray();
                Assert.True(encodings.Length == 1);
                Assert.True(encodings[0].Size == 128);

                foreach (var encoding in encodings)
                    encoding.Dispose();
            }
        }

        [Fact]
        public void TestFaceLandmarks()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var faceLandmarks = this._FaceRecognition.FaceLandmark(img).ToArray();

                var parts = new[]
                {
                    FacePart.Chin,
                    FacePart.LeftEyebrow,
                    FacePart.RightEyebrow,
                    FacePart.NoseBridge,
                    FacePart.NoseTip,
                    FacePart.LeftEye,
                    FacePart.RightEye,
                    FacePart.TopLip,
                    FacePart.BottomLip
                };

                foreach (var facePart in faceLandmarks[0].Keys)
                    if (!parts.Contains(facePart))
                        Assert.True(false, $"{facePart} does not contain.");

                var points = new[]
                {
                    new Point(369, 220),
                    new Point(372, 254),
                    new Point(378, 289),
                    new Point(384, 322),
                    new Point(395, 353),
                    new Point(414, 382),
                    new Point(437, 407),
                    new Point(464, 424),
                    new Point(495, 428),
                    new Point(527, 420),
                    new Point(552, 399),
                    new Point(576, 372),
                    new Point(594, 344),
                    new Point(604, 314),
                    new Point(610, 282),
                    new Point(613, 250),
                    new Point(615, 219)
                };

                var facePartPoints = faceLandmarks[0][FacePart.Chin].ToArray();
                for (var index = 0; index < facePartPoints.Length; index++)
                    Assert.True(facePartPoints[index].Point == points[index]);
            }
        }

        [Fact]
        public void TestFaceLandmarksSmallModel()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var faceLandmarks = this._FaceRecognition.FaceLandmark(img, null, PredictorModel.Small).ToArray();

                var parts = new[]
                {
                    FacePart.NoseTip,
                    FacePart.LeftEye,
                    FacePart.RightEye
                };

                foreach (var facePart in faceLandmarks[0].Keys)
                    if (!parts.Contains(facePart))
                        Assert.True(false, $"{facePart} does not contain.");

                var points = new[]
                {
                    new Point(496, 295)
                };

                var facePartPoints = faceLandmarks[0][FacePart.NoseTip].ToArray();
                for (var index = 0; index < facePartPoints.Length; index++)
                    Assert.True(facePartPoints[index].Point == points[index]);
            }
        }

        [Fact]
        public void TestFaceLocations()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var detectedFaces = this._FaceRecognition.FaceLocations(img).ToArray();
                Assert.True(detectedFaces.Length == 1);
                Assert.True(detectedFaces[0].Top == 142);
                Assert.True(detectedFaces[0].Right == 617);
                Assert.True(detectedFaces[0].Bottom == 409);
                Assert.True(detectedFaces[0].Left == 349);
            }
        }

        [Fact]
        public void TestLoadImageFile()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                Assert.True(img.Height == 1137);
                Assert.True(img.Width == 910);
            }
        }

        [Fact]
        public void TestLoadBitmap()
        {
            Location mono;
            Location color;
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama_8bppIndexed.bmp"), Mode.Greyscale))
                mono = this._FaceRecognition.FaceLocations(img).ToArray().FirstOrDefault();
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama_24bppRgb.bmp")))
                color = this._FaceRecognition.FaceLocations(img).ToArray().FirstOrDefault();

            var targets = new[]
            {
                new { Action = new Func<Bitmap>(() => (Bitmap)System.Drawing.Image.FromFile(Path.Combine(TestImageDirectory, "obama_8bppIndexed.bmp"))), Format = PixelFormat.Format8bppIndexed, Expect = mono },
                new { Action = new Func<Bitmap>(() => (Bitmap)System.Drawing.Image.FromFile(Path.Combine(TestImageDirectory, "obama_24bppRgb.bmp"))),    Format = PixelFormat.Format24bppRgb,    Expect = color },
                // linux looks like to not support loading 32Argb
                // new { Action = new Func<Bitmap>(() => (Bitmap)System.Drawing.Image.FromFile(Path.Combine(TestImageDirectory, "obama_32bppArgb.bmp"))),   Format = PixelFormat.Format32bppArgb,   Expect = color },
                // new { Action = new Func<Bitmap>(() =>
                // {
                //     using(var tmp = (Bitmap)System.Drawing.Image.FromFile(Path.Combine(TestImageDirectory, "obama_32bppArgb.bmp")))
                //     {
                //         var bitmap = new Bitmap(tmp.Width,tmp.Height,PixelFormat.Format32bppRgb );
                //         var rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, tmp.Size);
                //         using(var g = Graphics.FromImage(bitmap))
                //             g.DrawImage(tmp, rect,rect, GraphicsUnit.Pixel);
                //         return bitmap;
                //     }
                // }), Format = PixelFormat.Format32bppRgb, Expect = color }
            };
            foreach (var target in targets)
            {
                using (var bitmap = target.Action.Invoke())
                {
                    Assert.True(bitmap.PixelFormat == target.Format);
                    using (var img = FaceRecognition.LoadImage(bitmap))
                    {
                        Assert.True(img.Height == 1137);
                        Assert.True(img.Width == 910);

                        var location = this._FaceRecognition.FaceLocations(img).ToArray().FirstOrDefault();
                        Assert.True(location.Left == target.Expect.Left);
                        Assert.True(location.Top == target.Expect.Top);
                        Assert.True(location.Right == target.Expect.Right);
                        Assert.True(location.Bottom == target.Expect.Bottom);
                    }
                }
            }
        }

        [Fact]
        public void TestLoadImageFile32Bit()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "32bit.png")))
            {
                Assert.True(img.Height == 1200);
                Assert.True(img.Width == 626);
            }
        }

        [Fact]
        public void TestPartialFaceLocations()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama_partial_face.jpg")))
            {
                var detectedFaces = this._FaceRecognition.FaceLocations(img).ToArray();
                Assert.True(detectedFaces.Length == 1);
                Assert.True(detectedFaces[0].Top == 142);
                Assert.True(detectedFaces[0].Right == 191);
                Assert.True(detectedFaces[0].Bottom == 365);
                Assert.True(detectedFaces[0].Left == 0);
            }

            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama_partial_face2.jpg")))
            {
                var detectedFaces = this._FaceRecognition.FaceLocations(img).ToArray();
                Assert.True(detectedFaces.Length == 1);
                Assert.True(detectedFaces[0].Top == 142);
                Assert.True(detectedFaces[0].Right == 551);
                Assert.True(detectedFaces[0].Bottom == 409);
                Assert.True(detectedFaces[0].Left == 349);
            }
        }

        [Fact]
        public void TestRawFaceLandmarks()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var faceLandmarks = this.RawFaceLandmarks(img).ToArray();
                var exampleLandmark = faceLandmarks[0].GetPart(10);

                Assert.True(faceLandmarks.Length == 1);
                Assert.True(faceLandmarks[0].Parts == 68);
                Assert.True(exampleLandmark.X == 552);
                Assert.True(exampleLandmark.Y == 399);

                foreach (var faceLandmark in faceLandmarks)
                    faceLandmark.Dispose();
            }
        }

        [Fact]
        public void TestRawFaceLocations()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var detectedFaces = this.RawFaceLocations(img).ToArray();
                Assert.True(detectedFaces.Length == 1);
                Assert.True(detectedFaces[0].Rect.Top == 142);
                Assert.True(detectedFaces[0].Rect.Bottom == 409);

                foreach (var detectedFace in detectedFaces)
                    detectedFace.Dispose();
            }
        }

        [Fact]
        public void TestRawFaceLocations32BitImage()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "32bit.png")))
            {
                var detectedFaces = this.RawFaceLocations(img).ToArray();
                Assert.True(detectedFaces.Length == 1);
                Assert.True(detectedFaces[0].Rect.Top == 290);
                Assert.True(detectedFaces[0].Rect.Bottom == 558);

                foreach (var detectedFace in detectedFaces)
                    detectedFace.Dispose();
            }
        }

        [Fact]
        public void TestRawFaceLocationsBatched()
        {
            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var images = new[] { img, img, img };
                var batchedDetectedFaces = this.RawFaceLocationsBatched(images, 0).ToArray();

                foreach (var detectedFaces in batchedDetectedFaces)
                {
                    var tmp = detectedFaces.ToArray();
                    Assert.True(tmp.Length == 1);
                    Assert.True(tmp[0].Rect.Top == 154);
                    Assert.True(tmp[0].Rect.Bottom == 390);
                }

                foreach (var batchedDetectedFace in batchedDetectedFaces)
                    foreach (var rect in batchedDetectedFace)
                        rect.Dispose();
            }
        }

        #region Helpers

        private static bool AssertAlmostEqual(int actual, int expected, int delta)
        {
            return expected - delta <= actual && actual <= expected + delta;
        }

        private static void DrawAxis(Graphics g, IEnumerable<FacePoint> nose, double roll, double pitch, double yaw, uint size)
        {
            // https://github.com/natanielruiz/deep-head-pose/blob/master/code/utils.py
            // plot_pose_cube
            pitch = pitch * Math.PI / 180;
            yaw = -(yaw * Math.PI / 180);
            roll = roll * Math.PI / 180;

            var tdx = (nose.Max(p => p.Point.X) + nose.Min(p => p.Point.X)) / 2;
            var tdy = (nose.Max(p => p.Point.Y) + nose.Min(p => p.Point.Y)) / 2;

            // X-Axis pointing to right. drawn in red
            var x1 = size * (Math.Cos(yaw) * Math.Cos(roll)) + tdx;
            var y1 = size * (Math.Cos(pitch) * Math.Sin(roll) + Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw)) + tdy;

            // Y-Axis | drawn in green
            // v
            var x2 = size * (-Math.Cos(yaw) * Math.Sin(roll)) + tdx;
            var y2 = size * (Math.Cos(pitch) * Math.Cos(roll) - Math.Sin(pitch) * Math.Sin(yaw) * Math.Sin(roll)) + tdy;

            // Z-Axis (out of the screen) drawn in blue
            var x3 = size * (Math.Sin(yaw)) + tdx;
            var y3 = size * (-Math.Cos(yaw) * Math.Sin(pitch)) + tdy;

            using (var pen = new Pen(Color.Red, 3))
                g.DrawLine(pen, tdx, tdy, (int)x1, (int)y1);
            using (var pen = new Pen(Color.Green, 3))
                g.DrawLine(pen, tdx, tdy, (int)x2, (int)y2);
            using (var pen = new Pen(Color.Blue, 3))
                g.DrawLine(pen, tdx, tdy, (int)x3, (int)y3);
        }
        
        private void EyeBlinkDetect(EyeBlinkDetector eyeBlinkDetector, PredictorModel model)
        {
            try
            {
                this._FaceRecognition.CustomEyeBlinkDetector = eyeBlinkDetector;
                Assert.Equal(this._FaceRecognition.CustomEyeBlinkDetector, eyeBlinkDetector);

                var groundTruth = new[]
                {
                    new { Path = Path.Combine(TestImageDirectory, "EyeBlink", "Adele_Haenel_Cannes_2016.jpg"),        Left = true,  Right = false },
                    new { Path = Path.Combine(TestImageDirectory, "EyeBlink", "Adele_Haenel_Cannes_2016_mirror.jpg"), Left = false, Right = true },
                };

                foreach (var gt in groundTruth)
                    using (var image = FaceRecognition.LoadImageFile(gt.Path))
                    {
                        var landmark = this._FaceRecognition.FaceLandmark(image, null, model).ToArray()[0];
                        this._FaceRecognition.EyeBlinkDetect(landmark, out var leftBlink, out var rightBlink);
                        Assert.True(leftBlink == gt.Left, $"Failed to detect '{gt.Path}' for left eye");
                        Assert.True(rightBlink == gt.Right, $"Failed to detect '{gt.Path}' for right eye");
                    }
            }
            finally
            {
                this._FaceRecognition.CustomEyeBlinkDetector = null;
            }
        }

        private void FaceLandmark(string testName, PredictorModel model, bool useKnownLocation)
        {
            const int pointSize = 2;

            var path = Path.Combine(TestImageDirectory, TwoPersonFile);

            foreach (var mode in new[] { Mode.Rgb, Mode.Greyscale })
            {
                using (var image = FaceRecognition.LoadImageFile(path, mode))
                {
                    IEnumerable<Location> knownLocations = null;
                    if (useKnownLocation)
                        knownLocations = this._FaceRecognition.FaceLocations(image).ToArray();

                    var landmarks = this._FaceRecognition.FaceLandmark(image, knownLocations, model).ToArray();
                    Assert.True(landmarks.Length > 1, $"{mode}");

                    foreach (var facePart in Enum.GetValues(typeof(FacePart)).Cast<FacePart>())
                        using (var bitmap = System.Drawing.Image.FromFile(path))
                        {
                            var draw = false;
                            using (var g = Graphics.FromImage(bitmap))
                                foreach (var landmark in landmarks)
                                    if (landmark.ContainsKey(facePart))
                                    {
                                        draw = true;
                                        foreach (var p in landmark[facePart].Select((point, i) => point.Point).ToArray())
                                            g.DrawEllipse(Pens.GreenYellow, p.X - pointSize, p.Y - pointSize, pointSize * 2, pointSize * 2);
                                    }

                            if (draw)
                            {
                                var directory = Path.Combine(ResultDirectory, testName);
                                Directory.CreateDirectory(directory);

                                var dst = Path.Combine(directory, $"{facePart}-{mode}-known_{useKnownLocation}.bmp");
                                bitmap.Save(dst, System.Drawing.Imaging.ImageFormat.Bmp);
                            }
                        }

                    using (var bitmap = System.Drawing.Image.FromFile(path))
                    {
                        using (var g = Graphics.FromImage(bitmap))
                            foreach (var landmark in landmarks)
                                foreach (var points in landmark.Values)
                                    foreach (var p in points)
                                    {
                                        g.DrawEllipse(Pens.GreenYellow, p.Point.X - pointSize, p.Point.Y - pointSize, pointSize * 2, pointSize * 2);
                                    }

                        var directory = Path.Combine(ResultDirectory, testName);
                        Directory.CreateDirectory(directory);

                        var dst = Path.Combine(directory, $"All-{mode}-known_{useKnownLocation}.bmp");
                        bitmap.Save(dst, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                }
            }
        }

        private void FaceLocation(string testName, int numberOfTimesToUpsample, Model model)
        {
            var path = Path.Combine(TestImageDirectory, TwoPersonFile);

            foreach (var mode in new[] { Mode.Rgb, Mode.Greyscale })
            {
                using (var image = FaceRecognition.LoadImageFile(path, mode))
                {
                    var locations = this._FaceRecognition.FaceLocations(image, numberOfTimesToUpsample, model).ToArray();
                    Assert.True(locations.Length > 1, $"{mode}");

                    using (var bitmap = System.Drawing.Image.FromFile(path))
                    {
                        using (var g = Graphics.FromImage(bitmap))
                            foreach (var l in locations)
                                g.DrawRectangle(Pens.GreenYellow, l.Left, l.Top, l.Right - l.Left, l.Bottom - l.Top);

                        var directory = Path.Combine(ResultDirectory, testName);
                        Directory.CreateDirectory(directory);

                        var dst = Path.Combine(directory, $"All-{mode}-{numberOfTimesToUpsample}.bmp");
                        bitmap.Save(dst, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                }
            }
        }

        private IEnumerable<FullObjectDetection> RawFaceLandmarks(Image img, IEnumerable<Location> faceLocations = null, PredictorModel predictorModel = PredictorModel.Large)
        {
            var method = this._FaceRecognition.GetType().GetMethod("RawFaceLandmarks", BindingFlags.Instance | BindingFlags.NonPublic);
            return method.Invoke(this._FaceRecognition, new object[] { img, faceLocations, predictorModel, Model.Hog }) as IEnumerable<FullObjectDetection>;
        }

        private IEnumerable<MModRect> RawFaceLocations(Image img, int numberOfTimesToUpsample = 1, Model model = Model.Hog)
        {
            var method = this._FaceRecognition.GetType().GetMethod("RawFaceLocations", BindingFlags.Instance | BindingFlags.NonPublic);
            return method.Invoke(this._FaceRecognition, new object[] { img, numberOfTimesToUpsample, model }) as IEnumerable<MModRect>;
        }

        private IEnumerable<IEnumerable<MModRect>> RawFaceLocationsBatched(IEnumerable<Image> faceImages, int numberOfTimesToUpsample = 1, int batchSize = 128)
        {
            var method = this._FaceRecognition.GetType().GetMethod("RawFaceLocationsBatched", BindingFlags.Instance | BindingFlags.NonPublic);
            return method.Invoke(this._FaceRecognition, new object[] { faceImages, numberOfTimesToUpsample, batchSize }) as IEnumerable<IEnumerable<MModRect>>;
        }

        #endregion

        #endregion

        #region IDisposable Members

        private bool _IsDisposed;

        /// <summary>
        /// Releases all resources used by this <see cref="FaceRecognitionTest"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by this <see cref="FaceRecognitionTest"/>.
        /// </summary>
        /// <param name="disposing">Indicate value whether <see cref="IDisposable.Dispose"/> method was called.</param>
        private void Dispose(bool disposing)
        {
            if (this._IsDisposed)
            {
                return;
            }

            this._IsDisposed = true;

            if (disposing)
            {
                var array = this.ModelFiles.ToArray();

                // Remove all files
                foreach (var file in array)
                {
                    var path = Path.Combine(ModelTempDirectory, file);
                    if (File.Exists(path))
                        File.Delete(path);
                }

                if (Directory.Exists(ModelTempDirectory))
                    Directory.Delete(ModelTempDirectory);

                this._FaceRecognition?.Dispose();
            }
        }

        #endregion

    }

}