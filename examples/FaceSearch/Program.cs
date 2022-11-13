using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.Extensions.CommandLineUtils;

using FaceRecognitionDotNet;
using FaceRecognitionDotNet.Extensions;

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

                if (!encodings.Any())
                {
                    Console.WriteLine($"There is no face encodings");
                    return -1;
                }

                var searches = new []
                {
                    new { Search = new AnnoySearch(256) as Search,         Name = "Annoy Search" },
                    new { Search = new KNearestNeighborSearch() as Search, Name = "K Nearest Neighbor Search" }
                };
                
                foreach (var search in searches)
                {
                    var name = search.Name;

                    Console.WriteLine($"Start: {name}");
                    using (var s = search.Search)
                    {
                        var item = encodings[0];

                        Console.WriteLine("Start: Add encoding");
                        sw.Reset();
                        sw.Start();
                        foreach (var encoding in encodings)
                            s.Add(encoding.Item2, encoding.Item3);
                        sw.Stop();
                        Console.WriteLine($"Finish: Add encoding [{sw.ElapsedMilliseconds} ms]");

                        Console.WriteLine("Start: Build index");
                        sw.Reset();
                        sw.Start();
                        s.Build();
                        sw.Stop();
                        Console.WriteLine($"Finish: Build index [{sw.ElapsedMilliseconds} ms]");

                        Console.WriteLine($"Start: Query: {item.Item1}");
                        sw.Reset();
                        sw.Start();
                        var results = s.Query(item.Item3, k);
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
                    Console.WriteLine($"Finish: {name}");
                    Console.WriteLine();
                }

                foreach (var encoding in encodings)
                    encoding.Item3.Dispose();

                return 0;
            });

            app.Execute(args);
        }

        #endregion

    }

}
