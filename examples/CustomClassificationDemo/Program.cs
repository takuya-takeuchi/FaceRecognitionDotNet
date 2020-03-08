using System.Drawing;
using System.IO;
using System.Linq;
using FaceRecognitionDotNet;
using FaceRecognitionDotNet.Extensions;

namespace CustomClassificationDemo
{

    internal class Program
    {

        #region Methods

        private static void Main()
        {
            var imageFile = "DianaPrincessOfWales_1997_36.jpg";

            using (var fr = FaceRecognition.Create("models"))
            using (var image = FaceRecognition.LoadImageFile(imageFile))
            using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(imageFile))
            using (var g = Graphics.FromImage(bitmap))
            {
                var box = fr.FaceLocations(image, model: Model.Cnn).FirstOrDefault();

                using (var p = new Pen(Color.Red, bitmap.Width / 200f))
                    g.DrawRectangle(p, box.Left, box.Top, box.Right - box.Left, box.Bottom - box.Top);

                // load custom estimator
                using (var ageEstimator = new SimpleAgeEstimator(Path.Combine("models", "adience-age-network.dat")))
                using (var genderEstimator = new SimpleGenderEstimator(Path.Combine("models", "utkface-gender-network.dat")))
                {
                    fr.CustomAgeEstimator = ageEstimator;
                    fr.CustomGenderEstimator = genderEstimator;

                    var ageRange = ageEstimator.Groups.Select(range => $"({range.Start}, {range.End})").ToArray();
                    var age = ageRange[fr.PredictAge(image, box)];
                    var gender = fr.PredictGender(image, box);

                    var agePos = new PointF(box.Left + 10, box.Top + 10);
                    var genderPos = new PointF(box.Left + 10, box.Bottom - 50);
                    g.DrawString(gender.ToString(), SystemFonts.CaptionFont, Brushes.Blue, agePos);
                    g.DrawString(age, SystemFonts.CaptionFont, Brushes.Green, genderPos);

                    bitmap.Save("result.png");
                }
            }
        }

        #endregion

    }

}
