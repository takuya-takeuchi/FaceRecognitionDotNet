using System.Collections.Generic;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to estimate human head pose from face landmark.
    /// </summary>
    public abstract class Search : DisposableObject
    {

        #region Methods

        public abstract void Add(int item, FaceEncoding encoding);

        public abstract void Build();

        public abstract IDictionary<int, double> Query(FaceEncoding encoding, uint topK);

        #endregion

    }

}