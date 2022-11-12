using System;
using System.Collections.Generic;
using System.IO;

using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The head pose estimator which was trained by 300W-LP dataset. This class cannot be inherited.
    /// </summary>
    public sealed class AnnoySearch : Search
    {

        #region Fileds

        private readonly IntPtr _Index;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnoySearch"/> class with the model files to estimate head pose.
        /// </summary>
        /// <param name="rollModelFile">The model file path to estimate roll angle.</param>
        /// <param name="pitchModelFile">The model file path to estimate pitch angle.</param>
        /// <param name="yawModelFile">The model file path to estimate yaw angle.</param>
        /// <exception cref="FileNotFoundException"><paramref name="rollModelFile"/>, <paramref name="pitchModelFile"/> or <paramref name="yawModelFile"/> does not exist.</exception>
        public AnnoySearch()
        {
            this._Index = NativeMethods.AnnoySearch_AnnoyIndex_new(128);
        }

        #endregion

        #region Methods

        public override void Add(int item, FaceEncoding encoding)
        {
            this.ThrowIfDisposed();
            NativeMethods.AnnoySearch_AnnoyIndex_add_item(this._Index, item, encoding.Encoding.ToArray());
        }

        public override void Build()
        {
            this.ThrowIfDisposed();
            // ToDo: I'm not sure why 2 *...
            //       https://github.com/spotify/annoy/blob/master/examples/precision_test.cpp#L50
            NativeMethods.AnnoySearch_AnnoyIndex_build(this._Index, 2 * 128);
        }

        public override IDictionary<int, double> Query(FaceEncoding encoding, uint topK)
        {
            this.ThrowIfDisposed();

            // ToDo: What is n parameter... it could be related to threading
            using (var toplist = new StdVector<int>())
            using (var distances = new StdVector<double>())
            {
                NativeMethods.AnnoySearch_AnnoyIndex_get_nns_by_vector(this._Index,
                                                                       encoding.Encoding.ToArray(),
                                                                       (ulong)topK,
                                                                       -1,
                                                                       toplist.NativePtr,
                                                                       distances.NativePtr);

                var dictionary = new Dictionary<int, double>();
                var count = toplist.Count;
                for (var index = 0; index < count; index++)
                    dictionary.Add(toplist[index], distances[index]);
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