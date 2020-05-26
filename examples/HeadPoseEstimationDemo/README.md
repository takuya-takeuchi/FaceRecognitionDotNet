# Head Pose Estimation Demo

This example demonstrate estimation of human head pose.

## How to use?

## 1. Preparation

This sample requires model files and webcam.

## 2. Build

1. Open command prompt and change to &lt;HeadPoseEstimationDemo_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````

## 3. Run

1. Open command prompt and change to &lt;HeadPoseEstimationDemo_dir&gt;
1. Type the following sample command
````
$ dotnet run -c Release
````