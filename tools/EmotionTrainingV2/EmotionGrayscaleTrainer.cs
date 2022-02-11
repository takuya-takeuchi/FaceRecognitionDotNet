using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using DlibDotNet;
using DlibDotNet.Dnn;
using FaceRecognitionDotNet;
using Shared;
using Image = FaceRecognitionDotNet.Image;
using Rectangle = DlibDotNet.Rectangle;

namespace EmotionTrainingV2
{

    internal sealed class EmotionGrayscaleTrainer : ImageTrainerProgram<Emotion, byte>
    {

        #region Constructors

        public EmotionGrayscaleTrainer(int size, string name, string description) :
            base(size, name, description)
        { }

        #endregion

        #region ImageTrainerProgram<Emotion, RgbPixel> Members

        protected override void SetEvalMode(int networkId, LossMulticlassLog net)
        {
            NativeMethods.LossMulticlassLog_emotion_train_type_eval(networkId, net.NativePtr);
        }

        protected override string GetBaseName(uint epoch, double learningRate, double minLearningRate, uint minBatchSize)
        {
            return $"Corrective_re-annotation_of_FER_CK+_KDEF-cnn_{epoch}_{learningRate}_{minLearningRate}_{minBatchSize}";
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

        protected override void Demo(FaceRecognition faceRecognition, string modelFile, string imageFile, Image image, Location location)
        {
            var networkId = SetupNetwork();

            using (var net = LossMulticlassLog.Deserialize(modelFile, networkId))
            using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(imageFile))
            using (var org = new Bitmap(bitmap.Width, bitmap.Height))
            using (var g = Graphics.FromImage(org))
            {
                g.DrawImage(bitmap, new System.Drawing.Rectangle(0, 0, org.Width, org.Height), new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
                
                var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);
                var dPoint = new[]
                {
                    new DPoint(rect.Left, rect.Top),
                    new DPoint(rect.Right, rect.Top),
                    new DPoint(rect.Left, rect.Bottom),
                    new DPoint(rect.Right, rect.Bottom),
                };

                using (var tmp = Dlib.LoadImageAsMatrix<byte>(imageFile))
                using (var face = Dlib.ExtractImage4Points(tmp, dPoint, this.Size, this.Size))
                {
                    this.SetEvalMode(networkId, net);
                    var results = net.Probability(face, 1).ToArray();

                    var labels = net.GetLabels();
                    var dictionary = new Dictionary<string, float>();
                    for (var index = 0; index < labels.Length; index++)
                        dictionary.Add(labels[index], results[0][index]);

                    var maxResult = dictionary.Aggregate((max, working) => (max.Value > working.Value) ? max : working);
                    var emotion = maxResult.Key;
                    var probability = maxResult.Value;
                    
                    using (var p = new Pen(Color.Red, bitmap.Width / 200f))
                    using (var b = new SolidBrush(Color.Blue))
                    using (var font = new Font("Calibri", 16))
                    {
                        g.DrawRectangle(p, rect.Left, rect.Top, rect.Width, rect.Height);

                        g.DrawString($"{emotion}\n({probability})", font, b, new PointF(rect.Left + 10, rect.Top + 10));
                    }

                    org.Save("demo.jpg");
                }
            }
        }

        protected override int SetupNetwork()
        {
            var trainNet = NativeMethods.LossMulticlassLog_emotion_train_type6_create();
            var networkId = LossMulticlassLogRegistry.GetId(trainNet);
            LossMulticlassLogRegistry.Add(trainNet);
            return networkId;
        }

        protected override void Load(string directory, string type, out IList<Matrix<byte>> images, out IList<Emotion> labels)
        {
            var imageList = new List<Matrix<byte>>();
            var labelList = new List<Emotion>();

            //const int max = 300;
            //var count = 0;
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

                            using (var tmp = Dlib.LoadImageAsMatrix<byte>(imagePath))
                            {
                                var m = new Matrix<byte>(this.Size, this.Size);
                                Dlib.ResizeImage(tmp, m);
                                imageList.Add(m);
                                labelList.Add(emotion);
                            }

                            //count++;
                            break;
                    }

                    //if (max <= count)
                    //    break;
                } while (true);
            }

            images = imageList;
            labels = labelList;
        }
        
        #endregion

    }

}