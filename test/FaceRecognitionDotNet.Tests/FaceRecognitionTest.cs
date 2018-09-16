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
        public void CompareFacesFalse()
        {
            var bidenUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/64/Biden_2013.jpg";
            var bidenFile = "480px-Biden_2013.jpg";
            var obamaUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/President_Barack_Obama.jpg";
            var obamaFile = "480px-President_Barack_Obama.jpg";

            var path1 = Path.Combine(ImageDirectory, bidenFile);
            if (!File.Exists(path1))
            {
                var url = $"{bidenUrl}/{bidenFile}";
                var binary = new HttpClient().GetByteArrayAsync(url).Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path1, binary);
            }

            var path2 = Path.Combine(ImageDirectory, obamaFile);
            if (!File.Exists(path2))
            {
                var url = $"{obamaUrl}/{obamaFile}";
                var binary = new HttpClient().GetByteArrayAsync(url).Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path2, binary);
            }

            bool atLeast1Time = false;

            using (var image1 = FaceRecognition.LoadImageFile(path1))
            using (var image2 = FaceRecognition.LoadImageFile(path2))
            {
                var endodings1 = this._FaceRecognition.FaceEncodings(image1).ToArray();
                var endodings2 = this._FaceRecognition.FaceEncodings(image2).ToArray();

                foreach (var encoding in endodings1)
                    foreach (var compareFace in FaceRecognition.CompareFaces(endodings2, encoding))
                    {
                        atLeast1Time = true;
                        Assert.IsFalse(compareFace);
                    }

                foreach (var encoding in endodings1)
                    encoding.Dispose();
                foreach (var encoding in endodings2)
                    encoding.Dispose();
            }

            if (!atLeast1Time)
                Assert.Fail("Assert check did not execute");
        }

        [TestMethod]
        public void CompareFacesTrue()
        {
            var obamaUrl1 = "https://upload.wikimedia.org/wikipedia/commons/3/3f";
            var obamaFile1 = "Barack_Obama_addresses_LULAC_7-8-08.JPG";
            var obamaUrl2 = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/President_Barack_Obama.jpg";
            var obamaFile2 = "480px-President_Barack_Obama.jpg";

            var path1 = Path.Combine(ImageDirectory, obamaFile1);
            if (!File.Exists(path1))
            {
                var url = $"{obamaUrl1}/{obamaFile1}";
                var binary = new HttpClient().GetByteArrayAsync(url).Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path1, binary);
            }

            var path2 = Path.Combine(ImageDirectory, obamaFile2);
            if (!File.Exists(path2))
            {
                var url = $"{obamaUrl2}/{obamaFile2}";
                var binary = new HttpClient().GetByteArrayAsync(url).Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path2, binary);
            }

            bool atLeast1Time = false;

            using (var image1 = FaceRecognition.LoadImageFile(path1))
            using (var image2 = FaceRecognition.LoadImageFile(path2))
            {
                var endodings1 = this._FaceRecognition.FaceEncodings(image1).ToArray();
                var endodings2 = this._FaceRecognition.FaceEncodings(image2).ToArray();

                foreach (var encoding in endodings1)
                    foreach (var compareFace in FaceRecognition.CompareFaces(endodings2, encoding))
                    {
                        atLeast1Time = true;
                        Assert.IsTrue(compareFace);
                    }

                foreach (var encoding in endodings1)
                    encoding.Dispose();
                foreach (var encoding in endodings2)
                    encoding.Dispose();
            }

            if (!atLeast1Time)
                Assert.Fail("Assert check did not execute");
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
        public void FaceDistance()
        {
            var bidenUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/64/Biden_2013.jpg";
            var bidenFile = "480px-Biden_2013.jpg";
            var obamaUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/President_Barack_Obama.jpg";
            var obamaFile = "480px-President_Barack_Obama.jpg";

            var path1 = Path.Combine(ImageDirectory, bidenFile);
            if (!File.Exists(path1))
            {
                var url = $"{bidenUrl}/{bidenFile}";
                var binary = new HttpClient().GetByteArrayAsync(url).Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path1, binary);
            }

            var path2 = Path.Combine(ImageDirectory, obamaFile);
            if (!File.Exists(path2))
            {
                var url = $"{obamaUrl}/{obamaFile}";
                var binary = new HttpClient().GetByteArrayAsync(url).Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path2, binary);
            }

            bool atLeast1Time = false;

            using (var image1 = FaceRecognition.LoadImageFile(path1))
            using (var image2 = FaceRecognition.LoadImageFile(path2))
            {
                var endodings1 = this._FaceRecognition.FaceEncodings(image1).ToArray();
                var endodings2 = this._FaceRecognition.FaceEncodings(image2).ToArray();

                foreach (var e1 in endodings1)
                    foreach (var e2 in endodings2)
                    {
                        atLeast1Time = true;
                        var distance = FaceRecognition.FaceDistance(e1, e2);
                        Assert.IsTrue(distance > 0.6d);
                    }

                foreach (var encoding in endodings1)
                    encoding.Dispose();
                foreach (var encoding in endodings2)
                    encoding.Dispose();
            }

            if (!atLeast1Time)
                Assert.Fail("Assert check did not execute");
        }

        [TestMethod]
        public void FaceEncodings()
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
                var encodings = this._FaceRecognition.FaceEncodings(image).ToArray();
                Assert.IsTrue(encodings.Length > 1, "");

                foreach (var encoding in encodings)
                    encoding.Dispose();

                foreach (var encoding in encodings)
                    Assert.IsTrue(encoding.IsDisposed, $"{typeof(FaceEncoding)} should be already disposed.");
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

            var array2D = DlibDotNet.Dlib.LoadImage<RgbPixel>(path);
            var bytes = array2D.ToBytes();

            var image = FaceRecognition.LoadImage(bytes, array2D.Rows, array2D.Columns, 3);
            Assert.IsTrue(image.Width == 419, $"Width of {path} is wrong");
            Assert.IsTrue(image.Height == 600, $"Height of {path} is wrong");

            image.Dispose();
            Assert.IsTrue(image.IsDisposed, $"{typeof(Image)} should be already disposed.");
        }

        [TestMethod]
        public void LoadImageCheckIdentity()
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

            var image1 = FaceRecognition.LoadImageFile(path);

            var array2D = DlibDotNet.Dlib.LoadImage<RgbPixel>(path);
            var bytes = array2D.ToBytes();
            var image2 = FaceRecognition.LoadImage(bytes, array2D.Rows, array2D.Columns, 3);

            var location1 = this._FaceRecognition.FaceLocations(image1).ToArray();
            var location2 = this._FaceRecognition.FaceLocations(image2).ToArray();

            Assert.IsTrue(location1.Length == location2.Length, $"FaceRecognition.FaceLocations returns different results for {nameof(location1)} and {nameof(location2)}.");

            for (var index = 0; index < location1.Length; index++)
            {
                Assert.IsTrue(location1[index] == location2[index], 
                    $"{nameof(location1)}[{nameof(index)}] does not equal to {nameof(location2)}[{nameof(index)}].");
            }

            image2.Dispose();
            image1.Dispose();
            Assert.IsTrue(image2.IsDisposed, $"{nameof(image2)} should be already disposed.");
            Assert.IsTrue(image1.IsDisposed, $"{nameof(image1)} should be already disposed.");
        }

        [TestMethod]
        public void LoadImageFile()
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

            var image = FaceRecognition.LoadImageFile(path);
            Assert.IsTrue(image.Width == 419, $"Width of {path} is wrong");
            Assert.IsTrue(image.Height == 600, $"Height of {path} is wrong");

            image.Dispose();
            Assert.IsTrue(image.IsDisposed, $"{typeof(Image)} should be already disposed.");
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

        [TestMethod]
        public void SerializeDeserializeBinaryFormatter()
        {
            const string testName = nameof(this.SerializeDeserializeBinaryFormatter);
            var directory = Path.Combine(ResultDirectory, testName);
            Directory.CreateDirectory(directory);

            var path = Path.Combine(ImageDirectory, TwoPersonFile);
            if (!File.Exists(path))
            {
                var binary = new HttpClient().GetByteArrayAsync($"{TwoPersonUrl}/{TwoPersonFile}").Result;

                Directory.CreateDirectory(ImageDirectory);
                File.WriteAllBytes(path, binary);
            }

            using (var image = FaceRecognition.LoadImageFile(path))
            {
                var encodings = this._FaceRecognition.FaceEncodings(image).ToArray();
                Assert.IsTrue(encodings.Length > 1, "");

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
                    Assert.IsTrue(Math.Abs(distance) < double.Epsilon);
                }

                foreach (var encoding in encodings)
                    encoding.Dispose();

                foreach (var encoding in encodings)
                    Assert.IsTrue(encoding.IsDisposed, $"{typeof(FaceEncoding)} should be already disposed.");
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
                            g.DrawRectangle(Pens.GreenYellow, l.Left, l.Top, l.Right - l.Left, l.Bottom - l.Top);

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
