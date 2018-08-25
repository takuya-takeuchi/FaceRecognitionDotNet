using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FaceRecognitionDotNet.Tests
{

    [TestClass]
    public class FaceRecognitionTest
    {

        #region Fields

        private FaceRecognition _FaceRecognition;

        private const string ImageDirectory = "Images";

        private const string ModelDirectory = "Models";

        private const string ModelTempDirectory = "TempModels";

        private IList<string> ModelFiles = new List<string>();

        private const string ModelBaseUrl = "https://github.com/ageitgey/face_recognition_models/raw/master/face_recognition_models/models";

        private const string ResultDirectory = "Result";

        private const string TwoPersonUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e4/Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

        private const string TwoPersonFile = "419px-Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

        #endregion

        #region Methods

        [TestCleanup]
        public void Cleanup()
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

        [TestMethod]
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
                    Assert.Fail($"{Path.Combine(ModelTempDirectory, array[j])} is missing and Create method should throw exception.");
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

        [TestMethod]
        public void CreateFail2()
        {
            var tempModelDirectory = "Temp";
            FaceRecognition faceRecognition = null;

            try
            {
                faceRecognition = FaceRecognition.Create(tempModelDirectory);
                Assert.Fail($"{tempModelDirectory} directory is missing and Create method should throw exception.");
            }
            catch (DirectoryNotFoundException)
            {
            }
            finally
            {
                faceRecognition?.Dispose();
            }
        }

        [TestMethod]
        public void FaceLandmarkLarge()
        {
            const string testName = nameof(this.FaceLandmarkLarge);
            this.FaceLandmark(testName, PredictorModel.Large);
        }

        [TestMethod]
        public void FaceLandmarkSmall()
        {
            const string testName = nameof(this.FaceLandmarkSmall);
            this.FaceLandmark(testName, PredictorModel.Small);
        }

        [TestMethod]
        public void FaceLocationCnn()
        {
            const string testName = nameof(this.FaceLocationCnn);
            this.FaceLocation(testName, Model.Cnn);
        }

        [TestMethod]
        public void FaceLocationHog()
        {
            const string testName = nameof(this.FaceLocationHog);
            this.FaceLocation(testName, Model.Hog);
        }

        [TestInitialize]
        public void Initialize()
        {
            var faceRecognition = typeof(FaceRecognition);
            var type = faceRecognition.Assembly.GetTypes().FirstOrDefault(t => t.Name == "FaceRecognitionModels");
            if (type == null)
                Assert.Fail("FaceRecognition.FaceRecognitionModels is not found.");

            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

            var models = new List<string>();
            foreach (var method in methods)
            {
                var result = method.Invoke(null, BindingFlags.Public | BindingFlags.Static, null, null, null) as string;
                if (string.IsNullOrWhiteSpace(result))
                    Assert.Fail($"{method.Name} does not return {typeof(string).FullName} value or return null or whitespace value.");

                models.Add(result);

                var path = Path.Combine(ModelDirectory, result);
                if (File.Exists(path))
                    continue;

                var binary = new HttpClient().GetByteArrayAsync($"{ModelBaseUrl}/{result}").Result;

                Directory.CreateDirectory(ModelDirectory);
                File.WriteAllBytes(path, binary);
            }

            foreach (var model in models)
                this.ModelFiles.Add(model);

            this._FaceRecognition = FaceRecognition.Create(ModelDirectory);
        }

        [TestMethod]
        public void LoadImage()
        {
            const string url = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e4/Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";
            const string file = "419px-Official_portrait_of_President_Obama_and_Vice_President_Biden_2012.jpg";

            var path = Path.Combine(ImageDirectory, file);
            if (!File.Exists(path))
            {
                var binary = new HttpClient().GetByteArrayAsync($"{url}/{file}").Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path, binary);
            }

            using (var image = FaceRecognition.LoadImageFile(path))
            {
                Assert.IsTrue(image.Width == 419, $"Width of {path} is wrong");
                Assert.IsTrue(image.Height == 600, $"Height of {path} is wrong");
            }
        }

        [TestMethod]
        public void LoadImageFail()
        {
            try
            {
                FaceRecognition.LoadImageFile("test.bmp");
                Assert.Fail("test.bmp directory is missing and LoadImageFile method should throw exception.");
            }
            catch (FileNotFoundException)
            {
            }
        }

        #region Helpers

        private void FaceLandmark(string testName, PredictorModel model)
        {
            const int pointSize = 2;

            var path = Path.Combine(ImageDirectory, TwoPersonFile);
            if (!File.Exists(path))
            {
                var binary = new HttpClient().GetByteArrayAsync($"{TwoPersonUrl}/{TwoPersonFile}").Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path, binary);
            }

            using (var image = FaceRecognition.LoadImageFile(path))
            {
                var landmarks = this._FaceRecognition.FaceLandmark(image, null, model).ToArray();
                Assert.IsTrue(landmarks.Length > 1, "");

                foreach (var facePart in Enum.GetValues(typeof(FacePart)).Cast<FacePart>())
                    using (var bitmap = System.Drawing.Image.FromFile(path))
                    {
                        var draw = false;
                        using (var g = Graphics.FromImage(bitmap))
                            foreach (var landmark in landmarks)
                                if (landmark.ContainsKey(facePart))
                                {
                                    draw = true;
                                    foreach (var p in landmark[facePart].ToArray())
                                        g.DrawEllipse(Pens.GreenYellow, p.X - pointSize, p.Y - pointSize, pointSize * 2, pointSize * 2);
                                }

                        if (draw)
                        {
                            var directory = Path.Combine(ResultDirectory, testName);
                            Directory.CreateDirectory(directory);

                            var dst = Path.Combine(directory, $"{facePart}.bmp");
                            bitmap.Save(dst, ImageFormat.Bmp);
                        }
                    }

                using (var bitmap = System.Drawing.Image.FromFile(path))
                {
                    using (var g = Graphics.FromImage(bitmap))
                        foreach (var landmark in landmarks)
                            foreach (var points in landmark.Values)
                                foreach (var p in points)
                                {
                                    g.DrawEllipse(Pens.GreenYellow, p.X - pointSize, p.Y - pointSize, pointSize * 2, pointSize * 2);
                                }

                    var directory = Path.Combine(ResultDirectory, testName);
                    Directory.CreateDirectory(directory);

                    var dst = Path.Combine(directory, "All.bmp");
                    bitmap.Save(dst, ImageFormat.Bmp);
                }
            }
        }

        private void FaceLocation(string testName, Model model)
        {
            var path = Path.Combine(ImageDirectory, TwoPersonFile);
            if (!File.Exists(path))
            {
                var binary = new HttpClient().GetByteArrayAsync($"{TwoPersonUrl}/{TwoPersonFile}").Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path, binary);
            }

            using (var image = FaceRecognition.LoadImageFile(path))
            {
                var locations = this._FaceRecognition.FaceLocations(image, 1, model).ToArray();
                Assert.IsTrue(locations.Length > 1, "");

                using (var bitmap = System.Drawing.Image.FromFile(path))
                {
                    using (var g = Graphics.FromImage(bitmap))
                        foreach (var l in locations)
                            g.DrawRectangle(Pens.GreenYellow,l.Left, l.Top, l.Right - l.Left, l.Bottom - l.Top);

                    var directory = Path.Combine(ResultDirectory, testName);
                    Directory.CreateDirectory(directory);

                    var dst = Path.Combine(directory, "All.bmp");
                    bitmap.Save(dst, ImageFormat.Bmp);
                }
            }
        }

        #endregion

        #endregion

    }

}
