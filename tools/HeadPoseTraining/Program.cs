using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DlibDotNet;
using MathNet.Numerics.Data.Matlab;
using Microsoft.Extensions.CommandLineUtils;
using ShellProgressBar;
using Point = System.Drawing.Point;

namespace HeadPoseTraining
{

    internal class Program
    {

        #region Methods

        private static int Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(HeadPoseTraining);
            app.Description = "The program for training 300W-LP dataset";
            app.HelpOption("-h|--help");

            app.Command("train", command =>
            {
                const double toleranceDefault = 0.001d;
                const double gammaDefault = 0.1d;
                const Pose poseDefault = Pose.All;
                const uint rangeDefault = 10;

                var datasetOption = command.Option("-d|--dataset", "The directory of dataset", CommandOptionType.SingleValue);
                var toleranceOption = command.Option("-t|--tolerance", $"The tolerance. Default is {toleranceDefault}", CommandOptionType.SingleValue);
                var gammaOption = command.Option("-g|--gamma", $"The gamma. Default is {gammaDefault}", CommandOptionType.SingleValue);
                var rangeOption = command.Option("-r|--range", $"The range for difference between predict and ground truth. Default is {rangeDefault}", CommandOptionType.SingleValue);
                var poseOption = command.Option("-p|--pose", $"The target pose of training. Default is {poseDefault}", CommandOptionType.SingleValue);
                var outputOption = command.Option("-o|--output", $"The output directory path.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var dataset = datasetOption.Value();
                    if (!datasetOption.HasValue() || !Directory.Exists(dataset))
                    {
                        Console.WriteLine("dataset does not exist");
                        return -1;
                    }

                    var tolerance = toleranceDefault;
                    if (toleranceOption.HasValue() && !double.TryParse(toleranceOption.Value(), NumberStyles.Float, Thread.CurrentThread.CurrentCulture.NumberFormat, out tolerance))
                    {
                        Console.WriteLine("tolerance is invalid value");
                        return -1;
                    }

                    var gamma = gammaDefault;
                    if (gammaOption.HasValue() && !double.TryParse(gammaOption.Value(), NumberStyles.Float, Thread.CurrentThread.CurrentCulture.NumberFormat, out gamma))
                    {
                        Console.WriteLine("gamma is invalid value");
                        return -1;
                    }

                    var range = rangeDefault;
                    if (rangeOption.HasValue() && !uint.TryParse(rangeOption.Value(), out range))
                    {
                        Console.WriteLine("range is invalid value");
                        return -1;
                    }

                    var pose = poseDefault;
                    if (poseOption.HasValue() && !Enum.TryParse(poseOption.Value(), true, out pose))
                    {
                        Console.WriteLine("pose is invalid value");
                        return -1;
                    }

                    var output = "result";
                    if (outputOption.HasValue())
                    {
                        output = outputOption.Value();
                    }

                    Directory.CreateDirectory(output);

                    Console.WriteLine($"  Dataset: {dataset}");
                    Console.WriteLine($"Tolerance: {tolerance}");
                    Console.WriteLine($"    Gamma: {gamma}");
                    Console.WriteLine($"    Range: {range}");
                    Console.WriteLine($"     Pose: {pose}");
                    Console.WriteLine($"   Output: {output}");
                    Console.WriteLine();

                    if (pose == Pose.All)
                    {
                        foreach (var p in Enum.GetValues(typeof(Pose)).Cast<Pose>())
                        {
                            if (p == Pose.All)
                                continue;

                            var name = p.ToString().ToLowerInvariant();
                            var baseName = $"300w-lp-{name}-krls_{tolerance}_{gamma}";

                            var parameter = new Parameter
                            {
                                BaseName = baseName,
                                Dataset = dataset,
                                Output = output,
                                Tolerance = tolerance,
                                Gamma = gamma,
                                Range = range,
                                Pose = p
                            };

                            Train(parameter);
                        }
                    }
                    else
                    {

                        var name = pose.ToString().ToLowerInvariant();
                        var baseName = $"300w-lp-{name}-krls_{tolerance}_{gamma}";

                        var parameter = new Parameter
                        {
                            BaseName = baseName,
                            Dataset = dataset,
                            Output = output,
                            Tolerance = tolerance,
                            Gamma = gamma,
                            Range = range,
                            Pose = pose
                        };

                        Train(parameter);
                    }

                    return 0;
                });
            });

            app.Command("test", command =>
            {
                const uint rangeDefault = 10;
                const Pose poseDefault = Pose.Roll;

                var datasetOption = command.Option("-d|--dataset", "The directory of dataset", CommandOptionType.SingleValue);
                var modelOption = command.Option("-m|--model", "The model file.", CommandOptionType.SingleValue);
                var rangeOption = command.Option("-r|--range", $"The range for difference between predict and ground truth. Default is {rangeDefault}", CommandOptionType.SingleValue);
                var poseOption = command.Option("-p|--pose", $"The target pose of training. Default is {poseDefault}", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var dataset = datasetOption.Value();
                    if (!datasetOption.HasValue() || !Directory.Exists(dataset))
                    {
                        Console.WriteLine("dataset does not exist");
                        return -1;
                    }

                    var model = modelOption.Value();
                    if (!modelOption.HasValue() || !File.Exists(model))
                    {
                        Console.WriteLine("model does not exist");
                        return -1;
                    }

                    var range = rangeDefault;
                    if (rangeOption.HasValue() && !uint.TryParse(rangeOption.Value(), out range))
                    {
                        Console.WriteLine("range is invalid value");
                        return -1;
                    }

                    var pose = poseDefault;
                    if (poseOption.HasValue() && !Enum.TryParse(poseOption.Value(), true, out pose))
                    {
                        Console.WriteLine("pose is invalid value");
                        return -1;
                    }

                    if (pose == Pose.All)
                    {
                        Console.WriteLine("pose 'All' is invalid value");
                        return -1;
                    }

                    Console.WriteLine($"Dataset: {dataset}");
                    Console.WriteLine($"  Model: {model}");
                    Console.WriteLine($"  Range: {range}");
                    Console.WriteLine($"  Pose: {pose}");
                    Console.WriteLine();

                    var parameter = new Parameter
                    {
                        Dataset = dataset,
                        Model = model,
                        Range = range,
                        Pose = pose
                    };

                    Test(parameter);

                    return 0;
                });
            });

            app.Command("eval", command =>
            {
                var imageOption = command.Option("-i|--image", "The image file.", CommandOptionType.SingleValue);
                var matOption = command.Option("-m|--mat", "The mat file.", CommandOptionType.SingleValue);
                var landmarkOption = command.Option("-l|--landmark", "The landmark mat file.", CommandOptionType.SingleValue);
                var rollOption = command.Option("-r|--roll", "The roll model file.", CommandOptionType.SingleValue);
                var pitchOption = command.Option("-p|--pitch", "The pitch model file.", CommandOptionType.SingleValue);
                var yawOption = command.Option("-y|--yaw", "The yaw model file.", CommandOptionType.SingleValue);
                var outputOption = command.Option("-o|--output", "The output directory path.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var image = imageOption.Value();
                    if (!imageOption.HasValue() || !File.Exists(image))
                    {
                        Console.WriteLine("image does not exist");
                        return -1;
                    }

                    var mat = matOption.Value();
                    if (!matOption.HasValue() || !File.Exists(mat))
                    {
                        Console.WriteLine($"mat does not exist");
                        return -1;
                    }

                    var landmark = landmarkOption.Value();
                    if (!landmarkOption.HasValue() || !File.Exists(landmark))
                    {
                        Console.WriteLine("landmark does not exist");
                        return -1;
                    }

                    var roll = rollOption.Value();
                    if (!rollOption.HasValue() || !File.Exists(roll))
                    {
                        Console.WriteLine("roll model file does not exist");
                        return -1;
                    }

                    var pitch = pitchOption.Value();
                    if (!pitchOption.HasValue() || !File.Exists(pitch))
                    {
                        Console.WriteLine("pitch model file does not exist");
                        return -1;
                    }

                    var yaw = yawOption.Value();
                    if (!yawOption.HasValue() || !File.Exists(yaw))
                    {
                        Console.WriteLine("yaw model file does not exist");
                        return -1;
                    }

                    var output = "result";
                    if (outputOption.HasValue())
                    {
                        output = outputOption.Value();
                    }

                    Directory.CreateDirectory(output);

                    Console.WriteLine($"      Image File: {image}");
                    Console.WriteLine($"        Mat File: {mat}");
                    Console.WriteLine($"   Landmark File: {landmark}");
                    Console.WriteLine($"       Roll File: {roll}");
                    Console.WriteLine($"      Pitch File: {pitch}");
                    Console.WriteLine($"        Yaw File: {yaw}");
                    Console.WriteLine($"Output Directory: {output}");
                    Console.WriteLine();

                    var pt2d = MatlabReader.Read<double>(landmark, "pts_2d");
                    var points = GetPoints(pt2d);
                    using (var source = Image.FromFile(image))
                    using (var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb))
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(source, Point.Empty);

                        // https://github.com/natanielruiz/deep-head-pose/blob/master/code/utils.py
                        var poseMatrix = MatlabReader.Read<double>(mat, "Pose_Para");
                        GetRollPitchYaw(poseMatrix, out var rollValue, out var pitchValue, out var yawValue);

                        Console.WriteLine($"       Roll: {rollValue}");
                        Console.WriteLine($"      Pitch: {pitchValue}");
                        Console.WriteLine($"        Yaw: {yawValue}");

                        const int pointSize = 2;
                        foreach (var p in points)
                        {
                            g.DrawEllipse(Pens.GreenYellow, (float)p.X - pointSize, (float)p.Y - pointSize, pointSize * 2, pointSize * 2);
                        }

                        DrawAxis(g, bitmap.Width, bitmap.Height, rollValue, pitchValue, yawValue, 150);

                        var filename = Path.GetFileName(image);
                        bitmap.Save(Path.Combine(output, $"{filename}-gt.jpg"), ImageFormat.Jpeg);
                    }

                    using (var source = Image.FromFile(image))
                    using (var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb))
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        double rollValue;
                        double pitchValue;
                        double yawValue;
                        using (var matrix = GetPointMatrix(points, Pose.Roll))
                        using (var rbk = new RadialBasisKernel<double, Matrix<double>>(0.1, 0, 0))
                        {
                            Krls<double, RadialBasisKernel<double, Matrix<double>>> trainer = null;

                            try
                            {
                                trainer = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(rbk);
                                Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(roll, ref trainer);
                                rollValue = trainer.Operator(matrix);
                            }
                            finally
                            {
                                trainer?.Dispose();
                            }
                        }

                        using (var matrix = GetPointMatrix(points, Pose.Pitch))
                        using (var rbk = new RadialBasisKernel<double, Matrix<double>>(0.1, 0, 0))
                        {
                            Krls<double, RadialBasisKernel<double, Matrix<double>>> trainer = null;

                            try
                            {
                                trainer = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(rbk);
                                Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(pitch, ref trainer);
                                pitchValue = trainer.Operator(matrix);
                            }
                            finally
                            {
                                trainer?.Dispose();
                            }
                        }

                        using (var matrix = GetPointMatrix(points, Pose.Yaw))
                        using (var rbk = new RadialBasisKernel<double, Matrix<double>>(0.1, 0, 0))
                        {
                            Krls<double, RadialBasisKernel<double, Matrix<double>>> trainer = null;

                            try
                            {
                                trainer = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(rbk);
                                Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(yaw, ref trainer);
                                yawValue = trainer.Operator(matrix);
                            }
                            finally
                            {
                                trainer?.Dispose();
                            }
                        }

                        g.DrawImage(source, Point.Empty);

                        Console.WriteLine($" Predicted Roll: {rollValue}");
                        Console.WriteLine($"Predicted Pitch: {pitchValue}");
                        Console.WriteLine($"  Predicted Yaw: {yawValue}");

                        const int pointSize = 2;
                        foreach (var p in points)
                        {
                            g.DrawEllipse(Pens.GreenYellow, (float)p.X - pointSize, (float)p.Y - pointSize, pointSize * 2, pointSize * 2);
                        }

                        DrawAxis(g, bitmap.Width, bitmap.Height, rollValue, pitchValue, yawValue, 150);

                        var filename = Path.GetFileName(image);
                        bitmap.Save(Path.Combine(output, $"{filename}-predicted.jpg"), ImageFormat.Jpeg);
                    }

                    return 0;
                });
            });

            return app.Execute(args);
        }

        #region Helpers

        private static void GetRollPitchYaw(MathNet.Numerics.LinearAlgebra.Matrix<double> poseMatrix, out double roll,
                                                                                                      out double pitch,
                                                                                                      out double yaw)
        {
            roll = poseMatrix[0, 2] * 180 / Math.PI;
            pitch = poseMatrix[0, 0] * 180 / Math.PI;
            yaw = poseMatrix[0, 1] * 180 / Math.PI;
        }

        private static IList<DPoint> GetPoints(MathNet.Numerics.LinearAlgebra.Matrix<double> pointMatrix)
        {
            var points = new List<DPoint>();
            for (var c = 0; c < 68; c++)
            {
                var x = pointMatrix[c, 0];
                var y = pointMatrix[c, 1];
                points.Add(new DPoint(x, y));
            }

            return points;
        }

        private static Matrix<double> GetPointMatrix(IList<DPoint> points, Pose pose)
        {
            // https://cdn-images-1.medium.com/max/1600/1*AbEg31EgkbXSQehuNJBlWg.png
            switch (pose)
            {

                case Pose.Roll:
                    {
                        // Calc angle from 33 to each point except for 33
                        var vector = new List<double>();
                        var p1 = points[33];
                        for (var c = 0; c < 68; c++)
                        {
                            if (c == 33)
                                continue;

                            var p2 = points[c];
                            var distance = Math.Atan((p2.X - p1.X) / (p2.Y - p1.Y));
                            //var distance = Math.Sqrt(Math.Pow(p2.X - p1.X,2) + Math.Pow(p2.Y - p1.Y,2));
                            vector.Add(distance);
                        }

                        // Need not to use Normalization
                        //NormalizeVector(vector);

                        return new Matrix<double>(vector.ToArray(), vector.Count, 1);
                    }
                case Pose.Pitch:
                    {
                        // Calc angle from 33 to each point except for 33
                        var vector = new List<double>();
                        var p1 = points[33];
                        for (var c = 0; c < 68; c++)
                        {
                            if (c == 33)
                                continue;

                            var p2 = points[c];
                            var distance = p2.Y - p1.Y;
                            vector.Add(distance);
                        }

                        NormalizeVector(vector);

                        return new Matrix<double>(vector.ToArray(), vector.Count, 1);
                    }
                case Pose.Yaw:
                    {
                        // Calc angle from 33 to each point except for 33
                        var vector = new List<double>();
                        var p1 = points[33];
                        for (var c = 0; c < 68; c++)
                        {
                            if (c == 33)
                                continue;

                            var p2 = points[c];
                            var distance = p2.X - p1.X;
                            vector.Add(distance);
                        }

                        NormalizeVector(vector);

                        return new Matrix<double>(vector.ToArray(), vector.Count, 1);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(pose), pose, null);
            }
        }

        private static void NormalizeVector(IList<double> vector)
        {
            // z-score normalization
            var count = vector.Count;
            var mean = vector.Average();
            var sum2 = vector.Select(a => a * a).Sum();
            var variance = sum2 / count - mean * mean;
            var std = Math.Sqrt(variance);
            for (var index = 0; index < vector.Count; index++)
                vector[index] = (vector[index] - mean) / std;
        }

        private static void DrawAxis(Graphics g, int width, int height, double roll, double pitch, double yaw, uint size)
        {
            // https://github.com/natanielruiz/deep-head-pose/blob/master/code/utils.py
            // plot_pose_cube
            pitch = pitch * Math.PI / 180;
            yaw = -(yaw * Math.PI / 180);
            roll = roll * Math.PI / 180;

            var tdx = width / 2;
            var tdy = height / 2;

            // X-Axis pointing to right. drawn in red
            var x1 = size * (Math.Cos(yaw) * Math.Cos(roll)) + tdx;
            var y1 = size * (Math.Cos(pitch) * Math.Sin(roll) + Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw)) + tdy;

            // Y-Axis | drawn in green
            // v
            var x2 = size * (-Math.Cos(yaw) * Math.Sin(roll)) + tdx;
            var y2 = size * (Math.Cos(pitch) * Math.Cos(roll) - Math.Sin(pitch) * Math.Sin(yaw) * Math.Sin(roll)) + tdy;

            // Z-Axis (out of the screen) drawn in blue
            var x3 = size * (Math.Sin(yaw)) + tdx;
            var y3 = size * (-Math.Cos(yaw) * Math.Sin(pitch)) + tdy;

            using (var pen = new Pen(Color.Red, 3))
                g.DrawLine(pen, tdx, tdy, (int)x1, (int)y1);
            using (var pen = new Pen(Color.Green, 3))
                g.DrawLine(pen, tdx, tdy, (int)x2, (int)y2);
            using (var pen = new Pen(Color.Blue, 3))
                g.DrawLine(pen, tdx, tdy, (int)x3, (int)y3);
        }

        private static void Load(string directory, Pose pose, out IList<Matrix<double>> images, out IList<double> labels)
        {
            var fileList = new List<string>();
            var imageList = new List<Matrix<double>>();
            var labelList = new List<double>();
            var pointList = new List<double[]>();

            var path = Path.Combine(directory, $"cache_{pose}.dat");
            if (File.Exists(path))
            {
                Console.WriteLine($"Use Cache {path}");

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var s = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    var cache = (Cache)s.Deserialize(fs);

                    foreach (var points in cache.Points)
                    {
                        var tmp = new List<DPoint>();
                        for (var index = 0; index < points.Length; index += 2)
                        {
                            tmp.Add(new DPoint(points[index], points[index + 1]));
                        }

                        var matrix = GetPointMatrix(tmp, pose);
                        imageList.Add(matrix);
                    }
                    labelList.AddRange(cache.Angles);
                }

                images = imageList;
                labels = labelList;

                return;
            }

            foreach (var file in Directory.EnumerateFiles(directory, "*.jpg", SearchOption.AllDirectories))
            {
                var dir = Path.GetDirectoryName(file);
                var mat = Path.ChangeExtension(file, ".mat");
                if (!File.Exists(mat))
                {
                    Console.WriteLine($"{mat} does not exist!!");
                    continue;
                }
                var basename = Path.GetFileNameWithoutExtension(file);
                var landmark = Path.Combine(dir, $"{basename}_pts.mat");
                if (!File.Exists(landmark))
                {
                    Console.WriteLine($"{landmark} does not exist!!");
                    continue;
                }

                // https://github.com/natanielruiz/deep-head-pose/blob/master/code/utils.py
                var poseMatrix = MatlabReader.Read<double>(mat, "Pose_Para");
                GetRollPitchYaw(poseMatrix, out var roll, out var pitch, out var yaw);

                double value = 0;
                switch (pose)
                {
                    case Pose.Roll:
                        value = roll;
                        break;
                    case Pose.Pitch:
                        value = pitch;
                        break;
                    case Pose.Yaw:
                        value = yaw;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pose), pose, null);
                }

                // https://github.com/natanielruiz/deep-head-pose/blob/master/code/utils.py
                // https://github.com/tensorflow/datasets/blob/master/tensorflow_datasets/image/the300w_lp.py
                //var pt2d = MatlabReader.Read<double>(mat, "pt2d");
                var pt2d = MatlabReader.Read<double>(landmark, "pts_2d");
                var points = GetPoints(pt2d);

                var tmp = new List<double>();
                for (var index = 0; index < points.Count; index++)
                {
                    tmp.Add(points[index].X);
                    tmp.Add(points[index].Y);
                }
                pointList.Add(tmp.ToArray());

                // https://cdn-images-1.medium.com/max/1600/1*AbEg31EgkbXSQehuNJBlWg.png
                // Calc angle from 33 to each point except for 33
                var matrix = GetPointMatrix(points, pose);

                fileList.Add(mat);
                imageList.Add(matrix);
                labelList.Add(value);
            }

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Console.WriteLine($"Save Cache {path}");

                var cache = new Cache();
                cache.Points = pointList;
                cache.Angles = labelList;
                var s = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                s.Serialize(fs, cache);
            }

            path = Path.Combine(directory, $"list_{pose}.txt");
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var sw = new StreamWriter(fs))
            {
                Console.WriteLine($"Save List {path}");

                for (var index = 0; index < fileList.Count; index++)
                    sw.WriteLine($"{fileList[index]} : {labelList[index]}");
            }

            images = imageList;
            labels = labelList;
        }

        private static void Test(Parameter parameter)
        {
            try
            {
                IList<Matrix<double>> trainingImages;
                IList<double> trainingLabels;
                IList<Matrix<double>> testingImages;
                IList<double> testingLabels;

                Console.WriteLine("Start load train images");
                Load(Path.Combine(parameter.Dataset, "train"), parameter.Pose, out trainingImages, out trainingLabels);
                Console.WriteLine($"Load train images: {trainingImages.Count}");

                Console.WriteLine("Start load test images");
                Load(Path.Combine(parameter.Dataset, "test"), parameter.Pose, out testingImages, out testingLabels);
                Console.WriteLine($"Load test images: {testingImages.Count}");
                Console.WriteLine();

                using (var rbk = new RadialBasisKernel<double, Matrix<double>>(parameter.Gamma, 0, 0))
                {
                    Krls<double, RadialBasisKernel<double, Matrix<double>>> trainer = null;

                    try
                    {
                        trainer = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(rbk, parameter.Tolerance);
                        Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(parameter.Model, ref trainer);

                        var validationParameter = new ValidationParameter
                        {
                            BaseName = Path.GetFileNameWithoutExtension(parameter.Model),
                            Trainer = trainer,
                            TrainingImages = trainingImages,
                            TrainingLabels = trainingLabels,
                            TestingImages = testingImages,
                            TestingLabels = testingLabels,
                            Range = parameter.Range,
                            UseConsole = true,
                            SaveToXml = false,
                            OutputDiffLog = true,
                            Output = Path.GetDirectoryName(parameter.Model)
                        };

                        Validation(validationParameter, out var trainAccuracy, out var testAccuracy);

                        var validationLog = $"train accuracy: {trainAccuracy:F4}, test accuracy: {testAccuracy:F4}";
                        Console.WriteLine(validationLog);
                        Console.WriteLine();
                    }
                    finally
                    {
                        if (trainer != null) trainer.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void Train(Parameter parameter)
        {
            try
            {
                IList<Matrix<double>> trainingImages;
                IList<double> trainingLabels;
                IList<Matrix<double>> testingImages;
                IList<double> testingLabels;

                Console.WriteLine("Start load train images");
                Load(Path.Combine(parameter.Dataset, "train"), parameter.Pose, out trainingImages, out trainingLabels);
                Console.WriteLine($"Load train images: {trainingImages.Count}");

                Console.WriteLine("Start load test images");
                Load(Path.Combine(parameter.Dataset, "test"), parameter.Pose, out testingImages, out testingLabels);
                Console.WriteLine($"Load test images: {testingImages.Count}");
                Console.WriteLine();

                Console.WriteLine("Training");
                using (var rbk = new RadialBasisKernel<double, Matrix<double>>(parameter.Gamma, 0, 0))
                using (var trainer = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(rbk, parameter.Tolerance))
                {
                    // now we train our object on a few samples of the sinc function.
                    var count = trainingImages.Count;
                    var options = new ProgressBarOptions
                    {
                        ProgressCharacter = '─',
                        ProgressBarOnBottom = true,
                        EnableTaskBarProgress = true
                    };
                    using (var pbar = new ProgressBar(count, "", options))
                    {
                        for (var index = 0; index < count; index++)
                        {
                            var train = trainingImages[index];
                            var label = trainingLabels[index];
                            trainer.Train(train, label);
                            pbar.Tick($"Step {index + 1} of {count}");
                        }
                    }

                    var validationParameter = new ValidationParameter
                    {
                        BaseName = parameter.BaseName,
                        Output = parameter.Output,
                        Trainer = trainer,
                        TrainingImages = trainingImages,
                        TrainingLabels = trainingLabels,
                        TestingImages = testingImages,
                        TestingLabels = testingLabels,
                        Range = parameter.Range,
                        UseConsole = true,
                        SaveToXml = true,
                        OutputDiffLog = true
                    };

                    Validation(validationParameter, out var trainAccuracy, out var testAccuracy);

                    var validationLog = $"train accuracy: {trainAccuracy:F4}, test accuracy: {testAccuracy:F4}";
                    Console.WriteLine(validationLog);
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void Validation(ValidationParameter parameter,
                                       out double trainAccuracy,
                                       out double testAccuracy)
        {
            trainAccuracy = 0;
            testAccuracy = 0;

            var train_diff = new List<Tuple<double, double, double>>();
            var test_diff = new List<Tuple<double, double, double>>();

            Console.WriteLine();
            Console.WriteLine("Validation for Training data");
            var numRight = 0;
            var numWrong = 0;
            var count = parameter.TrainingImages.Count;
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true,
                EnableTaskBarProgress = true
            };
            using (var pbar = new ProgressBar(count, "", options))
            {
                for (var index = 0; index < count; index++)
                {
                    var x = parameter.TrainingImages[index];
                    var y = parameter.TrainingLabels[index];

                    var p = parameter.Trainer.Operator(x);

                    var diff = Math.Abs(p - y);
                    if (Math.Abs(p - y) < parameter.Range)
                        ++numRight;
                    else
                        ++numWrong;

                    train_diff.Add(new Tuple<double, double, double>(y, p, diff));
                    pbar.Tick($"Step {index + 1} of {count}");
                }
            }

            trainAccuracy = numRight / (double)(numRight + numWrong);

            if (parameter.UseConsole)
            {
                Console.WriteLine($"training num_right: {numRight}");
                Console.WriteLine($"training num_wrong: {numWrong}");
                Console.WriteLine($" training accuracy:  {trainAccuracy:F4}");
            }

            Console.WriteLine();
            Console.WriteLine("Validation for Test data");
            numRight = 0;
            numWrong = 0;
            count = parameter.TestingImages.Count;
            using (var pbar = new ProgressBar(count, "", options))
            {
                for (var index = 0; index < parameter.TestingImages.Count; index++)
                {
                    var x = parameter.TestingImages[index];
                    var y = parameter.TestingLabels[index];

                    var p = parameter.Trainer.Operator(x);

                    var diff = Math.Abs(p - y);
                    if (Math.Abs(p - y) < parameter.Range)
                        ++numRight;
                    else
                        ++numWrong;

                    test_diff.Add(new Tuple<double, double, double>(y, p, diff));
                    pbar.Tick($"Step {index + 1} of {count}");
                }
            }

            testAccuracy = numRight / (double)(numRight + numWrong);

            if (parameter.UseConsole)
            {
                Console.WriteLine($"testing num_right: {numRight}");
                Console.WriteLine($"testing num_wrong: {numWrong}");
                Console.WriteLine($" testing accuracy:  {testAccuracy:F4}");
            }

            if (parameter.SaveToXml)
                Krls<double, RadialBasisKernel<double, Matrix<double>>>.Serialize(parameter.Trainer, Path.Combine(parameter.Output, $"{parameter.BaseName}.dat"));

            if (parameter.OutputDiffLog)
            {
                using (var train_fs = new FileStream(Path.Combine(parameter.Output, $"{parameter.BaseName}_train_diff.log"), FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var test_fs = new FileStream(Path.Combine(parameter.Output, $"{parameter.BaseName}_test_diff.log"), FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var train_sw = new StreamWriter(train_fs, Encoding.UTF8))
                using (var test_sw = new StreamWriter(test_fs, Encoding.UTF8))
                {
                    foreach (var v in train_diff)
                        train_sw.WriteLine($"{v.Item1}\t{v.Item2}\t{v.Item3}");
                    foreach (var v in test_diff)
                        test_sw.WriteLine($"{v.Item1}\t{v.Item2}\t{v.Item3}");
                }
            }
        }

        #endregion

        #endregion

        private enum Pose
        {

            Roll,

            Pitch,

            Yaw,

            All

        }

        private sealed class Parameter
        {

            public string Dataset
            {
                get;
                set;
            }

            public string Model
            {
                get;
                set;
            }

            public string BaseName
            {
                get;
                set;
            }

            public string Output
            {
                get;
                set;
            }

            public double Tolerance
            {
                get;
                set;
            }

            public double Gamma
            {
                get;
                set;
            }

            public Pose Pose
            {
                get;
                set;
            }

            public uint Range
            {
                get;
                set;
            }

        }

        private sealed class ValidationParameter
        {

            public string BaseName
            {
                get;
                set;
            }

            public string Output
            {
                get;
                set;
            }

            public Krls<double, RadialBasisKernel<double, Matrix<double>>> Trainer
            {
                get;
                set;
            }

            public IList<Matrix<double>> TrainingImages
            {
                get;
                set;
            }


            public IList<double> TrainingLabels
            {
                get;
                set;
            }


            public IList<Matrix<double>> TestingImages
            {
                get;
                set;
            }


            public IList<double> TestingLabels
            {
                get;
                set;
            }


            public uint Range
            {
                get;
                set;
            }

            public bool UseConsole
            {
                get;
                set;
            }

            public bool SaveToXml
            {
                get;
                set;
            }

            public bool OutputDiffLog
            {
                get;
                set;
            }

        }

        [Serializable]
        private struct Cache
        {

            public List<double[]> Points;

            public List<double> Angles;

        }

    }

}