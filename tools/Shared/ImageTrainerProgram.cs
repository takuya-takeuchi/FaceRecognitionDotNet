using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Extensions.CommandLineUtils;
using NLog;

using DlibDotNet;
using DlibDotNet.Dnn;
using FaceRecognitionDotNet;
using Image = FaceRecognitionDotNet.Image;
using Rectangle = DlibDotNet.Rectangle;

namespace Shared
{

    public abstract class ImageTrainerProgram<T, C>
        where T : struct
        where C : struct
    {

        #region Fields

        protected readonly int Size;

        private readonly string _Name;

        private readonly string _Description;

        protected static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        protected ImageTrainerProgram(int size, string name, string description)
        {
            this.Size = size;
            this._Name = name;
            this._Description = description;
        }

        #endregion

        #region Methods

        protected abstract uint Cast(T label);

        protected abstract T Cast(uint label);

        protected abstract bool Compare(uint predicted, T expected);
        
        protected virtual void Demo(FaceRecognition faceRecognition, string modelFile, string imageFile, Image image, Location location)
        {

        }

        protected abstract string GetBaseName(uint epoch, double learningRate, double minLearningRate, uint minBatchSize);

        protected abstract void Load(string directory, string type, out IList<Matrix<C>> images, out IList<T> labels);

        protected virtual void SetEvalMode(int networkId, LossMulticlassLog net)
        {

        }

        protected abstract int SetupNetwork();

        public int Start(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = this._Name;
            app.Description = this._Description;
            app.HelpOption("-h|--help");

            app.Command("clean", command =>
            {
                var outputOption = command.Option("-o|--output", "The output directory path.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    if (!outputOption.HasValue() || !Directory.Exists(outputOption.Value()))
                    {
                        Logger.Error($"'{outputOption.Value()} is missing or output option is not specified");
                        return -1;
                    }
                    
                    Logger.Info($"             Output: {outputOption.Value()}");
                    Logger.Info("");
                    
                    Clean(outputOption.Value());

                    return 0;
                });
            });

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
                var outputOption = command.Option("-o|--output", "The output directory path.", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var dataset = datasetOption.Value();
                    if (!datasetOption.HasValue() || !Directory.Exists(dataset))
                    {
                        Logger.Error("dataset does not exist");
                        return -1;
                    }

                    var epoch = epochDefault;
                    if (epochOption.HasValue() && !uint.TryParse(epochOption.Value(), out epoch))
                    {
                        Logger.Error("epoch is invalid value");
                        return -1;
                    }

                    var learningRate = learningRateDefault;
                    if (learningRateOption.HasValue() && !double.TryParse(learningRateOption.Value(), NumberStyles.Float, Thread.CurrentThread.CurrentCulture.NumberFormat, out learningRate))
                    {
                        Logger.Error("learning rate is invalid value");
                        return -1;
                    }

                    var minLearningRate = minLearningRateDefault;
                    if (minLearningRateOption.HasValue() && !double.TryParse(minLearningRateOption.Value(), NumberStyles.Float, Thread.CurrentThread.CurrentCulture.NumberFormat, out minLearningRate))
                    {
                        Logger.Error("minimum learning rate is invalid value");
                        return -1;
                    }

                    var minBatchSize = minBatchSizeDefault;
                    if (minBatchSizeOption.HasValue() && !uint.TryParse(minBatchSizeOption.Value(), out minBatchSize))
                    {
                        Logger.Error("minimum batch size is invalid value");
                        return -1;
                    }

                    var validation = validationDefault;
                    if (validationOption.HasValue() && !uint.TryParse(validationOption.Value(), out validation) || validation == 0)
                    {
                        Logger.Error("validation interval is invalid value");
                        return -1;
                    }

                    var output = "result";
                    if (outputOption.HasValue())
                    {
                        output = outputOption.Value();
                    }

                    Directory.CreateDirectory(output);

                    var useMean = useMeanOption.HasValue();

                    Logger.Info($"            Dataset: {dataset}");
                    Logger.Info($"              Epoch: {epoch}");
                    Logger.Info($"      Learning Rate: {learningRate}");
                    Logger.Info($"  Min Learning Rate: {minLearningRate}");
                    Logger.Info($"     Min Batch Size: {minBatchSize}");
                    Logger.Info($"Validation Interval: {validation}");
                    Logger.Info($"           Use Mean: {useMean}");
                    Logger.Info($"             Output: {output}");
                    Logger.Info("");

                    var name = this.GetBaseName(epoch, learningRate, minLearningRate, minBatchSize);
                    var baseName = Path.Combine(output, name);
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
                var modelOption = command.Option("-m|--model", "The model file path", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var dataset = datasetOption.Value();
                    if (!datasetOption.HasValue() || !Directory.Exists(dataset))
                    {
                        Logger.Error("dataset does not exist");
                        return -1;
                    }

                    var model = modelOption.Value();
                    if (!modelOption.HasValue() || !File.Exists(model))
                    {
                        Logger.Error("model does not exist");
                        return -1;
                    }

                    Logger.Info($"Dataset: {dataset}");
                    Logger.Info($"  Model: {model}");
                    Logger.Info("");

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
                var modelOption = command.Option("-m|--model", "The model file path", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var image = imageOption.Value();
                    if (!imageOption.HasValue() || !File.Exists(image))
                    {
                        Logger.Error("image does not exist");
                        return -1;
                    }

                    var model = modelOption.Value();
                    if (!modelOption.HasValue() || !File.Exists(model))
                    {
                        Logger.Error("model file does not exist");
                        return -1;
                    }

                    Logger.Info($"Image File: {image}");
                    Logger.Info($"     Model: {model}");
                    Logger.Info("");

                    var networkId = SetupNetwork();

                    using (var net = LossMulticlassLog.Deserialize(model, networkId))
                    using (var fr = FaceRecognition.Create("Models"))
                    using (var img = FaceRecognition.LoadImageFile(image))
                    {
                        var location = fr.FaceLocations(img).FirstOrDefault();
                        if (location == null)
                        {
                            Logger.Info("Missing face");
                            return -1;
                        }

                        var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);
                        var dPoint = new[]
                        {
                            new DPoint(rect.Left, rect.Top),
                            new DPoint(rect.Right, rect.Top),
                            new DPoint(rect.Left, rect.Bottom),
                            new DPoint(rect.Right, rect.Bottom),
                        };
                        using (var tmp = Dlib.LoadImageAsMatrix<byte>(image))
                        {
                            using (var face = Dlib.ExtractImage4Points(tmp, dPoint, this.Size, this.Size))
                            {
                                this.SetEvalMode(networkId, net);
                                using (var predictedLabels = net.Operator(face))
                                    Logger.Info($"{this.Cast(predictedLabels[0])}");
                            }
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

                    using (var fr = FaceRecognition.Create(directory))
                    using (var image = FaceRecognition.LoadImageFile(imageFile))
                    {
                        var loc = fr.FaceLocations(image).FirstOrDefault();
                        if (loc == null)
                        {
                            Console.WriteLine("No face is detected");
                            return 0;
                        }

                        this.Demo(fr, modelFile, imageFile, image, loc);
                    }

                    return 0;
                });
            });

            return app.Execute(args);
        }

        #region Helpers

        private void Clean(string directory)
        {
            var files = Directory.GetFiles(directory, "*.tmp").ToArray();
            foreach (var file in files)
            {
                try
                {
                    var newPath = Path.ChangeExtension(file, "dat");
                    // So with that out of the way, we can make a network instance.
                    var networkId = SetupNetwork();

                    using (var net = LossMulticlassLog.Deserialize(file, networkId))
                    {
                        net.Clean();
                        LossMulticlassLog.Serialize(net, newPath);
                    }

                    File.Delete(file);
                    Logger.Info($"'{file}' was cleaned and '{newPath}' is created.");
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
            }
        }

        private void Test(Parameter parameter)
        {
            try
            {
                IList<Matrix<C>> trainingImages;
                IList<T> trainingLabels;
                IList<Matrix<C>> testingImages;
                IList<T> testingLabels;

                Logger.Info("Start load train images");
                Load(parameter.Dataset, "train", out trainingImages, out trainingLabels);
                Logger.Info($"Load train images: {trainingImages.Count}");

                Logger.Info("Start load test images");
                Load(parameter.Dataset, "test", out testingImages, out testingLabels);
                Logger.Info($"Load test images: {testingImages.Count}");
                Logger.Info("");

                // So with that out of the way, we can make a network instance.
                var networkId = SetupNetwork();

                using (var net = LossMulticlassLog.Deserialize(parameter.Model, networkId))
                {
                    this.SetEvalMode(networkId, net);
                    var validationParameter = new ValidationParameter<T, C>
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
                Logger.Error(e.Message);
            }
        }

        private void Train(Parameter parameter)
        {
            try
            {
                IList<Matrix<C>> trainingImages;
                IList<T> trainingLabels;
                IList<Matrix<C>> testingImages;
                IList<T> testingLabels;

                Logger.Info("Start load train images");
                Load(parameter.Dataset, "train", out trainingImages, out trainingLabels);
                Logger.Info($"Load train images: {trainingImages.Count}");

                Logger.Info("Start load test images");
                Load(parameter.Dataset, "test", out testingImages, out testingLabels);
                Logger.Info($"Load test images: {testingImages.Count}");
                Logger.Info("");

                // So with that out of the way, we can make a network instance.
                var networkId = SetupNetwork();

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
                    var imageBatches = new Matrix<C>[maxIteration][];
                    var labelBatches = new uint[maxIteration][];
                    for (var i = 0; i < maxIteration; i++)
                    {
                        if (miniBatchSize <= trainingImagesCount - i * miniBatchSize)
                        {
                            imageBatches[i] = new Matrix<C>[miniBatchSize];
                            labelBatches[i] = new uint[miniBatchSize];
                        }
                        else
                        {
                            imageBatches[i] = new Matrix<C>[trainingImagesCount % miniBatchSize];
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
                                    currentLabels[j] = this.Cast(trainingLabels[rIndex]);
                                    index++;
                                }
                            }

                            for (var i = 0; i < maxIteration; i++)
                                LossMulticlassLog.TrainOneStep(trainer, imageBatches[i], labelBatches[i]);

                            var lr = trainer.GetLearningRate();
                            var loss = trainer.GetAverageLoss();

                            var trainLog = $"Epoch: {e}, learning Rate: {lr}, average loss: {loss}";
                            Logger.Info(trainLog);
                            sw.WriteLine(trainLog);

                            if (e >= 0 && e % validation == 0)
                            {
                                var validationParameter = new ValidationParameter<T, C>
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
                                Logger.Info(validationLog);
                                sw.WriteLine(validationLog);

                                var name = this.GetBaseName(parameter.Epoch,
                                                            parameter.LearningRate,
                                                            parameter.MinLearningRate,
                                                            parameter.MiniBatchSize);

                                UpdateBestModelFile(net, testAccuracy, parameter.Output, name, "test");
                                UpdateBestModelFile(net, trainAccuracy, parameter.Output, name, "train");
                            }

                            if (lr < minLearningRate)
                            {
                                Logger.Info($"Stop training: {lr} < {minLearningRate}");
                                break;
                            }
                        }

                    // wait for training threads to stop
                    trainer.GetNet();
                    Logger.Info("done training");

                    net.Clean();
                    LossMulticlassLog.Serialize(net, $"{baseName}.tmp");

                    // Now let's run the training images through the network.  This statement runs all the
                    // images through it and asks the loss layer to convert the network's raw output into
                    // labels.  In our case, these labels are the numbers between 0 and 9.
                    var validationParameter2 = new ValidationParameter<T, C>
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

                    // clean up tmp files
                    Clean(parameter.Output);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        private static void UpdateBestModelFile(LossMulticlassLog net, double accuracy, string output, string basename, string postfix)
        {
            bool save;
            var candidates = new Dictionary<string, double>();

            var files = Directory.GetFiles(output, $"{basename}_{postfix}_best_*.tmp").ToArray();
            if (files.Any())
            {
                var culture = new CultureInfo("en-US");
                foreach (var file in files)
                {
                    var value = Path.GetFileNameWithoutExtension(file).Replace($"{basename}_{postfix}_best_", "");
                    if (double.TryParse(value, NumberStyles.Float, culture, out var tmp))
                        candidates.Add(file, tmp);
                }

                // there is no last best file or latest accuracy gets over old one
                save = !candidates.Any() || candidates.All(pair => pair.Value < accuracy);
            }
            else
            {
                save = true;
            }

            if (save)
            {
                var path = Path.Combine(output, $"{basename}_{postfix}_best_{accuracy}.tmp");
                LossMulticlassLog.Serialize(net, path);
                Logger.Info($"Best Accuracy Model file is saved for {postfix} [{accuracy}]");

                // delete old files
                foreach (var (key, _) in candidates)
                {
                    try
                    {
                        File.Delete(key);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Failed to delete '{key}'. Reason: {e.Message}");
                    }
                }
            }
        }

        private void Validation(ValidationParameter<T, C> parameter,
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
                    if (this.Compare(predictedLabels[i], trainingLabels[i]))
                        ++numRight;
                    else
                        ++numWrong;
                }

                if (useConsole)
                {
                    Logger.Info($"training num_right: {numRight}");
                    Logger.Info($"training num_wrong: {numWrong}");
                    Logger.Info($"training accuracy:  {numRight / (double)(numRight + numWrong)}");
                }

                trainAccuracy = numRight / (double)(numRight + numWrong);

                using (var predictedLabels2 = net.Operator(testingImages))
                {
                    numRight = 0;
                    numWrong = 0;
                    for (var i = 0; i < testingImages.Count; ++i)
                    {
                        if (this.Compare(predictedLabels2[i], testingLabels[i]))
                            ++numRight;
                        else
                            ++numWrong;
                    }

                    if (useConsole)
                    {
                        Logger.Info($"testing num_right: {numRight}");
                        Logger.Info($"testing num_wrong: {numWrong}");
                        Logger.Info($"testing accuracy:  {numRight / (double)(numRight + numWrong)}");
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
