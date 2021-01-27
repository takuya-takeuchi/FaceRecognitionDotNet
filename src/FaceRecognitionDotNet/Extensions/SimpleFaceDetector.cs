using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DlibDotNet;
using DlibDotNet.Dnn;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The face detector which was trained by custom dataset. This class cannot be inherited.
    /// </summary>
    public sealed class SimpleFaceDetector : FaceDetector
    {

        #region Fields

        private readonly LossMulticlassLog _Network;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleFaceDetector"/> class with the model file path that this detector uses.
        /// </summary>
        /// <param name="modelPath">The model file path that this detector uses.</param>
        /// <exception cref="FileNotFoundException">The model file is not found.</exception>
        public SimpleFaceDetector(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException(modelPath);

            var ret = NativeMethods.LossMulticlassLog_age_train_type_create();
            var networkId = LossMulticlassLogRegistry.GetId(ret);
            if (LossMulticlassLogRegistry.Contains(networkId))
                NativeMethods.LossMulticlassLog_age_train_type_delete(ret);
            else
                LossMulticlassLogRegistry.Add(ret);

            this._Network = LossMulticlassLog.Deserialize(modelPath, networkId);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns an enumerable collection of face location correspond to all faces in specified image.
        /// </summary>
        /// <param name="matrix">The matrix contains a face.</param>
        /// <param name="numberOfTimesToUpsample">The number of times to up-sample the image when finding faces.</param>
        /// <returns>An enumerable collection of face location correspond to all faces.</returns>
        protected override IEnumerable<Location> RawDetect(MatrixBase matrix, uint upsamplingAmount))
        {

        }

        #endregion

    }

}