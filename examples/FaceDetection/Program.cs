using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FaceRecognitionDotNet;
using Microsoft.Extensions.CommandLineUtils;

namespace FaceDetection
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
            app.Name = nameof(FaceDetection);
            app.HelpOption("-h|--help");

            var directoryOption = app.Option("-d|--directory", "The directory path which includes image files", CommandOptionType.SingleValue);
            var cpuOption = app.Option("-c|--cpus", "The number of CPU cores to use in parallel. -1 means \"use all in system\"", CommandOptionType.SingleValue);
            var modelOption = app.Option("-m|--model", "Which face detection model to use. Options are \"hog\" or \"cnn\".", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var imageToCheck = "";
                if (directoryOption.HasValue())
                    imageToCheck = directoryOption.Value();

                var strCpus = "1";
                if (cpuOption.HasValue())
                    strCpus = cpuOption.Value();

                var strModel = "Hog";
                if (modelOption.HasValue())
                    strModel = modelOption.Value();

                if (!Enum.TryParse<Model>(strModel, true, out var model))
                {
                    app.ShowHelp();
                    Console.WriteLine($"\n\tmodel: {strModel}");
                    return -1;
                }

                if (!int.TryParse(strCpus, out var cpus))
                {
                    app.ShowHelp();
                    Console.WriteLine($"\n\tcpus: {strCpus}");
                    return -1;
                }

                if (!Directory.Exists(imageToCheck))
                {
                    app.ShowHelp();
                    Console.WriteLine($"\n\tdirectory: {imageToCheck}");
                    return -1;
                }

                var directory = Path.GetFullPath("models");
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"Please check whether model directory '{directory}' exists");
                    return -1;
                }

                _FaceRecognition = FaceRecognition.Create(directory);

                if (Directory.Exists(imageToCheck))
                    switch (cpus)
                    {
                        case 1:
                            foreach (var imageFile in ImageFilesInFolder(imageToCheck))
                                TestImage(imageFile, model);
                            break;
                        default:
                            ProcessImagesInProcessPool(ImageFilesInFolder(imageToCheck), cpus, model);
                            break;
                    }
                else
                    TestImage(imageToCheck, model);

                return 0;
            });

            app.Execute(args);
        }

        #region Helpers

        private static IEnumerable<string> ImageFilesInFolder(string folder)
        {
            return Directory.GetFiles(folder)
                            .Where(s => Regex.IsMatch(Path.GetExtension(s), "(jpg|jpeg|png)$", RegexOptions.Compiled));
        }

        private static void PrintResult(string filename, Location location)
        {
            Console.WriteLine($"{filename},{location.Top},{location.Right},{location.Bottom},{location.Left}");
        }

        private static void ProcessImagesInProcessPool(IEnumerable<string> imagesToCheck, int numberOfCpus, Model model)
        {
            if (numberOfCpus == -1)
                numberOfCpus = Environment.ProcessorCount;

            var files = imagesToCheck.ToArray();
            var functionParameters = files.Select(s => new Tuple<string, Model>(s, model)).ToArray();

            var total = functionParameters.Length;
            var option = new ParallelOptions
            {
                MaxDegreeOfParallelism = numberOfCpus
            };

            Parallel.For(0, total, option, i =>
            {
                var t = functionParameters[i];
                TestImage(t.Item1, t.Item2);
            });
        }

        private static void TestImage(string imageToCheck, Model model)
        {
            using (var unknownImage = FaceRecognition.LoadImageFile(imageToCheck))
            {
                var faceLocations = _FaceRecognition.FaceLocations(unknownImage, 0, model).ToArray();

                foreach (var faceLocation in faceLocations)
                    PrintResult(imageToCheck, faceLocation);
            }
        }

        #endregion

        #endregion

    }

}
