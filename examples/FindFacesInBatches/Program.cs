/*
 * This sample program is ported by C# from https://github.com/ageitgey/face_recognition/blob/master/examples/find_faces_in_batches.py.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FaceRecognitionDotNet;
using Microsoft.Extensions.CommandLineUtils;
using OpenCvSharp;

namespace FindFacesInBatches
{

    internal class Program
    {

        #region Fields

        private static FaceRecognition _FaceRecognition;

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.Name = nameof(FindFacesInBatches);
            app.HelpOption("-h|--help");
            var batchSizeOption = app.Option("-b|--batchsize", "Number of batch size", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var directory = Path.GetFullPath("Models");
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"Please check whether model directory '{directory}' exists");
                    return -1;
                }

                if (!int.TryParse(batchSizeOption.Value(), out var batchSize) || batchSize < 0)
                {
                    Console.WriteLine($"--batchSize '{batchSizeOption.Value()}' must be positive integer");
                    return -1;
                }

                _FaceRecognition = FaceRecognition.Create(directory);

                var frames = new List<Image>();
                var frameCount = 0;

                using (var capture = new VideoCapture("short_hamilton_clip.mp4"))
                    while (capture.IsOpened())
                    {
                        // Grab a single frame of video
                        using (var frame = new Mat())
                        {
                            var ret = capture.Read(frame);

                            // Bail out when the video file ends
                            if (!ret || !frame.IsContinuous())
                                break;

                            // Convert the image from BGR color (which OpenCV uses) to RGB color (which face_recognition uses)
                            using (var tmp = frame.CvtColor(ColorConversionCodes.BGR2RGB))
                            {
                                var array = new byte[tmp.Width * tmp.Height * tmp.ElemSize()];
                                Marshal.Copy(tmp.Data, array, 0, array.Length);

                                var image = FaceRecognition.LoadImage(array, tmp.Rows, tmp.Cols, tmp.Width * tmp.ElemSize(), Mode.Rgb);

                                // Save each frame of the video to a list
                                frameCount += 1;
                                frames.Add(image);
                            }
                        }

                        if (frames.Count == batchSize)
                        {
                            var batchOfFaceLocations = _FaceRecognition.BatchFaceLocations(frames, 0, batchSize).ToArray();

                            // Now let's list all the faces we found in all 128 frames
                            for (var frameNumberInBatch = 0; frameNumberInBatch < batchOfFaceLocations.Length; frameNumberInBatch++)
                            {
                                var faceLocations = batchOfFaceLocations[frameNumberInBatch];
                                var numberOfFacesInFrame = faceLocations.Length;

                                var frameNumber = frameCount - batchSize + frameNumberInBatch;
                                Console.WriteLine($"I found {numberOfFacesInFrame} face(s) in frame #{frameNumber}.");

                                foreach (var faceLocation in faceLocations)
                                {
                                    // Print the location of each face in this frame
                                    var top = faceLocation.Top;
                                    var right = faceLocation.Right;
                                    var bottom = faceLocation.Bottom;
                                    var left = faceLocation.Left;
                                    Console.WriteLine($" - A face is located at pixel location Top: {top}, Left: {left}, Bottom: {bottom}, Right: {right}");
                                }
                            }

                            // Clear the frames array to start the next batch
                            foreach (var frame in frames)
                                frame.Dispose();
                            frames.Clear();
                        }
                    }

                return 0;
            });

            app.Execute(args);
        }

        #endregion

    }

}
