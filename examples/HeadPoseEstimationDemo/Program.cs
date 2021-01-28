using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FaceRecognitionDotNet;
using FaceRecognitionDotNet.Extensions;
using Microsoft.Extensions.CommandLineUtils;
using OpenCvSharp;

namespace HeadPoseEstimationDemo
{

    internal class Program
    {

        #region Methods

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(HeadPoseEstimationDemo);
            app.Description = "The program for blink detection demo";
            app.HelpOption("-h|--help");

            app.OnExecute(() =>
            {
                using (var fr = FaceRecognition.Create("models"))
                using (var videoCapture = new VideoCapture(0))
                {
                    var rollFile = Path.Combine("models", "300w-lp-roll-krls_0.001_0.1.dat");
                    var pitchFile = Path.Combine("models", "300w-lp-pitch-krls_0.001_0.1.dat");
                    var yawFile = Path.Combine("models", "300w-lp-yaw-krls_0.001_0.1.dat");
                    fr.CustomHeadPoseEstimator = new SimpleHeadPoseEstimator(rollFile, pitchFile, yawFile);

                    using (var smallFrame = new Mat())
                        while (true)
                        {
                            using (var frame = videoCapture.RetrieveMat())
                            {
                                //Cv2.Resize(frame, smallFrame, Size.Zero, 0.25, 0.25);
                                Cv2.Resize(frame, smallFrame, Size.Zero, 1, 1);

                                var cols = smallFrame.Cols;
                                var rows = smallFrame.Rows;
                                var elems = smallFrame.ElemSize();


                                // get the correct face landmarks
                                var bytes = new byte[rows * cols * elems];
                                Marshal.Copy(smallFrame.Data, bytes, 0, bytes.Length);
                                using (var rgbSmallFrame = FaceRecognition.LoadImage(bytes, rows, cols, cols * elems, Mode.Rgb))
                                {
                                    var faceLandmarksList = fr.FaceLandmark(rgbSmallFrame).ToArray();

                                    // get eyes
                                    foreach (var faceLandmark in faceLandmarksList)
                                    {
                                        var pose = fr.PredictHeadPose(faceLandmark);
                                        DrawAxis(smallFrame, faceLandmark, pose.Roll, pose.Pitch, pose.Yaw, 120);
                                    }

                                    Cv2.ImShow("Video", smallFrame);
                                    Cv2.WaitKey(1);
                                }
                            }
                        }
                }

                return 0;
            });

            app.Execute(args);
        }

        #region Helpers

        private static void DrawAxis(Mat mat, IDictionary<FacePart, IEnumerable<FacePoint>> landmark, double roll, double pitch, double yaw, uint size)
        {
            // https://github.com/natanielruiz/deep-head-pose/blob/master/code/utils.py
            // plot_pose_cube
            pitch = pitch * Math.PI / 180;
            yaw = -(yaw * Math.PI / 180);
            roll = roll * Math.PI / 180;

            var facePoints = new List<FacePoint>();
            foreach (var value in landmark.Values) facePoints.AddRange(value);
            facePoints = facePoints.Distinct().ToList();

            var center = facePoints.Find(point => point.Index == 33);
            var tdx = center.Point.X;
            var tdy = center.Point.Y;

            // X-Axis pointing to right. drawn in red
            var x1 = size * (Math.Cos(yaw) * Math.Cos(roll)) + tdx;
            var y1 = size * (Math.Cos(pitch) * Math.Sin(roll) + Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw)) + tdy;

            // Y-Axis | drawn in green
            // v
            var x2 = size * (-Math.Cos(yaw) * Math.Sin(roll)) + tdx;
            var y2 = size * (Math.Cos(pitch) * Math.Cos(roll) - Math.Sin(pitch) * Math.Sin(yaw) * Math.Sin(roll)) + tdy;

            // Z-Axis (out of the screen) drawn in blue
            var x3 = size * (Math.Sin(yaw)) + tdx;
            var y3 = size * (-Math.Cos(yaw) * Math.Sin(pitch)) + tdy;

            Cv2.Line(mat, new OpenCvSharp.Point(tdx, tdy), new OpenCvSharp.Point(x1, y1), new Scalar(0, 0, 255), 3);
            Cv2.Line(mat, new OpenCvSharp.Point(tdx, tdy), new OpenCvSharp.Point(x2, y2), new Scalar(0, 255, 0), 3);
            Cv2.Line(mat, new OpenCvSharp.Point(tdx, tdy), new OpenCvSharp.Point(x3, y3), new Scalar(255, 0, 0), 3);
        }

        #endregion

        #endregion

    }

}
