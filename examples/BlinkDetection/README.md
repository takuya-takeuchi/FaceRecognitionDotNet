# Blink Detection

This example demonstrate detection of eye blink.
This sample program is ported by C# from https://github.com/ageitgey/face_recognition/blob/master/examples/blink_detection.py

## How to use?

## 1. Preparation

This sample requires model files and webcam.

## 2. Build

1. Open command prompt and change to &lt;BlinkDetection_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````

## 3. Run

1. Open command prompt and change to &lt;BlinkDetection_dir&gt;
1. Type the following sample command
````
$ dotnet run -c Release
````
1. CameraImage is shown after camera detect face
1. Blue rectangle is drawn if syes are detected
1. Program will be paused if eyes are closing on 5 sec
1. User can restart program by push space button if program is paused