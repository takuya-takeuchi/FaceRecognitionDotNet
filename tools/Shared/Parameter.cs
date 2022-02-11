
namespace Shared
{

    internal sealed class Parameter
    {

        public string Dataset
        {
            get;
            set;
        }

        public string Model
        {
            get;
            set;
        }

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

        public uint Epoch
        {
            get;
            set;
        }

        public double LearningRate
        {
            get;
            set;
        }

        public double MinLearningRate
        {
            get;
            set;
        }

        public uint MiniBatchSize
        {
            get;
            set;
        }

        public uint Validation
        {
            get;
            set;
        }

    }

}
