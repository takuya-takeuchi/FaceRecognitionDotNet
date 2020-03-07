using System;
using System.Collections.Generic;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;

namespace FaceRecognitionDotNet.Extensions
{

    public sealed class SimpleGenderEstimator : GenderEstimator
    {

        #region Fields

        private readonly LossMulticlassLog _Network;

        #endregion

        #region Constructors

        public SimpleGenderEstimator(string modelPath)
        {
            var ret = NativeMethods.LossMulticlassLog_gender_train_type_create();
            var networkId = LossMulticlassLogRegistry.GetId(ret);
            if (LossMulticlassLogRegistry.Contains(networkId))
                NativeMethods.LossMulticlassLog_gender_train_type_delete(ret);
            else
                LossMulticlassLogRegistry.Add(ret);

            this._Network = LossMulticlassLog.Deserialize(modelPath, networkId);
        }

        #endregion

        #region Properties

        public override Gender[] Labels
        {
            get
            {
                return new[]
                {
                    Gender.Male,
                    Gender.Female
                };
            }
        }

        #endregion

        #region Methods

        public override Gender RawPredict(MatrixBase matrix, Location location)
        {
            if (!(matrix is Matrix<RgbPixel> mat))
                throw new ArgumentException();

            using (var det = new FullObjectDetection(new Rectangle(location.Left, location.Top, location.Right, location.Bottom)))
            using (var chip = DlibDotNet.Dlib.GetFaceChipDetails(det, 227u))
            using (var faceChips = DlibDotNet.Dlib.ExtractImageChip<RgbPixel>(matrix, chip))
            using (var results = this._Network.Operator(new[] { faceChips }, 1))
                return results[0] == 0 ? Gender.Male : Gender.Female;
        }

        public override IDictionary<Gender, float> RawPredictProbability(MatrixBase matrix, Location location)
        {
            if (!(matrix is Matrix<RgbPixel> mat))
                throw new ArgumentException();

            using (var det = new FullObjectDetection(new Rectangle(location.Left, location.Top, location.Right, location.Bottom)))
            using (var chip = DlibDotNet.Dlib.GetFaceChipDetails(det, 227u, 0.25d))
            using (var faceChips = DlibDotNet.Dlib.ExtractImageChip<RgbPixel>(matrix, chip))
            {
                var results = this._Network.Probability(faceChips, 1).ToArray();
                return new Dictionary<Gender, float>
                {
                    { Gender.Male,   results[0][0] },
                    { Gender.Female, results[0][1] }
                };
            }

            #endregion

        }

    }

}