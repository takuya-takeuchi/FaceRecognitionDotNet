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

        private static FaceRecognition _FaceRecognition;

        private static bool _UseCnn = false;

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(Benchmark);
            app.Description = "The program for measure face encoding performance";
            app.HelpOption("-h|--help");

            var modelsOption = app.Option("-m|--model", "model files directory path", CommandOptionType.SingleValue);
            var cnnOption = app.Option("-c|--cnn", "use cnn", CommandOptionType.NoValue);

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

                _UseCnn = cnnOption.HasValue();

                _FaceRecognition = FaceRecognition.Create(directory);

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

        private static Tuple<double, double> RunTest<T>(string path, Func<string, T> setup, Action<T> test, int iterationsPerTest = 5, int testsToRun = 10, bool useCnn = false)
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
            var image = FaceRecognition.LoadImageFile(path);
            var model = _UseCnn ? Model.Cnn : Model.Hog;
            var locations = _FaceRecognition.FaceLocations(image, model: model).ToArray();
            return new Tuple<Image, Location[]>(image, locations);
        }

        private static Image SetupEndToEnd(string path)
        {
            return FaceRecognition.LoadImageFile(path);
        }

        private static Tuple<Image, Location[]> SetupFaceLandmarks(string path)
        {
            var image = FaceRecognition.LoadImageFile(path);
            var model = _UseCnn ? Model.Cnn : Model.Hog;
            var locations = _FaceRecognition.FaceLocations(image, model: model).ToArray();
            return new Tuple<Image, Location[]>(image, locations);
        }

        private static Image SetupLocateFaces(string path)
        {
            return FaceRecognition.LoadImageFile(path);
        }

        private static void TestEncodeFace(Tuple<Image, Location[]> tuple)
        {
            var model = _UseCnn ? Model.Cnn : Model.Hog;
            var encoding = _FaceRecognition.FaceEncodings(tuple.Item1, tuple.Item2, model: model);
            foreach (var faceEncoding in encoding)
                faceEncoding.Dispose();
        }

        private static void TestEndToEnd(Image image)
        {
            var model = _UseCnn ? Model.Cnn : Model.Hog;
            var encoding = _FaceRecognition.FaceEncodings(image, model: model);
            foreach (var faceEncoding in encoding)
                faceEncoding.Dispose();
        }

        private static void TestFaceLandmarks(Tuple<Image, Location[]> tuple)
        {
            var model = _UseCnn ? Model.Cnn : Model.Hog;
            var landmarks = _FaceRecognition.FaceLandmark(tuple.Item1, tuple.Item2, model: model).First();
        }

        private static void TestLocateFaces(Image image)
        {
            var model = _UseCnn ? Model.Cnn : Model.Hog;
            var faceLocations = _FaceRecognition.FaceLocations(image, model: model).ToArray();
        }

        #endregion

        #endregion

    }

}
