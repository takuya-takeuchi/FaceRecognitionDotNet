using System;
using System.Collections.Generic;
using System.Linq;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The face search class that provides kNN (K-Nearest Neighbors). This class cannot be inherited.
    /// </summary>
    public sealed class KNearestNeighborSearch : Search
    {

        #region Fields

        private readonly IDictionary<int, FaceEncoding> _Dictionary = new Dictionary<int, FaceEncoding>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KNearestNeighborSearch"/>.
        /// </summary>
        public KNearestNeighborSearch()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add an <see cref="FaceEncoding"/> to feature data set.
        /// </summary>
        /// <param name="item">The label to specify <param name="encoding">.</param></param>
        /// <param name="encoding">A known face encodings to be added to feature data set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="encoding"/> or this object is disposed.</exception>
        public override void Add(int item, FaceEncoding encoding)
        {
            if (encoding == null) 
                throw new ArgumentNullException(nameof(encoding));

            encoding.ThrowIfDisposed();

            this._Dictionary[item] = encoding;
        }

        /// <summary>
        /// Build feature data set to make ready for query.
        /// </summary>
        /// <exception cref="ObjectDisposedException">this object is disposed.</exception>
        public override void Build()
        {
            this.ThrowIfDisposed();

            // nothing to do
        }

        /// <summary>
        /// Searches for elements that are closed to given face encoding, and returns the top K occurrence within the entire feature data set.
        /// </summary>
        /// <param name="encoding">A face encodings to query in feature data set.</param>
        /// <param name="topK">The number of most likely outcomes to query the label.</param>
        /// <returns>A dictionary of label and distance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is null.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="encoding"/> or this object is disposed.</exception>
        public override IDictionary<int, double> Query(FaceEncoding encoding, uint topK)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            encoding.ThrowIfDisposed();

            this.ThrowIfDisposed();

            var dictionary = new Dictionary<int, double>();

            if (topK == 1)
            {
                var index = -1;
                var distance = double.MaxValue;
                foreach (var kvp in this._Dictionary)
                {
                    var tmp = FaceRecognition.FaceDistance(encoding, kvp.Value);
                    if (!(tmp < distance)) 
                        continue;

                    distance = tmp;
                    index = kvp.Key;
                }

                if (index >= 0)
                    dictionary.Add(index, distance);
            }
            else
            {
                var results = this._Dictionary.Select(kvp => new Tuple<int, double>(kvp.Key, FaceRecognition.FaceDistance(kvp.Value, encoding))).ToList();
                results.Sort((tuple1, tuple2) => tuple1.Item2.CompareTo(tuple2.Item2));

                var max = Math.Min(topK, results.Count);
                for (var index = 0; index < max; index++)
                    dictionary.Add(results[index].Item1, results[index].Item2);
            }

            return dictionary;
        }

        #endregion

    }

}