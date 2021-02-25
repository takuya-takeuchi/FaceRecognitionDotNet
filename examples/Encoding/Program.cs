/*
 * This sample program is ported by C# from https://github.com/ageitgey/face_recognition/blob/master/examples/benchmark.py.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FaceRecognitionDotNet;
using Microsoft.Extensions.CommandLineUtils;

namespace Benchmark
{

    internal class Program
    {

        #region Methods

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(Benchmark);
            app.Description = "The program for measure face encoding performance";
            app.HelpOption("-h|--help");

            var modelsOption = app.Option("-m|--model", "model files directory path", CommandOptionType.SingleValue);
            var imageOption = app.Option("-i|--image", "target image", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!modelsOption.HasValue())
                {
                    Console.WriteLine("--model is not specified");
                    app.ShowHelp();
                    return -1;
                }

                var directory = modelsOption.Value();
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"{directory} does not exist");
                    app.ShowHelp();
                    return -1;
                }

                var image = imageOption.Value();
                if (!File.Exists(image))
                {
                    Console.WriteLine($"{image} does not exist");
                    app.ShowHelp();
                    return -1;
                }                

                using (var fr = FaceRecognition.Create(directory))
                using (var im = FaceRecognition.LoadImageFile(image))
                {
                    var locations = fr.FaceLocations(im);
                    foreach (var l in locations)
                        Console.WriteLine($"l: {l.Left}, t: {l.Top}, r: {l.Right}, b: {l.Bottom}");
                        
                    foreach (var l in locations)
                    {
                        var encodings = fr.FaceEncodings(im, new [] {l});
                        foreach (var e in encodings)
                        {
                            Console.WriteLine($"{string.Join(", ", e.GetRawEncoding().Select(s => s.ToString()))}");
                            e.Dispose();
                        }
                    }
                }

                return 0;
            });

            app.Execute(args);
        }

        #endregion

    }

}
