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
using FaceRecognitionDotNet;
using MathNet.Numerics.Data.Matlab;
using Microsoft.Extensions.CommandLineUtils;
using ShellProgressBar;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Point = System.Drawing.Point;

namespace EmotionTraining
{

    internal class Program
    {

        #region Fields

        

        #endregion

        #region Methods

        private static int Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(EmotionTraining);
            app.Description = "The program for training Corrective re-annotation of FER - CK+ - KDEF dataset";
            app.HelpOption("-h|--help");

            app.Command("train", command =>
            {
                const double alphaDefault = 0.1d;
                const double momentumDefault = 0.8d;

                var datasetOption = command.Option("-d|--dataset", "The directory of dataset", CommandOptionType.SingleValue);
                var alphaOption = command.Option("-a|--alpha", $"The alpha. Default is {alphaDefault}", CommandOptionType.SingleValue);
                var momentumOption = command.Option("-m|--momentum", $"The alpha. Default is {alphaDefault}", CommandOptionType.SingleValue);
                var outputOption = command.Option("-o|--output", $"The output directory path.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var dataset = datasetOption.Value();
                    if (!datasetOption.HasValue() || !Directory.Exists(dataset))
                    {
                        Console.WriteLine("dataset does not exist");
                        return -1;
                    }

                    var alpha = alphaDefault;
                    if (alphaOption.HasValue() && !double.TryParse(alphaOption.Value(), NumberStyles.Float, Thread.CurrentThread.CurrentCulture.NumberFormat, out alpha))
                    {
                        Console.WriteLine("alpha is invalid value");
                        return -1;
                    }

                    var momentum = momentumDefault;
                    if (momentumOption.HasValue() && !double.TryParse(momentumOption.Value(), NumberStyles.Float, Thread.CurrentThread.CurrentCulture.NumberFormat, out alpha))
                    {
                        Console.WriteLine("momentum is invalid value");
                        return -1;
                    }

                    var output = "result";
                    if (outputOption.HasValue())
                    {
                        output = outputOption.Value();
                    }

                    Directory.CreateDirectory(output);

                    Console.WriteLine($"  Dataset: {dataset}");;
                    Console.WriteLine($"    alpha: {alpha}");
                    Console.WriteLine($" momentum: {momentum}");
                    Console.WriteLine($"   Output: {output}");
                    Console.WriteLine();
                    
                    var baseName = $"Corrective_re-annotation_of_FER_CK+_KDEF-mlp_{alpha}_{momentum}";
                    var parameter = new Parameter
                    {
                        BaseName = baseName,
                        Dataset = dataset,
                        Output = output,
                        Alpha = alpha,
                        Momentum = momentum
                    };

                    Train(parameter);

                    return 0;
                });
            });

            app.Command("test", command =>
            {
                var datasetOption = command.Option("-d|--dataset", "The directory of dataset", CommandOptionType.SingleValue);
                var modelOption = command.Option("-m|--model", "The model file.", CommandOptionType.SingleValue);

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

                    Console.WriteLine($"Dataset: {dataset}");
                    Console.WriteLine($"  Model: {model}");
                    Console.WriteLine();

                    var parameter = new Parameter
                    {
                        Dataset = dataset,
                        Model = model
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

                    //using (var source = Image.FromFile(image))
                    //using (var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb))
                    //using (var g = Graphics.FromImage(bitmap))
                    //{
                    //    double rollValue;
                    //    double pitchValue;
                    //    double yawValue;
                    //    using (var matrix = GetPointMatrix(points, Emotion.Roll))
                    //    using (var rbk = new RadialBasisKernel<double, Matrix<double>>(0.1, 0, 0))
                    //    {
                    //        Krls<double, RadialBasisKernel<double, Matrix<double>>> trainer = null;

                    //        try
                    //        {
                    //            trainer = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(rbk);
                    //            Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(roll, ref trainer);
                    //            rollValue = trainer.Operator(matrix);
                    //        }
                    //        finally
                    //        {
                    //            trainer?.Dispose();
                    //        }
                    //    }

                    //    using (var matrix = GetPointMatrix(points, Emotion.Pitch))
                    //    using (var rbk = new RadialBasisKernel<double, Matrix<double>>(0.1, 0, 0))
                    //    {
                    //        Krls<double, RadialBasisKernel<double, Matrix<double>>> trainer = null;

                    //        try
                    //        {
                    //            trainer = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(rbk);
                    //            Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(pitch, ref trainer);
                    //            pitchValue = trainer.Operator(matrix);
                    //        }
                    //        finally
                    //        {
                    //            trainer?.Dispose();
                    //        }
                    //    }

                    //    using (var matrix = GetPointMatrix(points, Emotion.Yaw))
                    //    using (var rbk = new RadialBasisKernel<double, Matrix<double>>(0.1, 0, 0))
                    //    {
                    //        Krls<double, RadialBasisKernel<double, Matrix<double>>> trainer = null;

                    //        try
                    //        {
                    //            trainer = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(rbk);
                    //            Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(yaw, ref trainer);
                    //            yawValue = trainer.Operator(matrix);
                    //        }
                    //        finally
                    //        {
                    //            trainer?.Dispose();
                    //        }
                    //    }

                    //    g.DrawImage(source, Point.Empty);

                    //    Console.WriteLine($" Predicted Roll: {rollValue}");
                    //    Console.WriteLine($"Predicted Pitch: {pitchValue}");
                    //    Console.WriteLine($"  Predicted Yaw: {yawValue}");

                    //    const int pointSize = 2;
                    //    foreach (var p in points)
                    //    {
                    //        g.DrawEllipse(Pens.GreenYellow, (float)p.X - pointSize, (float)p.Y - pointSize, pointSize * 2, pointSize * 2);
                    //    }

                    //    DrawAxis(g, bitmap.Width, bitmap.Height, rollValue, pitchValue, yawValue, 150);

                    //    var filename = Path.GetFileName(image);
                    //    bitmap.Save(Path.Combine(output, $"{filename}-predicted.jpg"), ImageFormat.Jpeg);
                    //}

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

        private static Matrix<double> GetPointMatrix(IList<DPoint> points)
        {
            //// https://cdn-images-1.medium.com/max/1600/1*AbEg31EgkbXSQehuNJBlWg.png
            //switch (emotion)
            //{

            //    case Emotion.Roll:
            //        {
            //            // Calc angle from 33 to each point except for 33
            //            var vector = new List<double>();
            //            var p1 = points[33];
            //            for (var c = 0; c < 68; c++)
            //            {
            //                if (c == 33)
            //                    continue;

            //                var p2 = points[c];
            //                var distance = Math.Atan((p2.X - p1.X) / (p2.Y - p1.Y));
            //                //var distance = Math.Sqrt(Math.Pow(p2.X - p1.X,2) + Math.Pow(p2.Y - p1.Y,2));
            //                vector.Add(distance);
            //            }

            //            // Need not to use Normalization
            //            //NormalizeVector(vector);

            //            return new Matrix<double>(vector.ToArray(), vector.Count, 1);
            //        }
            //    case Emotion.Pitch:
            //        {
            //            // Calc angle from 33 to each point except for 33
            //            var vector = new List<double>();
            //            var p1 = points[33];
            //            for (var c = 0; c < 68; c++)
            //            {
            //                if (c == 33)
            //                    continue;

            //                var p2 = points[c];
            //                var distance = p2.Y - p1.Y;
            //                vector.Add(distance);
            //            }

            //            NormalizeVector(vector);

            //            return new Matrix<double>(vector.ToArray(), vector.Count, 1);
            //        }
            //    case Emotion.Yaw:
            //        {
            //            // Calc angle from 33 to each point except for 33
            //            var vector = new List<double>();
            //            var p1 = points[33];
            //            for (var c = 0; c < 68; c++)
            //            {
            //                if (c == 33)
            //                    continue;

            //                var p2 = points[c];
            //                var distance = p2.X - p1.X;
            //                vector.Add(distance);
            //            }

            //            NormalizeVector(vector);

            //            return new Matrix<double>(vector.ToArray(), vector.Count, 1);
            //        }
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(emotion), emotion, null);
            //}
            return null;
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

        private static double GetEuclideanDistance(FaceRecognitionDotNet.Point p1, FaceRecognitionDotNet.Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private static FaceRecognitionDotNet.Point CenterOf(FaceRecognitionDotNet.Point p1, FaceRecognitionDotNet.Point p2)
        {
            var x = (p1.X + p2.X) / 2;
            var y = (p1.Y + p2.Y) / 2;
            return new FaceRecognitionDotNet.Point(x, y);
        }

        private static void Load(string directory, string type, out IList<Matrix<double>> images, out IList<Emotion> labels)
        {
            var imageList = new List<Matrix<double>>();
            var vectorList = new List<double[]>();
            var labelList = new List<Emotion>();
            
            var path = $"{Path.Combine(directory, type)}_cache.dat";
            if (File.Exists(path))
            {
                Console.WriteLine($"Use Cache {path}");

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var s = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    var cache = (Cache)s.Deserialize(fs);
                    
                    imageList.AddRange(cache.Points.Select(vector => new Matrix<double>(vector.ToArray(), vector.Length, 1)));
                    labelList.AddRange(cache.Emotion);
                }

                images = imageList;
                labels = labelList;

                return;
            }

            using (var faceRecognition = FaceRecognition.Create("Models"))
            {
                using (var fs = new FileStream($"{Path.Combine(directory, type)}.txt", FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    do
                    {
                        var line = sr.ReadLine();
                        if (line == null)
                            break;

                        var imagePath = Path.Combine(directory, line);
                        var dir = Path.GetFileName(Path.GetDirectoryName(imagePath));
                        switch (dir)
                        {
                            case "anger":
                            case "contempt":
                            case "disgust":
                            case "fear":
                            case "happiness":
                            case "neutrality":
                            case "sadness":
                            case "surprise":
                                if (!Enum.TryParse<Emotion>(dir, true, out var emotion))
                                {
                                    continue;
                                }

                                using (var faceImage = FaceRecognition.LoadImageFile(imagePath, Mode.Greyscale))
                                {
                                    var location = new Location(0, 0, faceImage.Width, faceImage.Height);
                                    var landmark = faceRecognition.FaceLandmark(faceImage, new[] { location }).FirstOrDefault();
                                    if (landmark == null)
                                    {
                                        continue;
                                    }

                                    // Subsample of Input feature vectors and description
                                    {
                                        var leftEye = landmark[FacePart.LeftEye].ToArray();
                                        var rightEye = landmark[FacePart.RightEye].ToArray();
                                        var leftEyebrow = landmark[FacePart.LeftEyebrow].ToArray();
                                        var rightEyebrow = landmark[FacePart.RightEyebrow].ToArray();
                                        var bottomLip = landmark[FacePart.BottomLip];
                                        var topLip = landmark[FacePart.TopLip];
                                        var lip = bottomLip.Concat(topLip).ToArray();
                                        var nose = landmark[FacePart.NoseBridge].ToArray();

                                        // For a definition of each point index, see https://cdn-images-1.medium.com/max/1600/1*AbEg31EgkbXSQehuNJBlWg.png
                                        // convert 68 points to 18 points
                                        // left eye
                                        var leftEyeTop1 = leftEye.FirstOrDefault(point => point.Index == 37).Point;
                                        var leftEyeTop2 = leftEye.FirstOrDefault(point => point.Index == 38).Point;
                                        var leftEyeBottom1 = leftEye.FirstOrDefault(point => point.Index == 41).Point;
                                        var leftEyeBottom2 = leftEye.FirstOrDefault(point => point.Index == 40).Point;
                                        var f1 = CenterOf(leftEyeTop1, leftEyeTop2);
                                        var f2 = CenterOf(leftEyeBottom1, leftEyeBottom2);
                                        var f3 = leftEye.FirstOrDefault(point => point.Index == 36).Point;
                                        var f4 = leftEye.FirstOrDefault(point => point.Index == 39).Point;

                                        // left eye
                                        var rightEyeTop1 = rightEye.FirstOrDefault(point => point.Index == 43).Point;
                                        var rightEyeTop2 = rightEye.FirstOrDefault(point => point.Index == 44).Point;
                                        var rightEyeBottom1 = rightEye.FirstOrDefault(point => point.Index == 46).Point;
                                        var rightEyeBottom2 = rightEye.FirstOrDefault(point => point.Index == 47).Point;
                                        var f5 = CenterOf(rightEyeTop1, rightEyeTop2);
                                        var f6 = CenterOf(rightEyeBottom1, rightEyeBottom2);
                                        var f7 = rightEye.FirstOrDefault(point => point.Index == 42).Point;
                                        var f8 = rightEye.FirstOrDefault(point => point.Index == 45).Point;

                                        // nose
                                        var f9 = nose.FirstOrDefault(point => point.Index == 30).Point;

                                        // left eyebrow
                                        var f10 = leftEyebrow.FirstOrDefault(point => point.Index == 17).Point;
                                        var f11 = leftEyebrow.FirstOrDefault(point => point.Index == 21).Point;
                                        var f12 = leftEyebrow.FirstOrDefault(point => point.Index == 19).Point;

                                        // right eyebrow
                                        var f13 = rightEyebrow.FirstOrDefault(point => point.Index == 22).Point;
                                        var f14 = rightEyebrow.FirstOrDefault(point => point.Index == 26).Point;
                                        var f15 = rightEyebrow.FirstOrDefault(point => point.Index == 24).Point;

                                        // lip
                                        var f16 = lip.FirstOrDefault(point => point.Index == 48).Point;
                                        var f17 = lip.FirstOrDefault(point => point.Index == 54).Point;
                                        var top = lip.FirstOrDefault(point => point.Index == 62).Point;
                                        var bottom = lip.FirstOrDefault(point => point.Index == 66).Point;
                                        var f18 = CenterOf(top, bottom);

                                        //using (var canvas = new Bitmap(faceImage.Width * 2, faceImage.Height))
                                        //using (var image = new Bitmap(imagePath))
                                        //using (var g = Graphics.FromImage(canvas))
                                        //{
                                        //    g.DrawImage(image, new System.Drawing.Rectangle(0, 0, faceImage.Width, faceImage.Height));
                                        //    g.DrawImage(image, new System.Drawing.Rectangle(faceImage.Width, 0, faceImage.Width, faceImage.Height));

                                        //    const int pointSize = 2;

                                        //    var eyeAndNose = new[]
                                        //    {
                                        //        f1, f2, f3, f4, f5, f6, f7, f8, f9
                                        //    };

                                        //    var eyebrowAndLip = new[]
                                        //    {
                                        //        f10, f11, f12, f13, f14, f15, f16, f17, f18
                                        //    };

                                        //    foreach (var p in eyeAndNose)
                                        //        g.DrawEllipse(Pens.Red, (float)p.X - pointSize, (float)p.Y - pointSize, pointSize * 2, pointSize * 2);
                                        //    foreach (var p in eyebrowAndLip)
                                        //        g.DrawEllipse(Pens.CornflowerBlue, (float)p.X - pointSize, (float)p.Y - pointSize, pointSize * 2, pointSize * 2);

                                        //    foreach (var points in landmark.Values)
                                        //        foreach (var p in points)
                                        //            g.DrawEllipse(Pens.GreenYellow, (float)p.Point.X - pointSize + faceImage.Width, (float)p.Point.Y - pointSize, pointSize * 2, pointSize * 2);

                                        //    canvas.Save("tmp.png");
                                        //}

                                        // Left eye Height V1 = F1-F2
                                        var v1 = GetEuclideanDistance(f1, f2);

                                        // Left eye Width V2 = F4 - F3
                                        var v2 = GetEuclideanDistance(f4, f3);

                                        // Right eye Height V3 = F5 - F6
                                        var v3 = GetEuclideanDistance(f5, f6);

                                        // Right eye Width V4 = F8- F7
                                        var v4 = GetEuclideanDistance(f8, f7);

                                        // Left eyebrow width V5 = F11 - F10
                                        var v5 = GetEuclideanDistance(f11, f10);

                                        // Right eyebrow width V6 = F14 - F13
                                        var v6 = GetEuclideanDistance(f14, f13);

                                        // Lip width V7 = F17 - F16
                                        var v7 = GetEuclideanDistance(f17, f16);

                                        // Left eye upper corner and left eyebrow center dist. V8 = F12 - F1
                                        var v8 = GetEuclideanDistance(f12, f11);

                                        // Right eye upper corner and right eyebrow center dist. V9 = F15 - F5
                                        var v9 = GetEuclideanDistance(f15, f5);

                                        // Nose centre and lips centre dist. V10 = F9 - F18
                                        var v10 = GetEuclideanDistance(f9, f18);

                                        // Left eye lower corner and lips left corner dist. V11 = F2 - F16
                                        var v11 = GetEuclideanDistance(f2, f16);

                                        // Right eye lower corner and lips right corner dist. V12 = F6 - F17
                                        var v12 = GetEuclideanDistance(f6, f17);

                                        var vector = new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12 };
                                        vectorList.Add(vector);
                                        imageList.Add(new Matrix<double>(vector.ToArray(), vector.Length, 1));
                                        labelList.Add(emotion);
                                    }
                                }

                                break;
                        }
                    } while (true);
                }
            }

            images = imageList;
            labels = labelList;

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Console.WriteLine($"Save Cache {path}");

                var cache = new Cache();
                cache.Points = vectorList;
                cache.Emotion = labelList;
                var s = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                s.Serialize(fs, cache);
            }
        }

        private static void Test(Parameter parameter)
        {
            try
            {
                IList<Matrix<double>> trainingImages;
                IList<Emotion> trainingLabels;
                IList<Matrix<double>> testingImages;
                IList<Emotion> testingLabels;

                Console.WriteLine("Start load train images");
                Load(parameter.Dataset, "train", out trainingImages, out trainingLabels);
                Console.WriteLine($"Load train images: {trainingImages.Count}");

                Console.WriteLine("Start load test images");
                Load(parameter.Dataset, "test", out testingImages, out testingLabels);
                Console.WriteLine($"Load test images: {testingImages.Count}");
                Console.WriteLine();

                using (var mlp = new MultilayerPerceptron<Kernel1>(12, 100, 500, 7, parameter.Alpha, parameter.Momentum))
                {
                    var validationParameter = new ValidationParameter
                    {
                        BaseName = Path.GetFileNameWithoutExtension(parameter.Model),
                        Trainer = mlp,
                        TrainingImages = trainingImages,
                        TrainingLabels = trainingLabels,
                        TestingImages = testingImages,
                        TestingLabels = testingLabels,
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
                IList<Emotion> trainingLabels;
                IList<Matrix<double>> testingImages;
                IList<Emotion> testingLabels;

                Console.WriteLine("Start load train images");
                Load(parameter.Dataset, "train", out trainingImages, out trainingLabels);
                Console.WriteLine($"Load train images: {trainingImages.Count}");

                Console.WriteLine("Start load test images");
                Load(parameter.Dataset, "test", out testingImages, out testingLabels);
                Console.WriteLine($"Load test images: {testingImages.Count}");
                Console.WriteLine();

                Console.WriteLine("Training");
                using (var mlp = new MultilayerPerceptron<Kernel1>(12, 100, 500, 7, parameter.Alpha, parameter.Momentum))
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
                            mlp.Train(train, (double)label);
                            pbar.Tick($"Step {index + 1} of {count}");
                        }
                    }

                    var validationParameter = new ValidationParameter
                    {
                        BaseName = parameter.BaseName,
                        Output = parameter.Output,
                        Trainer = mlp,
                        TrainingImages = trainingImages,
                        TrainingLabels = trainingLabels,
                        TestingImages = testingImages,
                        TestingLabels = testingLabels,
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

        private static Emotion GetEmotion(Matrix<double> propability)
        {
            var exp = propability.ToArray().Select(Math.Exp);
            var sum = exp.Sum();
            var softmax = exp.Select(i => i / sum);
            return (Emotion)softmax.Select((value, index) => new { Value = value, Index = index })
                                   .Aggregate((max, working) => (max.Value > working.Value) ? max : working).Index;
        }

        private static void Validation(ValidationParameter parameter,
                                       out double trainAccuracy,
                                       out double testAccuracy)
        {
            trainAccuracy = 0;
            testAccuracy = 0;

            var train_diff = new List<Tuple<Emotion, Emotion>>();
            var test_diff = new List<Tuple<Emotion, Emotion>>();

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

                    using (var p = parameter.Trainer.Operator(x))
                    {
                        var result = GetEmotion(p);
                        if (result == y)
                            ++numRight;
                        else
                            ++numWrong;

                        train_diff.Add(new Tuple<Emotion, Emotion>(y, result));
                        pbar.Tick($"Step {index + 1} of {count}");
                    }
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

                    using (var p = parameter.Trainer.Operator(x))
                    {
                        var result = GetEmotion(p);
                        if (result == y)
                            ++numRight;
                        else
                            ++numWrong;
                        
                        test_diff.Add(new Tuple<Emotion, Emotion>(y, result));
                        pbar.Tick($"Step {index + 1} of {count}");
                    }
                }
            }

            testAccuracy = numRight / (double)(numRight + numWrong);

            if (parameter.UseConsole)
            {
                Console.WriteLine($"testing num_right: {numRight}");
                Console.WriteLine($"testing num_wrong: {numWrong}");
                Console.WriteLine($" testing accuracy:  {testAccuracy:F4}");
            }

            //if (parameter.SaveToXml)
            //    MultilayerPerceptron<Kernel1>.Serialize(parameter.Trainer, Path.Combine(parameter.Output, $"{parameter.BaseName}.dat"));

            if (parameter.OutputDiffLog)
            {
                using (var train_fs = new FileStream(Path.Combine(parameter.Output, $"{parameter.BaseName}_train_diff.log"), FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var test_fs = new FileStream(Path.Combine(parameter.Output, $"{parameter.BaseName}_test_diff.log"), FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var train_sw = new StreamWriter(train_fs, Encoding.UTF8))
                using (var test_sw = new StreamWriter(test_fs, Encoding.UTF8))
                {
                    foreach (var v in train_diff)
                        train_sw.WriteLine($"{v.Item1}\t{v.Item2}");
                    foreach (var v in test_diff)
                        test_sw.WriteLine($"{v.Item1}\t{v.Item2}");
                }
            }
        }

        #endregion

        #endregion

        private enum Emotion
        {

            Anger,

            Contempt,

            Disgust,

            Fear,

            Happiness,

            Neutrality,

            Sadness,

            Surprise

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

            public double Alpha
            {
                get;
                set;
            }

            public double Momentum
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

            public MultilayerPerceptron<Kernel1> Trainer
            {
                get;
                set;
            }

            public IList<Matrix<double>> TrainingImages
            {
                get;
                set;
            }


            public IList<Emotion> TrainingLabels
            {
                get;
                set;
            }


            public IList<Matrix<double>> TestingImages
            {
                get;
                set;
            }


            public IList<Emotion> TestingLabels
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

            public List<Emotion> Emotion;

        }

    }

}