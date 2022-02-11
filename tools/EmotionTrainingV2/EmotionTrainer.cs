using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DlibDotNet;
using DlibDotNet.Dnn;
using Shared;

namespace EmotionTrainingV2
{

    internal sealed class EmotionTrainer : ImageTrainerProgram<Emotion, RgbPixel>
    {

        #region Constructors
        public EmotionTrainer(int size, string name, string description) :
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

        protected override int SetupNetwork()
        {
            var trainNet = NativeMethods.LossMulticlassLog_emotion_train_type5_create();
            var networkId = LossMulticlassLogRegistry.GetId(trainNet);
            LossMulticlassLogRegistry.Add(trainNet);
            return networkId;
        }

        protected override void Load(string directory, string type, out IList<Matrix<RgbPixel>> images, out IList<Emotion> labels)
        {
            var imageList = new List<Matrix<RgbPixel>>();
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

                            using (var tmp = Dlib.LoadImageAsMatrix<RgbPixel>(imagePath))
                            {
                                var m = new Matrix<RgbPixel>(this.Size, this.Size);
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