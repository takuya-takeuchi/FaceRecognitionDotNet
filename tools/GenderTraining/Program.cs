using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DlibDotNet;
using DlibDotNet.Dnn;
using Microsoft.Extensions.CommandLineUtils;

namespace GenderTraining
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
            app.Name = nameof(GenderTraining);
            app.Description = "The program for training UTKFace dataset";
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

                    Console.WriteLine($"            Dataset: {dataset}");
                    Console.WriteLine($"              Epoch: {epoch}");
                    Console.WriteLine($"      Learning Rate: {learningRate}");
                    Console.WriteLine($"  Min Learning Rate: {minLearningRate}");
                    Console.WriteLine($"     Min Batch Size: {minBatchSize}");
                    Console.WriteLine($"Validation Interval: {validation}");
                    Console.WriteLine();

                    var baseName = $"utkface-gender-network_{epoch}_{learningRate}_{minLearningRate}_{minBatchSize}";
                    Train(baseName, dataset, epoch, learningRate, minLearningRate, minBatchSize, validation);

                    return 0;
                });
            });

            app.Command("test", command =>
            {
                var datasetOption = command.Option("-d|--dataset", "The directory of dataset", CommandOptionType.SingleValue);
                var modelOption = command.Option("-m|--model", $"The model file.", CommandOptionType.SingleValue);

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

                    Test(dataset, model);

                    return 0;
                });
            });
            
            return app.Execute(args);
        }

        #region Helpers

        private static void Load(string directory, out IList<Matrix<RgbPixel>> images, out IList<uint> labels)
        {
            var imageList = new List<Matrix<RgbPixel>>();
            var labelList = new List<uint>();
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                var name = Path.GetFileName(file);
                var s = name.Split('_');
                if (s.Length != 4 || !uint.TryParse(s[1], out var gender))
                    continue;

                using (var tmp = Dlib.LoadImageAsMatrix<RgbPixel>(file))
                {
                    var m = new Matrix<RgbPixel>(Size, Size);
                    Dlib.ResizeImage(tmp, m);
                    imageList.Add(m);
                    labelList.Add(gender);
                }
            }

            images = imageList;
            labels = labelList;
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
                Load(Path.Combine(dataset, "train"), out trainingImages, out trainingLabels);
                Console.WriteLine($"Load train images: {trainingImages.Count}");

                Console.WriteLine("Start load test images");
                Load(Path.Combine(dataset, "test"), out testingImages, out testingLabels);
                Console.WriteLine($"Load test images: {testingImages.Count}");

                // So with that out of the way, we can make a network instance.
                var trainNet = NativeMethods.LossMulticlassLog_gender_train_type_create();
                var networkId = LossMulticlassLogRegistry.GetId(trainNet);
                LossMulticlassLogRegistry.Add(trainNet);

                using (var net = LossMulticlassLog.Deserialize(model, networkId))
                {
                    Validation("", net, trainingImages, trainingLabels, testingImages, testingLabels, true, false, out _, out _);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void Train(string baseName, string dataset, uint epoch, double learningRate, double minLearningRate, uint miniBatchSize, uint validation)
        {
            try
            {
                IList<Matrix<RgbPixel>> trainingImages;
                IList<uint> trainingLabels;
                IList<Matrix<RgbPixel>> testingImages;
                IList<uint> testingLabels;

                Console.WriteLine("Start load train images");
                Load(Path.Combine(dataset, "train"), out trainingImages, out trainingLabels);
                Console.WriteLine($"Load train images: {trainingImages.Count}");

                Console.WriteLine("Start load test images");
                Load(Path.Combine(dataset, "test"), out testingImages, out testingLabels);
                Console.WriteLine($"Load test images: {testingImages.Count}");

                // So with that out of the way, we can make a network instance.
                var trainNet = NativeMethods.LossMulticlassLog_gender_train_type_create();
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
                Console.WriteLine(e.Message);
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