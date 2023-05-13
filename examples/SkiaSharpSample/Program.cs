using System;
using System.IO;
using System.Linq;
using System.Net.Http;

using Microsoft.Extensions.CommandLineUtils;
using SkiaSharp;

namespace SkiaSharpSample
{

    internal class Program
    {

        #region Fields

        private static FaceRecognitionDotNet.FaceRecognition _FaceRecognition;

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(SkiaSharpSample);
            app.Description = "The program for measure face encoding performance";
            app.HelpOption("-h|--help");

            var modelsOption = app.Option("-m|--model", "model files directory path", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!modelsOption.HasValue())
                {
                    app.ShowHelp();
                    return -1;
                }

                var directory = modelsOption.Value();
                if (!Directory.Exists(directory))
                {
                    app.ShowHelp();
                    return -1;
                }

                _FaceRecognition = FaceRecognitionDotNet.FaceRecognition.Create(directory);

                var testImages = new[]
                {
                    "obama-240p.jpg",
                    "obama-480p.jpg",
                    "obama-720p.jpg",
                    "obama-1080p.jpg"
                };

                const string url = "https://upload.wikimedia.org/wikipedia/commons/9/9d/Barack_Obama.jpg";
                var binary = new HttpClient().GetByteArrayAsync(url).Result;

                using (var bitmap = SKBitmap.Decode(binary))
                using (var searchImage = FaceRecognitionDotNet.Extensions.Skia.FaceRecognition.LoadImage(bitmap))
                {
                    var searchEncodings = _FaceRecognition.FaceEncodings(searchImage);

                    foreach (var path in testImages)
                    {
                        var targetImage = FaceRecognitionDotNet.FaceRecognition.LoadImageFile(path);
                        var targetEncoding = _FaceRecognition.FaceEncodings(targetImage);

                        var distance = FaceRecognitionDotNet.FaceRecognition.FaceDistance(searchEncodings.First(), targetEncoding.First());
                        Console.WriteLine($"Distance: {distance} for {path}");

                        foreach (var encoding in targetEncoding) encoding.Dispose();
                    }

                    foreach (var encoding in searchEncodings) encoding.Dispose();
                }

                return 0;
            });

            app.Execute(args);
        }

        #endregion

    }

}
