using System.Collections.Generic;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// An abstract base class that provides functionality to query face encoding from face encoding set.
    /// </summary>
    public abstract class Search : DisposableObject
    {

        #region Methods

        /// <summary>
        /// Add an <see cref="FaceEncoding"/> to feature data set.
        /// </summary>
        /// <param name="item">The label to specify <param name="encoding">.</param></param>
        /// <param name="encoding">A known face encodings to be added to feature data set.</param>
        public abstract void Add(int item, FaceEncoding encoding);

        /// <summary>
        /// Build feature data set to make ready for query.
        /// </summary>
        public abstract void Build();

        /// <summary>
        /// Searches for elements that are closed to given face encoding, and returns the top K occurrence within the entire feature data set.
        /// </summary>
        /// <param name="encoding">A face encodings to query in feature data set.</param>
        /// <param name="topK">The number of most likely outcomes to query the label.</param>
        /// <returns>A dictionary of label and distance.</returns>
        public abstract IDictionary<int, double> Query(FaceEncoding encoding, uint topK);

        #endregion

    }

}