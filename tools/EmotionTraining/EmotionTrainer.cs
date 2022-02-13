using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DlibDotNet;
using DlibDotNet.Dnn;
using FaceRecognitionDotNet;
using Shared;

namespace EmotionTraining
{

    internal sealed class EmotionTrainer : ImageTrainerProgram<Emotion, double>
    {

        #region Constructors

        public EmotionTrainer(int size, string name, string description) :
            base(size, name, description)
        { }

        #endregion

        #region Methods

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

        #endregion

        #endregion

        #region ImageTrainerProgram<Emotion, double> Members

        protected override void SetEvalMode(int networkId, LossMulticlassLog net)
        {
            NativeMethods.LossMulticlassLog_emotion_train_type_eval(networkId, net.NativePtr);
        }

        protected override string GetBaseName(uint epoch, double learningRate, double minLearningRate, uint minBatchSize)
        {
            return $"Corrective_re-annotation_of_FER_CK+_KDEF-mlp_{epoch}_{learningRate}_{minLearningRate}_{minBatchSize}";
        }

        protected override bool Compare(uint predicted, Emotion expected)
        {
            return (Emotion)predicted == expected;
        }

        protected override uint Cast(Emotion label)
        {
            return (uint)label;
        }

        protected override Emotion Cast(uint label)
        {
            return (Emotion)label;
        }

        protected override int SetupNetwork()
        {
            var trainNet = NativeMethods.LossMulticlassLog_emotion_train_type_create();
            var networkId = LossMulticlassLogRegistry.GetId(trainNet);
            LossMulticlassLogRegistry.Add(trainNet);
            return networkId;
        }

        protected override void Load(string directory, string type, out IList<Matrix<double>> images, out IList<Emotion> labels)
        {
            var imageList = new List<Matrix<double>>();
            var vectorList = new List<double[]>();
            var labelList = new List<Emotion>();

            var path = $"{Path.Combine(directory, type)}_cache.dat";
            if (File.Exists(path))
            {
                Logger.Info($"Use Cache {path}");

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
                Logger.Info($"Save Cache {path}");

                var cache = new Cache();
                cache.Points = vectorList;
                cache.Emotion = labelList;
                var s = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                s.Serialize(fs, cache);
            }
        }

        #endregion

    }

}