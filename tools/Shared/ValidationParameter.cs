using System.Collections.Generic;

using DlibDotNet;
using DlibDotNet.Dnn;

namespace Shared
{

    public sealed class ValidationParameter<T>
    {

        public string BaseName
        {
            get;
            set;
        }

        public string Output
        {
            get;
            set;
        }

        public LossMulticlassLog Trainer
        {
            get;
            set;
        }

        public IList<Matrix<RgbPixel>> TrainingImages
        {
            get;
            set;
        }

        public IList<T> TrainingLabels
        {
            get;
            set;
        }

        public IList<Matrix<RgbPixel>> TestingImages
        {
            get;
            set;
        }

        public IList<T> TestingLabels
        {
            get;
            set;
        }

        public bool UseConsole
        {
            get;
            set;
        }

        public bool SaveToXml
        {
            get;
            set;
        }

        public bool OutputDiffLog
        {
            get;
            set;
        }

    }

}
