using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.Extensions.CommandLineUtils;

using FaceRecognitionDotNet;

namespace FaceSearch
{

    internal class Program
    {

        #region Methods

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(FaceSearch);
            app.Description = "The program for measure face search performance";
            app.HelpOption("-h|--help");

            var modelsOption = app.Option("-m|--model", "model files directory path", CommandOptionType.SingleValue);
            var imageOption = app.Option("-d|--directory", "directory contains face image", CommandOptionType.SingleValue);
            var kOption = app.Option("-k|--topK", "number of query candidate", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!modelsOption.HasValue())
                {
                    Console.WriteLine("--model is not specified");
                    app.ShowHelp();
                    return -1;
                }

                var model = modelsOption.Value();
                if (!Directory.Exists(model))
                {
                    Console.WriteLine($"{model} does not exist");
                    app.ShowHelp();
                    return -1;
                }

                var directory = imageOption.Value();
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"{directory} does not exist");
                    app.ShowHelp();
                    return -1;
                }

                var kValue = kOption.Value();
                if (!uint.TryParse(kValue, NumberStyles.Integer, null, out var k) && k <= 0)
                {
                    Console.WriteLine($"{kValue} should be more than 1");
                    app.ShowHelp();
                    return -1;
                }

                var sw = new Stopwatch();

                Console.WriteLine("Start: Get face encodings");
                sw.Reset();
                sw.Start();

                var encodings = new List<Tuple<string, int, FaceEncoding>>();
                var extensions = new [] { ".jpg", ".png", ".bmp" };
                using (var fr = FaceRecognition.Create(model))
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (!extensions.Contains(Path.GetExtension(file).ToLower()))
                        continue;

                    using (var im = FaceRecognition.LoadImageFile(file))
                    {
                        var locations = fr.FaceLocations(im).ToArray();
                        if (!locations.Any())                            
                        {
                            Console.WriteLine($"{file} does not have any face");
                            continue;
                        }

                        if (locations.Count() > 1)                            
                        {
                            Console.WriteLine($"{file} has multiple faces");
                            continue;
                        }

                        var location = locations.First();
                        var encoding = fr.FaceEncodings(im, new [] {location}).First();
                        encodings.Add(new Tuple<string, int, FaceEncoding>(file, encodings.Count() + 1, encoding));
                    }
                }

                Console.WriteLine($"Total: {encodings.Count()} faces");
                Console.WriteLine($"Finish: Get face encodings [{sw.ElapsedMilliseconds} ms]");
                Console.WriteLine();

                Console.WriteLine("Start: Annoy Search");
                using (var annoySearch = new FaceRecognitionDotNet.Extensions.AnnoySearch())
                {
                    var item = encodings[0];

                    Console.WriteLine("Start: Add encoding");
                    sw.Reset();
                    sw.Start();
                    foreach (var encoding in encodings)
                        annoySearch.Add(encoding.Item2, encoding.Item3);
                    sw.Stop();
                    Console.WriteLine($"Finish: Add encoding [{sw.ElapsedMilliseconds} ms]");

                    Console.WriteLine("Start: Build index");
                    sw.Reset();
                    sw.Start();
                    annoySearch.Build();
                    sw.Stop();
                    Console.WriteLine($"Finish: Build index [{sw.ElapsedMilliseconds} ms]");

                    Console.WriteLine($"Start: Query: {item.Item1}");
                    sw.Reset();
                    sw.Start();
                    var results = annoySearch.Query(item.Item3, k);
                    sw.Stop();
                    Console.WriteLine($"Finish: Query [{sw.ElapsedMilliseconds} ms]");

                    foreach (var result in results)
                    {
                        var tuple = encodings.FirstOrDefault(t => t.Item2 == result.Key);
                        if (tuple != null)
                            Console.WriteLine($"{result.Key}: [{tuple.Item1}: {result.Value}]");
                        else
                            Console.WriteLine($"{result.Key}: {result.Value}");
                    }
                }
                Console.WriteLine("Finish: Annoy Search");
                Console.WriteLine();

                Console.WriteLine("Start: Linear Search");
                {
                    var item = encodings[0];
                    var results = new List<Tuple<string, int, double>>();

                    Console.WriteLine($"Start: Query: {item.Item1}");
                    sw.Reset();
                    sw.Start();
                    foreach (var encoding in encodings)
                        results.Add(new Tuple<string, int, double>(encoding.Item1, encoding.Item2, FaceRecognition.FaceDistance(item.Item3, encoding.Item3)));
                    results.Sort((tuple1, tuple2) => tuple1.Item3.CompareTo(tuple2.Item3));
                    sw.Stop();
                    Console.WriteLine($"Finish: Query [{sw.ElapsedMilliseconds} ms]");

                    for (var index = 0; index < k; index++)
                    {
                        var result = results[index];
                        Console.WriteLine($"{result.Item2}: [{result.Item1}: {result.Item3}]");
                    }
                }
                Console.WriteLine("Finish: Linear Search");

                foreach (var encoding in encodings)
                    encoding.Item3.Dispose();

                return 0;
            });

            app.Execute(args);
        }

        #endregion

    }

}
