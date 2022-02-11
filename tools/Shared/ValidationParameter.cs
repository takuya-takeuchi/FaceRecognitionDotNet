using System.Collections.Generic;

using DlibDotNet;
using DlibDotNet.Dnn;

namespace Shared
{

    public sealed class ValidationParameter<T, C>
        where T : struct
        where C : struct
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

        public IList<Matrix<C>> TrainingImages
        {
            get;
            set;
        }

        public IList<T> TrainingLabels
        {
            get;
            set;
        }

        public IList<Matrix<C>> TestingImages
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
