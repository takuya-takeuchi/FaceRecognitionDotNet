using System.Drawing;
using System.Linq;
using FaceRecognitionDotNet;

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

                var ageRange = new[]
                {
                    "(0, 2)", 
                    "(4, 6)", 
                    "(8, 13)", 
                    "(15, 20)",
                    "(25, 32)",
                    "(38, 43)", 
                    "(48, 53)",
                    "(60, 100)"
                };
                var age = ageRange[fr.PredictAge(image, box)];
                var gender = fr.PredictGender(image, box);

                var agePos = new PointF(box.Left + 10, box.Top + 10);
                var genderPos = new PointF(box.Left + 10, box.Bottom - 50);
                g.DrawString(gender.ToString(), SystemFonts.CaptionFont, Brushes.Blue, agePos );
                g.DrawString(age, SystemFonts.CaptionFont, Brushes.Green, genderPos);

                bitmap.Save("result.png");
            }
        }

        #endregion

    }

}
