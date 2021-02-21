using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using DlibDotNet;
using FaceRecognitionDotNet;
using Microsoft.Extensions.CommandLineUtils;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Point = DlibDotNet.Point;

namespace HelenTraining
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
            app.Name = nameof(HelenTraining);
            app.Description = "The program for training helen dataset";
            app.HelpOption("-h|--help");

            app.Command("generate", command =>
            {
                command.HelpOption("-?|-h|--help");
                var paddingOption = command.Option("-p|--padding", "padding of detected face", CommandOptionType.SingleValue);
                var modelsOption = command.Option("-m|--model", "model files directory path", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    if (!modelsOption.HasValue())
                    {
                        Console.WriteLine("model option is missing");
                        app.ShowHelp();
                        return -1;
                    }

                    if (!paddingOption.HasValue())
                    {
                        Console.WriteLine("padding option is missing");
                        app.ShowHelp();
                        return -1;
                    }

                    var directory = modelsOption.Value();
                    if (!Directory.Exists(directory))
                    {
                        Console.WriteLine($"'{directory}' is not found");
                        app.ShowHelp();
                        return -1;
                    }

                    if (!int.TryParse(paddingOption.Value(), out var padding))
                    {
                        Console.WriteLine($"padding '{paddingOption.Value()}' is not integer");
                        app.ShowHelp();
                        return -1;
                    }

                    Console.WriteLine($"Model: {directory}");
                    Console.WriteLine($"Padding: {padding}");

                    _FaceRecognition = FaceRecognition.Create(directory);

                    const string extractPath = "helen";
                    var zips = new[]
                    {
                        new{ Zip = "annotation.zip", IsImage = false, Directory = "annotation" },
                        new{ Zip = "helen_1.zip",    IsImage = true,  Directory = "helen_1" },
                        new{ Zip = "helen_2.zip",    IsImage = true,  Directory = "helen_2" },
                        new{ Zip = "helen_3.zip",    IsImage = true,  Directory = "helen_3" },
                        new{ Zip = "helen_4.zip",    IsImage = true,  Directory = "helen_4" },
                        new{ Zip = "helen_5.zip",    IsImage = true,  Directory = "helen_5" }
                    };

                    Directory.CreateDirectory(extractPath);

                    foreach (var zip in zips)
                    {
                        if (!Directory.Exists(Path.Combine(extractPath, zip.Directory)))
                            ZipFile.ExtractToDirectory(zip.Zip, extractPath);
                    }

                    var annotation = zips.FirstOrDefault(arg => !arg.IsImage);
                    var imageZips = zips.Where(arg => arg.IsImage).ToArray();
                    if (annotation == null)
                        return -1;

                    var images = new List<Image>();
                    foreach (var file in Directory.EnumerateFiles(Path.Combine(extractPath, annotation.Directory)))
                    {
                        Console.WriteLine($"Process: '{file}'");

                        var txt = File.ReadAllLines(file);
                        var filename = txt[0];
                        var jpg = $"{filename}.jpg";
                        foreach (var imageZip in imageZips)
                        {
                            var found = false;
                            var path = Path.Combine(Path.Combine(extractPath, imageZip.Directory, jpg));
                            if (File.Exists(path))
                            {
                                found = true;
                                using (var fi = FaceRecognition.LoadImageFile(path))
                                {
                                    var locations = _FaceRecognition.FaceLocations(fi, 1, Model.Hog).ToArray();
                                    if (locations.Length != 1)
                                    {
                                        Console.WriteLine($"\t'{path}' has {locations.Length} faces.");
                                    }
                                    else
                                    {
                                        var location = locations.First();
                                        var parts = new List<Part>();
                                        for (var i = 1; i < txt.Length; i++)
                                        {
                                            var tmp = txt[i].Split(',').Select(s => s.Trim()).Select(float.Parse).Select(s => (int)s).ToArray();
                                            parts.Add(new Part { X = tmp[0], Y = tmp[1], Name = $"{i - 1:D3}" });
                                        }

                                        var image = new Image
                                        {
                                            File = Path.Combine(imageZip.Directory, jpg),
                                            Box = new Box
                                            {
                                                Left = location.Left - padding,
                                                Top = location.Top - padding,
                                                Width = location.Right - location.Left + 1 + padding * 2,
                                                Height = location.Bottom - location.Top + 1 + padding * 2,
                                                Part = parts.ToArray()
                                            }
                                        };

                                        using (var bitmap = System.Drawing.Image.FromFile(path))
                                        {
                                            var b = image.Box;
                                            using (var g = Graphics.FromImage(bitmap))
                                            {
                                                using (var p = new Pen(Color.Red, bitmap.Width / 400f))
                                                    g.DrawRectangle(p, b.Left, b.Top, b.Width, b.Height);

                                                foreach (var part in b.Part)
                                                    g.FillEllipse(Brushes.GreenYellow, part.X, part.Y, 5, 5);
                                            }

                                            var result = Path.Combine(extractPath, "Result");
                                            Directory.CreateDirectory(result);

                                            bitmap.Save(Path.Combine(result, jpg), ImageFormat.Jpeg);
                                        }

                                        images.Add(image);
                                    }
                                }
                            }

                            if (found)
                                break;
                        }
                    }

                    var dataset = new Dataset
                    {
                        Name = "helen dataset",
                        Comment = "Created by Takuya Takeuchi.",
                        Images = images.ToArray()
                    };

                    var settings = new XmlWriterSettings();
                    using (var sw = new StreamWriter(Path.Combine(extractPath, "helen-dataset.xml"), false, new System.Text.UTF8Encoding(false)))
                    using (var writer = XmlWriter.Create(sw, settings))
                    {
                        writer.WriteProcessingInstruction("xml-stylesheet", @"type=""text/xsl"" href=""image_metadata_stylesheet.xsl""");
                        var serializer = new XmlSerializer(typeof(Dataset));
                        serializer.Serialize(writer, dataset);
                    }

                    return 0;
                });
            });

            app.Command("train", command =>
            {
                command.HelpOption("-?|-h|--help");
                var threadOption = command.Option("-t|--threads", "number of threads", CommandOptionType.SingleValue);
                var xmlOption = command.Option("-x|--xml", "generated xml file from helen dataset", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    if (!xmlOption.HasValue())
                    {
                        Console.WriteLine("xml option is missing");
                        app.ShowHelp();
                        return -1;
                    }

                    if (!threadOption.HasValue())
                    {
                        Console.WriteLine("thread option is missing");
                        app.ShowHelp();
                        return -1;
                    }

                    var xmlFile = xmlOption.Value();
                    if (!File.Exists(xmlFile))
                    {
                        Console.WriteLine($"'{xmlFile}' is not found");
                        app.ShowHelp();
                        return -1;
                    }

                    if (!uint.TryParse(threadOption.Value(), out var thread))
                    {
                        Console.WriteLine($"thread '{threadOption.Value()}' is not integer");
                        app.ShowHelp();
                        return -1;
                    }

                    Dlib.LoadImageDataset(xmlFile, out Array<Array2D<byte>> imagesTrain, out var facesTrain);

                    using (var trainer = new ShapePredictorTrainer())
                    {
                        trainer.NumThreads = thread;
                        trainer.BeVerbose();

                        Console.WriteLine("Start training");
                        using (var predictor = trainer.Train(imagesTrain, facesTrain))
                        {
                            Console.WriteLine("Finish training");

                            var directory = Path.GetDirectoryName(xmlFile);
                            var output = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(xmlFile)}.dat");
                            ShapePredictor.Serialize(predictor, output);
                        }
                    }

                    return 0;
                });
            });

            app.Command("demo", command =>
            {
                command.HelpOption("-?|-h|--help");
                var imageOption = command.Option("-i|--image", "test image file", CommandOptionType.SingleValue);
                var modelOption = command.Option("-m|--model", "model file", CommandOptionType.SingleValue);
                var directoryOption = command.Option("-d|--directory", "model files directory path", CommandOptionType.SingleValue);
                var scaleOption = command.Option("-s|--scale", "scale ration", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    if (!imageOption.HasValue())
                    {
                        Console.WriteLine("image option is missing");
                        app.ShowHelp();
                        return -1;
                    }

                    if (!directoryOption.HasValue())
                    {
                        Console.WriteLine("directory option is missing");
                        app.ShowHelp();
                        return -1;
                    }

                    if (!modelOption.HasValue())
                    {
                        Console.WriteLine("model option is missing");
                        app.ShowHelp();
                        return -1;
                    }

                    var scale = 1.0f;
                    if (scaleOption.HasValue() && !float.TryParse(scaleOption.Value(), NumberStyles.Float, null, out scale))
                    {
                        Console.WriteLine("scale option must be float value");
                        app.ShowHelp();
                        return -1;
                    }

                    var modelFile = modelOption.Value();
                    if (!File.Exists(modelFile))
                    {
                        Console.WriteLine($"'{modelFile}' is not found");
                        app.ShowHelp();
                        return -1;
                    }

                    var imageFile = imageOption.Value();
                    if (!File.Exists(imageFile))
                    {
                        Console.WriteLine($"'{imageFile}' is not found");
                        app.ShowHelp();
                        return -1;
                    }

                    var directory = directoryOption.Value();
                    if (!Directory.Exists(directory))
                    {
                        Console.WriteLine($"'{directory}' is not found");
                        app.ShowHelp();
                        return -1;
                    }

                    _FaceRecognition = FaceRecognition.Create(directory);

                    using (var predictor = ShapePredictor.Deserialize(modelFile))
                    using (var image = FaceRecognition.LoadImageFile(imageFile))
                    using (var mat = Dlib.LoadImageAsMatrix<RgbPixel>(imageFile))
                    using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(imageFile))
                    using (var org = new Bitmap((int)(bitmap.Width * scale), (int)(bitmap.Height * scale)))
                    using (var white = new Bitmap((int)(bitmap.Width * scale), (int)(bitmap.Height * scale)))
                    using (var g = Graphics.FromImage(org))
                    using (var gw = Graphics.FromImage(white))
                    {
                        var loc = _FaceRecognition.FaceLocations(image).FirstOrDefault();
                        if (loc == null)
                        {
                            Console.WriteLine("No face is detected");
                            return 0;
                        }

                        g.DrawImage(bitmap, new System.Drawing.Rectangle(0 ,0, org.Width, org.Height), new System.Drawing.Rectangle(0,0,bitmap.Width,bitmap.Height), GraphicsUnit.Pixel);
                        gw.Clear(Color.White);

                        var b = new DlibDotNet.Rectangle(loc.Left, loc.Top, loc.Right, loc.Bottom);
                        var detection = predictor.Detect(mat, b);

                        using (var p = new Pen(Color.Red, bitmap.Width * scale / 200f))
                        {
                            g.DrawRectangle(p, loc.Left * scale, b.Top * scale, b.Width * scale, b.Height * scale);
                            gw.DrawRectangle(p, loc.Left * scale, b.Top * scale, b.Width * scale, b.Height * scale);
                        }

                        DrawLandmarkPoints(g, gw, scale, Enumerable.Range(0, (int)detection.Parts).Select(s => detection.GetPart((uint)s)).ToArray());

                        org.Save("demo.jpg", ImageFormat.Jpeg);
                        white.Save("white.jpg", ImageFormat.Jpeg);
                    }

                    return 0;
                });
            });

            app.Command("check", command =>
            {
                command.HelpOption("-?|-h|--help");
                var imageOption = command.Option("-i|--image", "test image file", CommandOptionType.SingleValue);
                var annotationOption = command.Option("-a|--annotation", "annotation file path", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    if (!imageOption.HasValue())
                    {
                        Console.WriteLine("image option is missing");
                        app.ShowHelp();
                        return -1;
                    }

                    if (!annotationOption.HasValue())
                    {
                        Console.WriteLine("annotation option is missing");
                        app.ShowHelp();
                        return -1;
                    }

                    var imageFile = imageOption.Value();
                    if (!File.Exists(imageFile))
                    {
                        Console.WriteLine($"'{imageFile}' is not found");
                        app.ShowHelp();
                        return -1;
                    }

                    var annotation = annotationOption.Value();
                    if (!File.Exists(annotation))
                    {
                        Console.WriteLine($"'{annotation}' is not found");
                        app.ShowHelp();
                        return -1;
                    }

                    using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(imageFile))
                    using (var white = new Bitmap(bitmap.Width, bitmap.Height))
                    using (var g = Graphics.FromImage(bitmap))
                    using (var gw = Graphics.FromImage(white))
                    {
                        var txt = File.ReadAllLines(annotation);

                        var location = new List<Part>();
                        for (var i = 1; i < txt.Length; i++)
                        {
                            var tmp = txt[i].Split(',').Select(s => s.Trim()).Select(float.Parse).Select(s => (int)s).ToArray();
                            location.Add(new Part { X = tmp[0], Y = tmp[1], Name = $"{i - 1}" });
                        }

                        gw.Clear(Color.White);

                        DrawLandmarkPoints(g, gw, 1.0f, location.Select(part => new Point((int)part.X, (int)part.Y)).ToArray());

                        bitmap.Save("check-landmark.jpg", ImageFormat.Jpeg);
                        white.Save("check-landmark-white.jpg", ImageFormat.Jpeg);
                    }

                    return 0;
                });
            });

            app.Execute(args);
        }

        #region Helpers

        private static void DrawLandmarkPoints(Graphics graphics, Graphics graphicsWhite, float scale, IList<Point> landmark)
        {
            for (int i = 0, parts = landmark.Count; i < parts; i++)
            {
                var part = landmark[i];
                graphics.FillEllipse(Brushes.GreenYellow, part.X * scale, part.Y * scale, 15, 15);
                graphicsWhite.DrawString($"{i}", SystemFonts.DefaultFont, Brushes.Black, part.X * scale, part.Y * scale);
            }
        }

        #endregion

        #endregion

    }

}
