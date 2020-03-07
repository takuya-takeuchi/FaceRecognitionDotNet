using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;

namespace FaceRecognitionDotNet.Extensions
{

    public sealed class SimpleAgeEstimator : AgeEstimator
    {

        #region Fields

        private readonly LossMulticlassLog _Network;

        #endregion

        #region Constructors

        public SimpleAgeEstimator(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException(modelPath);

            var ret = NativeMethods.LossMulticlassLog_age_train_type_create();
            var networkId = LossMulticlassLogRegistry.GetId(ret);
            if (LossMulticlassLogRegistry.Contains(networkId))
                NativeMethods.LossMulticlassLog_age_train_type_delete(ret);
            else
                LossMulticlassLogRegistry.Add(ret);

            this._Network = LossMulticlassLog.Deserialize(modelPath, networkId);
        }

        #endregion

        #region Properties

        public override AgeRange[] Labels
        {
            get
            {
                return new[]
                {
                    new AgeRange(0,2),
                    new AgeRange(4,6),
                    new AgeRange(8,13),
                    new AgeRange(15,20),
                    new AgeRange(25,32),
                    new AgeRange(38,43),
                    new AgeRange(48,53),
                    new AgeRange(60,100)
                };
            }
        }

        #endregion

        #region Methods

        public override uint RawPredict(MatrixBase matrix, Location location)
        {
            if (!(matrix is Matrix<RgbPixel> mat))
                throw new ArgumentException();

            using (var det = new FullObjectDetection(new Rectangle(location.Left, location.Top, location.Right, location.Bottom)))
            using (var chip = DlibDotNet.Dlib.GetFaceChipDetails(det, 227u, 0.25d))
            using (var faceChips = DlibDotNet.Dlib.ExtractImageChip<RgbPixel>(matrix, chip))
            using (var results = this._Network.Operator(new[] {faceChips}, 1))
                return results[0];
        }

        public override IDictionary<uint, float> RawPredictProbability(MatrixBase matrix, Location location)
        {
            if (!(matrix is Matrix<RgbPixel> mat))
                throw new ArgumentException();

            using (var det = new FullObjectDetection(new Rectangle(location.Left, location.Top, location.Right, location.Bottom)))
            using (var chip = DlibDotNet.Dlib.GetFaceChipDetails(det, 227u, 0.25d))
            using (var faceChips = DlibDotNet.Dlib.ExtractImageChip<RgbPixel>(matrix, chip))
            {
                var results = this._Network.Probability(faceChips, 1).ToArray();
                var predict = results[0];
                return predict.Select((n, index) => new { index, n }).ToDictionary(n => (uint)n.index, n => n.n);
            }
        }

        #endregion

    }

}