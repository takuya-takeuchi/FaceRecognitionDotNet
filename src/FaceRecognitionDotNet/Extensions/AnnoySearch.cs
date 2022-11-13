using System;
using System.Collections.Generic;

using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The face search class that provides Annoy (Approximate Nearest Neighbors Oh Yeah) kind of Approximate Nearest Neighbors. This class cannot be inherited.
    /// </summary>
    public sealed class AnnoySearch : Search
    {

        #region Fileds

        private readonly IntPtr _Index;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnoySearch"/>.
        /// </summary>
        public AnnoySearch()
        {
            this._Index = NativeMethods.AnnoySearch_AnnoyIndex_new(128);
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

            this.ThrowIfDisposed();
            NativeMethods.AnnoySearch_AnnoyIndex_add_item(this._Index, item, encoding.Encoding.ToArray());
        }

        /// <summary>
        /// Build feature data set to make ready for query.
        /// </summary>
        public override void Build()
        {
            this.ThrowIfDisposed();
            // ToDo: I'm not sure why 2 *. It could be tree size.
            //       https://github.com/spotify/annoy/blob/master/examples/precision_test.cpp#L50
            NativeMethods.AnnoySearch_AnnoyIndex_build(this._Index, 2 * 128);
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

            // ToDo: What is n parameter... it could be related to threading
            using (var topList = new StdVector<int>())
            using (var distances = new StdVector<double>())
            {
                NativeMethods.AnnoySearch_AnnoyIndex_get_nns_by_vector(this._Index,
                                                                       encoding.Encoding.ToArray(),
                                                                       topK,
                                                                       -1,
                                                                       topList.NativePtr,
                                                                       distances.NativePtr);

                var dictionary = new Dictionary<int, double>();

                var topListArray = topList.ToArray();
                var distancesArray = distances.ToArray();

                var count = topListArray.Length;
                for (var index = 0; index < count; index++)
                    dictionary.Add(topListArray[index], distancesArray[index]);
                    
                return dictionary;
            }
        }

        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            NativeMethods.AnnoySearch_AnnoyIndex_delete(this._Index);
        }

        #endregion

    }

}