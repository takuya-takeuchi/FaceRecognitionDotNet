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

            var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);
            var dPoint = new[]
            {
                new DPoint(rect.Left, rect.Top),
                new DPoint(rect.Right, rect.Top),
                new DPoint(rect.Left, rect.Bottom),
                new DPoint(rect.Right, rect.Bottom),
            };
            using (var img = DlibDotNet.Dlib.ExtractImage4Points(mat, dPoint, 227, 227))
            using (var results = this._Network.Operator(new[] { img }, 1))
                return results[0] == 0 ? Gender.Male : Gender.Female;
        }

        public override IDictionary<Gender, float> RawPredictProbability(MatrixBase matrix, Location location)
        {
            if (!(matrix is Matrix<RgbPixel> mat))
                throw new ArgumentException();

            var rect = new Rectangle(location.Left, location.Top, location.Right, location.Bottom);
            var dPoint = new[]
            {
                new DPoint(rect.Left, rect.Top),
                new DPoint(rect.Right, rect.Top),
                new DPoint(rect.Left, rect.Bottom),
                new DPoint(rect.Right, rect.Bottom),
            };
            using (var img = DlibDotNet.Dlib.ExtractImage4Points(mat, dPoint, 227, 227))
            {
                var results = this._Network.Probability(img, 1).ToArray();
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