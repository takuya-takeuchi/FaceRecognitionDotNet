# Face Detection

This example demonstrates detecting faces from images in directory.
This program is ported by C# from https://github.com/ageitgey/face_recognition/face_recognition/face_detection_cli.py.

How to use?

## How to use?

## 1. Preparation

This sample requires test images and model files.  

***NOTE***  
The directory that contains model files should be in &lt;FaceDetection_dir&gt;.

## 2. Build

1. Open command prompt and change to &lt;FaceDetection_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````
2. Copy ***DlibDotNet.dll***, ***DlibDotNet.Native.dll*** and ***DlibDotNet.Dnn.dll*** to output directory; &lt;FaceDetection_dir&gt;\bin\Release\netcoreapp2.0.
   * if you use FaceRecognitionDotNet with CUDA, you must copy also cuda libraries.

## 3. Run

1. Open command prompt and change to &lt;FaceDetection_dir&gt;
1. Type the following sample command
````
$ dotnet run -c Release -- "-d=."
.\512px-President_Barack_Obama.jpg,79,314,203,189
.\Lenna.png,228,377,377,228
````

## 4. Parameters

This program support the following argument and option.

### Argument

|Argument|Description|
|:---|:---|
|-d\|--directory|The directory path which includes image files|
|-c\|--cpus|The number of CPU cores to use in parallel. -1 means "use all in system"|
|-m\|--model|Which face detection model to use. Options are "hog" or "cnn".|