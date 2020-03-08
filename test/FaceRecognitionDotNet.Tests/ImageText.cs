using System.IO;
using Xunit;

namespace FaceRecognitionDotNet.Tests
{

    public class ImageTest
    {

        #region Fields

        private const string ResultDirectory = "Result";

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

            using (var img = FaceRecognition.LoadImageFile(Path.Combine("TestImages", "obama.jpg")))
            {
                var directory = Path.Combine(ResultDirectory, testName);
                Directory.CreateDirectory(directory);

                foreach (var target in targets)
                {
                    var path = Path.Combine(directory, target.Name);
                    img.Save(path, target.Format);
                }
            }
        }

    }

}