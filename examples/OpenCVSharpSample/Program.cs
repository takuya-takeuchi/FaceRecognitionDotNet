using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using FaceRecognitionDotNet;
using Microsoft.Extensions.CommandLineUtils;
using OpenCvSharp;

namespace OpenCVSharpSample
{

    internal class Program
    {

        #region Fields

        private static FaceRecognition _FaceRecognition;

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(OpenCVSharpSample);
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

                _FaceRecognition = FaceRecognition.Create(directory);

                var testImages = new[]
                {
                    "obama-240p.jpg",
                    "obama-480p.jpg",
                    "obama-720p.jpg",
                    "obama-1080p.jpg"
                };

                const string url = "https://upload.wikimedia.org/wikipedia/commons/9/9d/Barack_Obama.jpg";
                var binary = new HttpClient().GetByteArrayAsync(url).Result;

                using (var mat = Cv2.ImDecode(binary, ImreadModes.Color))
                {
                    var bytes = new byte[mat.Rows * mat.Cols * mat.ElemSize()];
                    Marshal.Copy(mat.Data, bytes, 0, bytes.Length);

                    using (var searchImage = FaceRecognition.LoadImage(bytes, mat.Rows, mat.Cols, mat.ElemSize()))
                    {
                        var searchEncodings = _FaceRecognition.FaceEncodings(searchImage);

                        foreach (var path in testImages)
                        {
                            var targetImage = FaceRecognition.LoadImageFile(path);
                            var targetEncoding = _FaceRecognition.FaceEncodings(targetImage);

                            var distance = FaceRecognition.FaceDistance(searchEncodings.First(), targetEncoding.First());
                            Console.WriteLine($"Distance: {distance} for {path}");

                            foreach (var encoding in targetEncoding) encoding.Dispose();
                        }

                        foreach (var encoding in searchEncodings) encoding.Dispose();
                    }
                }

                return 0;
            });

            app.Execute(args);
        }

        #endregion

    }

}
