using System.Collections.Generic;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    public abstract class GenderEstimator : DisposableObject
    {
        
        #region Properties

        public abstract Gender[] Labels
        {
            get;
        }

        #endregion

        #region Methods

        public Gender Predict(Image image, Location location)
        {
            return this.RawPredict(image.Matrix, location);
        }

        public IDictionary<Gender, float> PredictProbability(Image image, Location location)
        {
            return this.RawPredictProbability(image.Matrix, location);
        }

        public abstract Gender RawPredict(MatrixBase matrix, Location location);

        public abstract IDictionary<Gender, float> RawPredictProbability(MatrixBase matrix, Location location);

        #endregion

    }

}