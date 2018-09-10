using System;
using System.Diagnostics;
using System.IO;
using FaceRecognitionDotNet;
using Microsoft.Extensions.CommandLineUtils;

namespace FaceEncodingPerformance
{

    internal class Program
    {

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(FaceEncodingPerformance);
            app.Description = "The program for measure face encoding performance";
            app.HelpOption("-h|--help");

            var loopOption = app.Option("-l|--loop", "loop count (An integer)", CommandOptionType.SingleValue);
            var fileOption = app.Option("-f|--file", "test image file path", CommandOptionType.SingleValue);
            var modelsOption = app.Option("-m|--model", "model files directory path", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (!loopOption.HasValue())
                {
                    app.ShowHelp();
                    return -1;
                }

                if (!fileOption.HasValue())
                {
                    app.ShowHelp();
                    return -1;
                }

                if (!modelsOption.HasValue())
                {
                    app.ShowHelp();
                    return -1;
                }

                if (!int.TryParse(loopOption.Value(), out var loop))
                {
                    app.ShowHelp();
                    return -1;
                }

                var path = fileOption.Value();
                if (!File.Exists(path))
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

                using (var fr = FaceRecognition.Create(directory))
                using (var image = FaceRecognition.LoadImageFile(path))
                {
                    var sw = new Stopwatch();
                    sw.Start();

                    for (var l = 0; l < loop; l++)
                    {
                        var encodings = fr.FaceEncodings(image);
                        if (encodings == null)
                            continue;

                        foreach (var encoding in encodings)
                            encoding.Dispose();
                    }

                    sw.Stop();

                    var total = sw.ElapsedMilliseconds;
                    var average = total / loop;
                    Console.WriteLine($"Total: {total} [ms], Average: {average} [ms]");
                }

                return 0;
            });

            app.Execute(args);
        }

    }

}
