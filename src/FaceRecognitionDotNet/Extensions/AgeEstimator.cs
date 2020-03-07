using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    public abstract class AgeEstimator : DisposableObject, IExtension
    {

        #region Properties

        public abstract AgeRange[] Labels
        {
            get;
        }

        #endregion

        #region Methods

        public uint Predict(Image image, Location location)
        {
            return this.RawPredict(image.Matrix, location);
        }

        public IDictionary<uint, float> PredictProbability(Image image, Location location)
        {
            return this.RawPredictProbability(image.Matrix, location);
        }

        public abstract uint RawPredict(MatrixBase matrix, Location location);

        public abstract IDictionary<uint, float> RawPredictProbability(MatrixBase matrix, Location location);

        #endregion

    }

}