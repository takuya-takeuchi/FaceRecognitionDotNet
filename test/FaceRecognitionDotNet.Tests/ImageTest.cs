using System;
using System.IO;
using Xunit;

namespace FaceRecognitionDotNet.Tests
{

    public class ImageTest
    {

        #region Fields

        private const string ResultDirectory = "Result";

        private const string TestImageDirectory = "TestImages";

        #endregion

        [Fact]
        public void Save()
        {
            const string testName = nameof(this.Save);

            var targets = new[]
            {
                new { Name = "saved.bmp", Format = ImageFormat.Bmp },
                new { Name = "saved.jpg", Format = ImageFormat.Jpeg },
                new { Name = "saved.png", Format = ImageFormat.Png },
            };

            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var directory = Path.Combine(ResultDirectory, testName);
                if (Directory.Exists((directory)))
                    Directory.Delete(directory, true);

                foreach (var target in targets)
                {
                    var path = Path.Combine(directory, target.Name);
                    img.Save(path, target.Format);
                }
            }
        }

        [Fact]
        public void SaveException()
        {
            const string testName = nameof(this.SaveException);

            var targets = new[]
            {
                new { Name = "saved.bmp", Format = ImageFormat.Bmp },
                new { Name = "saved.jpg", Format = ImageFormat.Jpeg },
                new { Name = "saved.png", Format = ImageFormat.Png },
            };

            using (var img = FaceRecognition.LoadImageFile(Path.Combine(TestImageDirectory, "obama.jpg")))
            {
                var directory = Path.Combine(ResultDirectory, testName);
                if (Directory.Exists((directory)))
                    Directory.Delete(directory, true);

                foreach (var target in targets)
                {
                    try
                    {
                        img.Save(null, target.Format);
                        Assert.True(false, $"{nameof(img.Save)} method should throw exception.");
                    }
                    catch (ArgumentNullException)
                    {
                    }
                }
            }
        }

        [Fact]
        public void ToBitmap()
        {
            var path = Path.Combine(TestImageDirectory, "obama.jpg");
            using (var img = FaceRecognition.LoadImageFile(path))
            using (var bitmap = img.ToBitmap())
            {
                Assert.Equal(img.Width, bitmap.Width);
                Assert.Equal(img.Height, bitmap.Height);
            }
        }

        [Fact]
        public void ToBitmapGrayscale()
        {
            var path = Path.Combine(TestImageDirectory, "obama.jpg");
            using (var img = FaceRecognition.LoadImageFile(path, Mode.Greyscale))
            {
                try
                {
                    using (var bitmap = img.ToBitmap())
                    {
                        Assert.Equal(img.Width, bitmap.Width);
                        Assert.Equal(img.Height, bitmap.Height);
                    }

                    Assert.True(false, $"{nameof(img.ToBitmap)} method should throw {nameof(NotSupportedException)}.");
                }
                catch (NotSupportedException)
                {
                }
            }
        }

    }

}