using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DlibDotNet;
using DlibDotNet.Dnn;
using Microsoft.Extensions.CommandLineUtils;

namespace AgeTraining
{

    internal class Program
    {

        #region Fields

        private const int Size = 227;

        #endregion

        #region Methods

        private static int Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(AgeTraining);
            app.Description = "The program for training Adience OUI Unfiltered faces for gender and age classification dataset";
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

                    var useMean = useMeanOption.HasValue();

                    Console.WriteLine($"            Dataset: {dataset}");
                    Console.WriteLine($"              Epoch: {epoch}");
                    Console.WriteLine($"      Learning Rate: {learningRate}");
                    Console.WriteLine($"  Min Learning Rate: {minLearningRate}");
                    Console.WriteLine($"     Min Batch Size: {minBatchSize}");
                    Console.WriteLine($"Validation Interval: {validation}");
                    Console.WriteLine($"           Use Mean: {useMean}");
                    Console.WriteLine();

                    var baseName = $"adience-age-network_{epoch}_{learningRate}_{minLearningRate}_{minBatchSize}_{useMean}";
                    Train(baseName, dataset, epoch, learningRate, minLearningRate, minBatchSize, validation, useMean);

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
                    if (!datasetOption.HasValue() || !File.Exists(model))
                    {
                        Console.WriteLine("model does not exist");
                        return -1;
                    }

                    Console.WriteLine($"Dataset: {dataset}");
                    Console.WriteLine($"  Model: {model}");
                    Console.WriteLine();

                    Test(dataset, model);

                    return 0;
                });
            });

            app.Command("preprocess", command =>
            {
                var datasetOption = command.Option("-d|--dataset", "The directory of dataset", CommandOptionType.SingleValue);
                var outputOption = command.Option("-o|--output", "The path to output preprocessed dataset", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var dataset = datasetOption.Value();
                    if (!datasetOption.HasValue() || !Directory.Exists(dataset))
                    {
                        Console.WriteLine("dataset does not exist");
                        return -1;
                    }

                    var output = outputOption.Value();
                    if (!outputOption.HasValue())
                    {
                        Console.WriteLine("output does not specify");
                        return -1;
                    }

                    Directory.CreateDirectory(output);

                    var types = new[]
                    {
                        "train", "test"
                    };

                    foreach (var type in types)
                    {
                        var imageDir = Path.Combine(dataset, type);
                        if (!Directory.Exists(imageDir))
                        {
                            Console.WriteLine($"{imageDir} does not exist");
                            return -1;
                        }

                        var csv = Path.Combine(dataset, $"{type}.csv");
                        if (!File.Exists(csv))
                        {
                            Console.WriteLine($"{csv} does not exist");
                            return -1;
                        }

                        File.Copy(csv, Path.Combine(output, $"{type}.csv"), true);

                        Directory.CreateDirectory(Path.Combine(output, type));
                    }

                    Console.WriteLine($"Dataset: {dataset}");
                    Console.WriteLine($" Output: {output}");
                    Console.WriteLine();

                    using (var posePredictor = ShapePredictor.Deserialize("shape_predictor_5_face_landmarks.dat"))
                    using (var faceDetector = Dlib.GetFrontalFaceDetector())
                    {
                        foreach (var type in types)
                            Preprocess(type, dataset, faceDetector, posePredictor, output);
                    }

                    return 0;
                });
            });

            return app.Execute(args);
        }

        #region Helpers

        private static IDictionary<string, uint> ReadCsv(string path)
        {
            var results = new Dictionary<string, uint>();

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sr = new StreamReader(fs))
            {
                var line = sr.ReadLine();
                while (line != null)
                {
                    // "aligned\10171175@N06\landmark_aligned_face.1191.11674850055_0107e2c11e_o.jpg","(8, 12)"
                    var match = Regex.Match(line, "^\"(?<path>[^\"]+)\",\"(?<age>[^\"]+)\"", RegexOptions.Compiled | RegexOptions.Singleline);
                    if (match.Success)
                    {
                        var ages = new[]
                        {
                            "(0, 2)",
                            "(4, 6)",
                            "(8, 23)",
                            "(15, 20)",
                            "(25, 32)",
                            "(38, 43)",
                            "(48, 53)",
                            "(60, 100)"
                        };

                        var index = Array.IndexOf(ages, match.Groups["age"].Value);
                        if (index >= 0)
                            results.Add(match.Groups["path"].Value, (uint)index);
                    }

                    line = sr.ReadLine();
                }
            }

            return results;
        }

        private static void Load(string type, string directory, string meanImage, out IList<Matrix<RgbPixel>> images, out IList<uint> labels)
        {
            Matrix<RgbPixel> mean = null;

            try
            {
                if (File.Exists(meanImage))
                    mean = Dlib.LoadImageAsMatrix<RgbPixel>(meanImage);

                var csv = ReadCsv(Path.Combine(directory, $"{type}.csv"));
                var imageList = new List<Matrix<RgbPixel>>();
                var labelList = new List<uint>();
                foreach (var kvp in csv)
                {
                    var path = Path.Combine(directory, type, kvp.Key);
                    if (!File.Exists(path))
                        continue;

                    using (var tmp = Dlib.LoadImageAsMatrix<RgbPixel>(path))
                    {
                        if (mean != null)
                        {
                            using (var m = new Matrix<RgbPixel>(Size, Size))
                            {
                                Dlib.ResizeImage(tmp, m);

                                // ToDo: Support subtract operator on DlibDotNet
                                // var ret = m - mean;
                                var ret = new Matrix<RgbPixel>(Size, Size);
                                for (var row = 0; row < Size; row++)
                                for (var col = 0; col < Size; col++)
                                {
                                    var left = m[row, col];
                                    var right = mean[row, col];
                                    var red = left.Red - right.Red;
                                    var green = left.Green - right.Green;
                                    var blue = left.Blue - right.Blue;
                                    ret[row, col] = new RgbPixel((byte)red, (byte)green, (byte)blue);
                                }
                                imageList.Add(ret);
                            }
                        }
                        else
                        {
                            var m = new Matrix<RgbPixel>(Size, Size);
                            Dlib.ResizeImage(tmp, m);
                            imageList.Add(m);
                        }

                        labelList.Add(kvp.Value);
                    }
                }

                images = imageList;
                labels = labelList;
            }
            finally
            {
                mean?.Dispose();
            }
        }

        private static void Preprocess(string type, string input, FrontalFaceDetector faceDetector, ShapePredictor posePredictor, string output)
        {
            var imageCount = 0;

            var r = new ulong[Size * Size];
            var g = new ulong[Size * Size];
            var b = new ulong[Size * Size];

            var csv = ReadCsv(Path.Combine(input, $"{type}.csv"));
            var outputDir = Path.Combine(output, type);

            foreach (var kvp in csv)
                using (var tmp = Dlib.LoadImageAsMatrix<RgbPixel>(Path.Combine(input, type, kvp.Key)))
                {
                    var dets = faceDetector.Operator(tmp);
                    if (!dets.Any())
                    {
                        Console.WriteLine($"Warning: Failed to detect face from '{kvp}'");
                        continue;
                    }

                    // Get max size rectangle. It could be better face.
                    var det = dets.Select((val, idx) => new { V = val, I = idx }).Aggregate((max, working) => (max.V.Area > working.V.Area) ? max : working).V;
                    using (var ret = posePredictor.Detect(tmp, det))
                    using (var chip = Dlib.GetFaceChipDetails(ret, Size, 0d))
                    using (var faceChips = Dlib.ExtractImageChip<RgbPixel>(tmp, chip))
                    {
                        var dst = Path.Combine(outputDir, kvp.Key);
                        var dstDir = Path.GetDirectoryName(dst);
                        Directory.CreateDirectory(dstDir);
                        Dlib.SaveJpeg(faceChips, Path.Combine(outputDir, kvp.Key), 100);

                        var index = 0;
                        for (var row = 0; row < Size; row++)
                            for (var col = 0; col < Size; col++)
                            {
                                var rgb = faceChips[row, col];
                                r[index] += rgb.Red;
                                g[index] += rgb.Green;
                                b[index] += rgb.Blue;
                                index++;
                            }
                    }

                    imageCount++;
                }

            using (var mean = new Matrix<RgbPixel>(Size, Size))
            {
                var index = 0;
                for (var row = 0; row < Size; row++)
                    for (var col = 0; col < Size; col++)
                    {
                        var red = (double)r[index] / imageCount;
                        var green = (double)g[index] / imageCount;
                        var blue = (double)b[index] / imageCount;

                        var newRed = (byte)Math.Round(red);
                        var newGreen = (byte)Math.Round(green);
                        var newBlue = (byte)Math.Round(blue);
                        mean[row, col] = new RgbPixel(newRed, newGreen, newBlue);

                        index++;
                    }

                Dlib.SaveBmp(mean, Path.Combine(output, $"{type}.mean.bmp"));
            }
        }

        private static void Test(string dataset, string model)
        {
            try
            {
                IList<Matrix<RgbPixel>> trainingImages;
                IList<uint> trainingLabels;
                IList<Matrix<RgbPixel>> testingImages;
                IList<uint> testingLabels;

                Console.WriteLine("Start load train images");
                Load("train", dataset, null, out trainingImages, out trainingLabels);
                Console.WriteLine($"Load train images: {trainingImages.Count}");

                Console.WriteLine("Start load test images");
                Load("test", dataset,  null, out testingImages, out testingLabels);
                Console.WriteLine($"Load test images: {testingImages.Count}");

                // So with that out of the way, we can make a network instance.
                var trainNet = NativeMethods.LossMulticlassLog_age_train_type_create();
                var networkId = LossMulticlassLogRegistry.GetId(trainNet);
                LossMulticlassLogRegistry.Add(trainNet);

                using (var net = LossMulticlassLog.Deserialize(model, networkId))
                {
                    Validation("", net, trainingImages, trainingLabels, testingImages, testingLabels, true, false, out _, out _);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Train(string baseName, string dataset, uint epoch, double learningRate, double minLearningRate, uint miniBatchSize, uint validation, bool useMean)
        {
            try
            {
                IList<Matrix<RgbPixel>> trainingImages;
                IList<uint> trainingLabels;
                IList<Matrix<RgbPixel>> testingImages;
                IList<uint> testingLabels;

                var mean = useMean ? Path.Combine(dataset, "train.mean.bmp") : null;

                Console.WriteLine("Start load train images");
                Load("train", dataset, mean, out trainingImages, out trainingLabels);
                Console.WriteLine($"Load train images: {trainingImages.Count}");

                Console.WriteLine("Start load test images");
                Load("test" , dataset, mean, out testingImages, out testingLabels);
                Console.WriteLine($"Load test images: {testingImages.Count}");

                // So with that out of the way, we can make a network instance.
                var trainNet = NativeMethods.LossMulticlassLog_age_train_type_create();
                var networkId = LossMulticlassLogRegistry.GetId(trainNet);
                LossMulticlassLogRegistry.Add(trainNet);

                using (var net = new LossMulticlassLog(networkId))
                using (var trainer = new DnnTrainer<LossMulticlassLog>(net))
                {
                    trainer.SetLearningRate(learningRate);
                    trainer.SetMinLearningRate(minLearningRate);
                    trainer.SetMiniBatchSize(miniBatchSize);
                    trainer.BeVerbose();
                    trainer.SetSynchronizationFile(baseName, 180);

                    // create array box
                    var trainingImagesCount = trainingImages.Count;
                    var trainingLabelsCount = trainingLabels.Count;

                    var maxIteration = (int)Math.Ceiling(trainingImagesCount / (float)miniBatchSize);
                    var imageBatches = new Matrix<RgbPixel>[maxIteration][];
                    var labelBatches = new uint[maxIteration][];
                    for (var i = 0; i < maxIteration; i++)
                    {
                        if (miniBatchSize <= trainingImagesCount - i * miniBatchSize)
                        {
                            imageBatches[i] = new Matrix<RgbPixel>[miniBatchSize];
                            labelBatches[i] = new uint[miniBatchSize];
                        }
                        else
                        {
                            imageBatches[i] = new Matrix<RgbPixel>[trainingImagesCount % miniBatchSize];
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
                                    currentLabels[j] = trainingLabels[rIndex];
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
                                Validation(baseName, net, trainingImages, trainingLabels, testingImages, testingLabels, false, false, out var trainAccuracy, out var testAccuracy);

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
                    Validation(baseName, net, trainingImages, trainingLabels, testingImages, testingLabels, true, true, out _, out _);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Validation(string baseName,
                                       LossMulticlassLog net,
                                       IList<Matrix<RgbPixel>> trainingImages,
                                       IList<uint> trainingLabels,
                                       IList<Matrix<RgbPixel>> testingImages,
                                       IList<uint> testingLabels,
                                       bool useConsole,
                                       bool saveToXml,
                                       out double trainAccuracy,
                                       out double testAccuracy)
        {
            trainAccuracy = 0;
            testAccuracy = 0;

            using (var predictedLabels = net.Operator(trainingImages))
            {
                var numRight = 0;
                var numWrong = 0;

                // And then let's see if it classified them correctly.
                for (var i = 0; i < trainingImages.Count; ++i)
                {
                    if (predictedLabels[i] == trainingLabels[i])
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
                        if (predictedLabels2[i] == testingLabels[i])
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

    }

}