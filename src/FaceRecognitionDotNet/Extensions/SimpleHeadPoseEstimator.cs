using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DlibDotNet;

namespace FaceRecognitionDotNet.Extensions
{

    /// <summary>
    /// The head pose estimator which was trained by 300W-LP dataset. This class cannot be inherited.
    /// </summary>
    public sealed class SimpleHeadPoseEstimator : HeadPoseEstimator
    {

        #region Fileds

        private readonly RadialBasisKernel<double, Matrix<double>> _RollKernel;

        private readonly RadialBasisKernel<double, Matrix<double>> _PitchKernel;

        private readonly RadialBasisKernel<double, Matrix<double>> _YawKernel;

        private readonly Krls<double, RadialBasisKernel<double, Matrix<double>>> _RollEstimator;

        private readonly Krls<double, RadialBasisKernel<double, Matrix<double>>> _PitchEstimator;

        private readonly Krls<double, RadialBasisKernel<double, Matrix<double>>> _YawEstimator;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleHeadPoseEstimator"/> class with the model files to estimate head pose.
        /// </summary>
        /// <param name="rollModelFile">The model file path to estimate roll angle.</param>
        /// <param name="pitchModelFile">The model file path to estimate pitch angle.</param>
        /// <param name="yawModelFile">The model file path to estimate yaw angle.</param>
        /// <exception cref="FileNotFoundException"><paramref name="rollModelFile"/>, <paramref name="pitchModelFile"/> or <paramref name="yawModelFile"/> does not exist.</exception>
        public SimpleHeadPoseEstimator(string rollModelFile,
                                       string pitchModelFile,
                                       string yawModelFile)
        {
            if (!File.Exists(rollModelFile))
                throw new FileNotFoundException($"{nameof(rollModelFile)} does not exist.", nameof(rollModelFile));
            if (!File.Exists(pitchModelFile))
                throw new FileNotFoundException($"{nameof(pitchModelFile)} does not exist.", nameof(pitchModelFile));
            if (!File.Exists(yawModelFile))
                throw new FileNotFoundException($"{nameof(yawModelFile)} does not exist.", nameof(yawModelFile));

            // gamma parameter is meaningless
            this._RollKernel = new RadialBasisKernel<double, Matrix<double>>(0.1, 0, 0);
            this._PitchKernel = new RadialBasisKernel<double, Matrix<double>>(0.1, 0, 0);
            this._YawKernel = new RadialBasisKernel<double, Matrix<double>>(0.1, 0, 0);

            this._RollEstimator = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(this._RollKernel);
            this._PitchEstimator = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(this._PitchKernel);
            this._YawEstimator = new Krls<double, RadialBasisKernel<double, Matrix<double>>>(this._YawKernel);

            Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(rollModelFile, ref this._RollEstimator);
            Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(pitchModelFile, ref this._PitchEstimator);
            Krls<double, RadialBasisKernel<double, Matrix<double>>>.Deserialize(yawModelFile, ref this._YawEstimator);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a head pose estimated from face parts locations.
        /// </summary>
        /// <param name="landmark">The dictionary of face parts locations (eyes, nose, etc).</param>
        /// <returns>A head pose estimated from face parts locations.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="landmark"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="landmark"/> does not have 68 points.</exception>
        protected override HeadPose RawPredict(IDictionary<FacePart, IEnumerable<FacePoint>> landmark)
        {
            if (landmark == null)
                throw new ArgumentNullException(nameof(landmark));

            var facePoints = new List<FacePoint>();
            foreach (var value in landmark.Values) facePoints.AddRange(value);
            facePoints = facePoints.Distinct().ToList();
            facePoints.Sort((point1, point2) => point1.Index - point2.Index);

            if (facePoints.Count != 68)
                throw new ArgumentException($"{nameof(landmark)} does not have 68 points.", nameof(landmark));

            using (var rollMatrix = GetRollMatrix(facePoints))
            using (var pitchMatrix = GetPitchMatrix(facePoints))
            using (var yawMatrix = GetYawMatrix(facePoints))
            {
                var roll = this._RollEstimator.Operator(rollMatrix);
                var pitch = this._PitchEstimator.Operator(pitchMatrix);
                var yaw = this._YawEstimator.Operator(yawMatrix);
                return new HeadPose(roll, pitch, yaw);
            }
        }

        /// <summary>
        /// Releases all unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            this._RollEstimator?.Dispose();
            this._RollKernel?.Dispose();
            this._PitchEstimator?.Dispose();
            this._PitchKernel?.Dispose();
            this._YawEstimator?.Dispose();
            this._YawKernel?.Dispose();
        }

        #endregion

        #region Helpers

        private static Matrix<double> GetPitchMatrix(IList<FacePoint> points)
        {
            // Calc angle from 33 to each point except for 33
            var vector = new List<double>();
            var p1 = points[33];
            for (var c = 0; c < 68; c++)
            {
                if (c == 33)
                    continue;

                var p2 = points[c];
                var distance = p2.Point.Y - p1.Point.Y;
                vector.Add(distance);
            }

            NormalizeVector(vector);

            return new Matrix<double>(vector.ToArray(), vector.Count, 1);
        }

        private static Matrix<double> GetRollMatrix(IList<FacePoint> points)
        {
            // Calc angle from 33 to each point except for 33
            var vector = new List<double>();
            var p1 = points[33];
            for (var c = 0; c < 68; c++)
            {
                if (c == 33)
                    continue;

                var p2 = points[c];
                var distance = Math.Atan((p2.Point.X - p1.Point.X) / (double)(p2.Point.Y - p1.Point.Y));
                //var distance = Math.Sqrt(Math.Pow(p2.X - p1.X,2) + Math.Pow(p2.Y - p1.Y,2));
                vector.Add(distance);
            }

            // Need not to use Normalization
            //NormalizeVector(vector);

            return new Matrix<double>(vector.ToArray(), vector.Count, 1);
        }

        private static Matrix<double> GetYawMatrix(IList<FacePoint> points)
        {
            // Calc angle from 33 to each point except for 33
            var vector = new List<double>();
            var p1 = points[33];
            for (var c = 0; c < 68; c++)
            {
                if (c == 33)
                    continue;

                var p2 = points[c];
                var distance = p2.Point.X - p1.Point.X;
                vector.Add(distance);
            }

            NormalizeVector(vector);

            return new Matrix<double>(vector.ToArray(), vector.Count, 1);
        }

        private static void NormalizeVector(IList<double> vector)
        {
            // z-score normalization
            var count = vector.Count;
            var mean = vector.Average();
            var sum2 = vector.Select(a => a * a).Sum();
            var variance = sum2 / count - mean * mean;
            var std = Math.Sqrt(variance);
            for (var index = 0; index < vector.Count; index++)
                vector[index] = (vector[index] - mean) / std;
        }

        #endregion

    }

}