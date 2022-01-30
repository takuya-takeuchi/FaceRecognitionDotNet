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
using DlibDotNet.Dnn;
using FaceRecognitionDotNet;
using MathNet.Numerics.Data.Matlab;
using Microsoft.Extensions.CommandLineUtils;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Point = System.Drawing.Point;

namespace EmotionTraining
{

    internal class Program
    {

        #region Methods

        private static int Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(EmotionTraining);
            app.Description = "The program for training Corrective re-annotation of FER - CK+ - KDEF dataset";
            app.HelpOption("-h|--help");

            app.Command("train", command =>
            {
                const uint epochDefault = 300;
                const double learningRateDefault = 0.001d;
                const double minLearningRateDefault = 0.00001d;
                const uint minBatchSizeDefault = 256;
                const uint validationDefault = 30;

                var datasetOption = command.Option("-d|--dataset", "The directory of dataset", CommandOptionType.SingleValue);
                var epochOption = command.Option("-e|--epoch", $"The epoch. Default is {epochDefault}", CommandOptionType.SingleValue);
                var learningRateOption = command.Option("-l|--lr", $"The learning rate. Default is {learningRateDefault}", CommandOptionType.SingleValue);
                var minLearningRateOption = command.Option("-m|--min-lr", $"The minimum learning rate. Default is {minLearningRateDefault}", CommandOptionType.SingleValue);
                var minBatchSizeOption = command.Option("-b|--min-batchsize", $"The minimum batch size. Default is {minBatchSizeDefault}", CommandOptionType.SingleValue);
                var validationOption = command.Option("-v|--validation-interval", $"The interval of validation. Default is {validationDefault}", CommandOptionType.SingleValue);
                var useMeanOption = command.Option("-u|--use-mean", "Use mean image", CommandOptionType.NoValue);
                var outputOption = command.Option("-o|--output", $"The output directory path.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var dataset = datasetOption.Value();
                    if (!datasetOption.HasValue() || !Directory.Exists(dataset))
                    {
                        Console.WriteLine("dataset does not exist");
                        return -1;
                    }

                    var epoch = epochDefault;
                    if (epochOption.HasValue() && !uint.TryParse(epochOption.Value(), out epoch))
                    {
                        Console.WriteLine("epoch is invalid value");
                        return -1;
                    }

                    var learningRate = learningRateDefault;
                    if (learningRateOption.HasValue() && !double.TryParse(learningRateOption.Value(), NumberStyles.Float, Thread.CurrentThread.CurrentCulture.NumberFormat, out learningRate))
                    {
                        Console.WriteLine("learning rate is invalid value");
                        return -1;
                    }

                    var minLearningRate = minLearningRateDefault;
                    if (minLearningRateOption.HasValue() && !double.TryParse(minLearningRateOption.Value(), NumberStyles.Float, Thread.CurrentThread.CurrentCulture.NumberFormat, out minLearningRate))
                    {
                        Console.WriteLine("minimum learning rate is invalid value");
                        return -1;
                    }

                    var minBatchSize = minBatchSizeDefault;
                    if (minBatchSizeOption.HasValue() && !uint.TryParse(minBatchSizeOption.Value(), out minBatchSize))
                    {
                        Console.WriteLine("minimum batch size is invalid value");
                        return -1;
                    }

                    var validation = validationDefault;
                    if (validationOption.HasValue() && !uint.TryParse(validationOption.Value(), out validation) || validation == 0)
                    {
                        Console.WriteLine("validation interval is invalid value");
                        return -1;
                    }

                    var output = "result";
                    if (outputOption.HasValue())
                    {
                        output = outputOption.Value();
                    }

                    Directory.CreateDirectory(output);

                    var useMean = useMeanOption.HasValue();

                    Console.WriteLine($"            Dataset: {dataset}");
                    Console.WriteLine($"              Epoch: {epoch}");
                    Console.WriteLine($"      Learning Rate: {learningRate}");
                    Console.WriteLine($"  Min Learning Rate: {minLearningRate}");
                    Console.WriteLine($"     Min Batch Size: {minBatchSize}");
                    Console.WriteLine($"Validation Interval: {validation}");
                    Console.WriteLine($"           Use Mean: {useMean}");
                    Console.WriteLine($"             Output: {output}");
                    Console.WriteLine();

                    var baseName = Path.Combine(output, $"Corrective_re-annotation_of_FER_CK+_KDEF-mlp_{epoch}_{learningRate}_{minLearningRate}_{minBatchSize}");
                    var parameter = new Parameter
                    {
                        BaseName = baseName,
                        Dataset = dataset,
                        Output = output,
                        Epoch = epoch,
                        LearningRate = learningRate,
                        MinLearningRate = minLearningRate,
                        MiniBatchSize = minBatchSize,
                        Validation = validation
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
                var modelOption = command.Option("-m|--model", "The model file path to estimate human emotion.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var image = imageOption.Value();
                    if (!imageOption.HasValue() || !File.Exists(image))
                    {
                        Console.WriteLine("image does not exist");
                        return -1;
                    }

                    var model = modelOption.Value();
                    if (!modelOption.HasValue() || !File.Exists(model))
                    {
                        Console.WriteLine("model file does not exist");
                        return -1;
                    }
                    
                    Console.WriteLine($"Image File: {image}");
                    Console.WriteLine($"     Model: {model}");
                    Console.WriteLine();

                    using (var faceRecognition = FaceRecognition.Create("Models"))
                    using (var faceImage = FaceRecognition.LoadImageFile(image, Mode.Greyscale))
                    {
                        var location = new Location(0, 0, faceImage.Width, faceImage.Height);
                        var landmark = faceRecognition.FaceLandmark(faceImage, new[] { location }).FirstOrDefault();
                        if (landmark == null)
                        {
                            Console.WriteLine($"Failed to get face landmark and estimate face emotion");
                            return 0;
                        }

                        var vector = GetFeatureVector(landmark);
                        var trainNet = NativeMethods.LossMulticlassLog_emotion_train_type_create();
                        var networkId = LossMulticlassLogRegistry.GetId(trainNet);
                        LossMulticlassLogRegistry.Add(trainNet);

                        using (var net = LossMulticlassLog.Deserialize(model, networkId))
                        using (var matrix = new Matrix<double>(vector.ToArray(), vector.Length, 1))
                        using (var predictedLabels = net.Operator(matrix))
                        {
                            Console.WriteLine($"{(Emotion)predictedLabels[0]}");
                        }
                    }

                    return 0;
                });
            });

            return app.Execute(args);
        }

        #region Helpers

        private static double[] GetFeatureVector(IDictionary<FacePart, IEnumerable<FacePoint>> landmark)
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

            return new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12 };
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

                                    var vector = GetFeatureVector(landmark);
                                    
                                    vectorList.Add(vector);
                                    imageList.Add(new Matrix<double>(vector.ToArray(), vector.Length, 1));
                                    labelList.Add(emotion);
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

                // So with that out of the way, we can make a network instance.
                var trainNet = NativeMethods.LossMulticlassLog_emotion_train_type_create();
                var networkId = LossMulticlassLogRegistry.GetId(trainNet);
                LossMulticlassLogRegistry.Add(trainNet);

                using (var net = LossMulticlassLog.Deserialize(parameter.Model, networkId))
                {
                    var validationParameter = new ValidationParameter
                    {
                        BaseName = Path.GetFileNameWithoutExtension(parameter.Model),
                        Trainer = net,
                        TrainingImages = trainingImages,
                        TrainingLabels = trainingLabels,
                        TestingImages = testingImages,
                        TestingLabels = testingLabels,
                        UseConsole = true,
                        SaveToXml = false,
                        OutputDiffLog = true,
                        Output = Path.GetDirectoryName(parameter.Model)
                    };

                    Validation(validationParameter, out _, out _);
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

                // So with that out of the way, we can make a network instance.
                var trainNet = NativeMethods.LossMulticlassLog_emotion_train_type_create();
                var networkId = LossMulticlassLogRegistry.GetId(trainNet);
                LossMulticlassLogRegistry.Add(trainNet);

                using (var net = new LossMulticlassLog(networkId))
                using (var solver = new Adam())
                using (var trainer = new DnnTrainer<LossMulticlassLog>(net, solver))
                {
                    var learningRate = parameter.LearningRate;
                    var minLearningRate = parameter.MinLearningRate;
                    var miniBatchSize = parameter.MiniBatchSize;
                    var baseName = parameter.BaseName;
                    var epoch = parameter.Epoch;
                    var validation = parameter.Validation;

                    trainer.SetLearningRate(learningRate);
                    trainer.SetMinLearningRate(minLearningRate);
                    trainer.SetMiniBatchSize(miniBatchSize);
                    trainer.BeVerbose();
                    trainer.SetSynchronizationFile(baseName, 180);

                    // create array box
                    var trainingImagesCount = trainingImages.Count;
                    var trainingLabelsCount = trainingLabels.Count;

                    var maxIteration = (int)Math.Ceiling(trainingImagesCount / (float)miniBatchSize);
                    var imageBatches = new Matrix<double>[maxIteration][];
                    var labelBatches = new uint[maxIteration][];
                    for (var i = 0; i < maxIteration; i++)
                    {
                        if (miniBatchSize <= trainingImagesCount - i * miniBatchSize)
                        {
                            imageBatches[i] = new Matrix<double>[miniBatchSize];
                            labelBatches[i] = new uint[miniBatchSize];
                        }
                        else
                        {
                            imageBatches[i] = new Matrix<double>[trainingImagesCount % miniBatchSize];
                            labelBatches[i] = new uint[trainingLabelsCount % miniBatchSize];
                        }
                    }

                    using (var fs = new FileStream($"{baseName}.log", FileMode.Create, FileAccess.Write, FileShare.Write))
                    using (var sw = new StreamWriter(fs, Encoding.UTF8))
                        for (var e = 0; e < epoch; e++)
                        {
                            var randomArray = Enumerable.Range(0, trainingImagesCount).OrderBy(i => Guid.NewGuid()).ToArray();
                            var index = 0;
                            for (var i = 0; i < imageBatches.Length; i++)
                            {
                                var currentImages = imageBatches[i];
                                var currentLabels = labelBatches[i];
                                for (var j = 0; j < imageBatches[i].Length; j++)
                                {
                                    var rIndex = randomArray[index];
                                    currentImages[j] = trainingImages[rIndex];
                                    currentLabels[j] = (uint)trainingLabels[rIndex];
                                    index++;
                                }
                            }

                            for (var i = 0; i < maxIteration; i++)
                                LossMulticlassLog.TrainOneStep(trainer, imageBatches[i], labelBatches[i]);

                            var lr = trainer.GetLearningRate();
                            var loss = trainer.GetAverageLoss();

                            var trainLog = $"Epoch: {e}, learning Rate: {lr}, average loss: {loss}";
                            Console.WriteLine(trainLog);
                            sw.WriteLine(trainLog);

                            if (e > 0 && e % validation == 0)
                            {
                                var validationParameter = new ValidationParameter
                                {
                                    BaseName = parameter.BaseName,
                                    Output = parameter.Output,
                                    Trainer = net,
                                    TrainingImages = trainingImages,
                                    TrainingLabels = trainingLabels,
                                    TestingImages = testingImages,
                                    TestingLabels = testingLabels,
                                    UseConsole = true,
                                    SaveToXml = true,
                                    OutputDiffLog = true
                                };

                                Validation(validationParameter, out var trainAccuracy, out var testAccuracy);

                                var validationLog = $"Epoch: {e}, train accuracy: {trainAccuracy}, test accuracy: {testAccuracy}";
                                Console.WriteLine(validationLog);
                                sw.WriteLine(validationLog);
                            }

                            if (lr < minLearningRate)
                                break;
                        }

                    // wait for training threads to stop
                    trainer.GetNet();
                    Console.WriteLine("done training");

                    net.Clean();
                    LossMulticlassLog.Serialize(net, $"{baseName}.dat");

                    // Now let's run the training images through the network.  This statement runs all the
                    // images through it and asks the loss layer to convert the network's raw output into
                    // labels.  In our case, these labels are the numbers between 0 and 9.
                    var validationParameter2 = new ValidationParameter
                    {
                        BaseName = parameter.BaseName,
                        Output = parameter.Output,
                        Trainer = net,
                        TrainingImages = trainingImages,
                        TrainingLabels = trainingLabels,
                        TestingImages = testingImages,
                        TestingLabels = testingLabels,
                        UseConsole = true,
                        SaveToXml = true,
                        OutputDiffLog = true
                    };

                    Validation(validationParameter2, out _, out _);
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

            var net = parameter.Trainer;
            var trainingImages = parameter.TrainingImages;
            var trainingLabels = parameter.TrainingLabels;
            var testingImages = parameter.TestingImages;
            var testingLabels = parameter.TestingLabels;
            var saveToXml = parameter.SaveToXml;
            var baseName = parameter.BaseName;
            var useConsole = parameter.UseConsole;

            using (var predictedLabels = net.Operator(trainingImages))
            {
                var numRight = 0;
                var numWrong = 0;

                // And then let's see if it classified them correctly.
                for (var i = 0; i < trainingImages.Count; ++i)
                {
                    if ((Emotion)predictedLabels[i] == trainingLabels[i])
                        ++numRight;
                    else
                        ++numWrong;
                }

                if (useConsole)
                {
                    Console.WriteLine($"training num_right: {numRight}");
                    Console.WriteLine($"training num_wrong: {numWrong}");
                    Console.WriteLine($"training accuracy:  {numRight / (double)(numRight + numWrong)}");
                }

                trainAccuracy = numRight / (double)(numRight + numWrong);

                using (var predictedLabels2 = net.Operator(testingImages))
                {
                    numRight = 0;
                    numWrong = 0;
                    for (var i = 0; i < testingImages.Count; ++i)
                    {
                        if ((Emotion)predictedLabels2[i] == testingLabels[i])
                            ++numRight;
                        else
                            ++numWrong;
                    }

                    if (useConsole)
                    {
                        Console.WriteLine($"testing num_right: {numRight}");
                        Console.WriteLine($"testing num_wrong: {numWrong}");
                        Console.WriteLine($"testing accuracy:  {numRight / (double)(numRight + numWrong)}");
                    }

                    testAccuracy = numRight / (double)(numRight + numWrong);

                    // Finally, you can also save network parameters to XML files if you want to do
                    // something with the network in another tool.  For example, you could use dlib's
                    // tools/convert_dlib_nets_to_caffe to convert the network to a caffe model.
                    if (saveToXml)
                        Dlib.NetToXml(net, $"{baseName}.xml");
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

            public uint Epoch
            {
                get;
                set;
            }

            public double LearningRate
            {
                get;
                set;
            }

            public double MinLearningRate
            {
                get;
                set;
            }

            public uint MiniBatchSize
            {
                get;
                set;
            }

            public uint Validation
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

            public LossMulticlassLog Trainer
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