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

        #region Fields

        private static FaceRecognition FaceRecognition;

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(Benchmark);
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

                FaceRecognition = FaceRecognition.Create(directory);

                var testImages = new[]
                {
                    "obama-240p.jpg",
                    "obama-480p.jpg",
                    "obama-720p.jpg",
                    "obama-1080p.jpg"
                };

                Console.WriteLine("Benchmarks");
                Console.WriteLine();

                foreach (var image in testImages)
                {
                    var size = image.Split('-')[1].Split('.')[0];
                    Console.WriteLine($"Timings at {size}:");

                    var faceLocations = RunTest(image, SetupLocateFaces, TestLocateFaces);
                    Console.WriteLine($" - Face locations: {faceLocations.Item1:F4}s ({faceLocations.Item2:F2} fps)");
                    var faceLandmarks = RunTest(image, SetupFaceLandmarks, TestFaceLandmarks);
                    Console.WriteLine($" - Face landmarks: {faceLandmarks.Item1:F4}s ({faceLandmarks.Item2:F2} fps)");
                    var encodeFace = RunTest(image, SetupEncodeFace, TestEncodeFace);
                    Console.WriteLine($" - Encode face (inc. landmarks): {encodeFace.Item1:F4}s ({encodeFace.Item2:F2} fps)");
                    var endToEnd = RunTest(image, SetupEndToEnd, TestEndToEnd);
                    Console.WriteLine($" - End-to-end: {endToEnd.Item1:F4}s ({endToEnd.Item2:F2} fps)");
                    Console.WriteLine();
                }

                return 0;
            });

            app.Execute(args);
        }

        #region Helpers

        private static Tuple<double, double> RunTest<T>(string path, Func<string, T> setup, Action<T> test, int iterationsPerTest = 5, int testsToRun = 10)
        {
            var image = setup(path);

            var iteration = new Func<double>(() =>
            {
                var sw = new Stopwatch();
                sw.Start();
                for (var count = 0; count < iterationsPerTest; count++)
                    test(image);
                sw.Stop();

                return sw.ElapsedMilliseconds;
            });

            var fastestExecution = Enumerable.Repeat(0, testsToRun).Select(i => iteration()).Min();
            var executionTime = fastestExecution / 1000 / iterationsPerTest;
            var fps = 1.0 / executionTime;

            (image as IDisposable)?.Dispose();

            return new Tuple<double, double>(executionTime, fps);
        }

        private static Tuple<Image, Location[]> SetupEncodeFace(string path)
        {
            var image = Image.Load(path);
            var locations = FaceRecognition.FaceLocations(image).ToArray();
            return new Tuple<Image, Location[]>(image, locations);
        }

        private static Image SetupEndToEnd(string path)
        {
            return Image.Load(path);
        }

        private static Tuple<Image, Location[]> SetupFaceLandmarks(string path)
        {
            var image = Image.Load(path);
            var locations = FaceRecognition.FaceLocations(image).ToArray();
            return new Tuple<Image, Location[]>(image, locations);
        }

        private static Image SetupLocateFaces(string path)
        {
            return Image.Load(path);
        }

        private static void TestEncodeFace(Tuple<Image, Location[]> tuple)
        {
            var encoding = FaceRecognition.FaceEncodings(tuple.Item1, tuple.Item2).First();
        }

        private static void TestEndToEnd(Image image)
        {
            var encoding = FaceRecognition.FaceEncodings(image).First();
        }

        private static void TestFaceLandmarks(Tuple<Image, Location[]> tuple)
        {
            var landmarks = FaceRecognition.FaceLandmark(tuple.Item1, tuple.Item2).First();
        }

        private static void TestLocateFaces(Image image)
        {
            var faceLocations = FaceRecognition.FaceLocations(image).ToArray();
        }

        #endregion

        #endregion

    }

}
