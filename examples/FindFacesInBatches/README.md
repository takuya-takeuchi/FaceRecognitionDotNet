# Find Faces In Batches

This example demonstrates detecting faces from video file.
This program is ported by C# from https://github.com/ageitgey/face_recognition/blob/master/examples/find_faces_in_batches.py.

How to use?

## How to use?

## 1. Preparation

This sample requires model files.  

***NOTE***  
The directory that contains model files should be in &lt;FindFacesInBatches_dir&gt;/Models.

## 2. Build

1. Open command prompt and change to &lt;FindFacesInBatches_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````
2. Copy ***DlibDotNet.Native.dll*** and ***DlibDotNet.Dnn.dll*** to output directory; &lt;FindFacesInBatches_dir&gt;\bin\Release\netcoreapp2.0.
   * If you use FaceRecognitionDotNet with CUDA, you must copy also cuda libraries.

## 3. Run

1. Open command prompt and change to &lt;FindFacesInBatches_dir&gt;
1. Type the following sample command
````
$ pwsh Run.ps1 32
I found 0 face(s) in frame #0.
I found 0 face(s) in frame #1.
I found 0 face(s) in frame #2.
I found 0 face(s) in frame #3.
I found 0 face(s) in frame #4.
I found 0 face(s) in frame #5.
I found 0 face(s) in frame #6.
...
...
...
I found 3 face(s) in frame #250.
 - A face is located at pixel location Top: 50, Left: 312, Bottom: 145, Right: 407
 - A face is located at pixel location Top: 100, Left: 380, Bottom: 179, Right: 459
 - A face is located at pixel location Top: 92, Left: 156, Bottom: 171, Right: 235
I found 3 face(s) in frame #251.
 - A face is located at pixel location Top: 50, Left: 312, Bottom: 145, Right: 407
 - A face is located at pixel location Top: 100, Left: 380, Bottom: 179, Right: 459
 - A face is located at pixel location Top: 92, Left: 164, Bottom: 171, Right: 243
I found 3 face(s) in frame #252.
 - A face is located at pixel location Top: 50, Left: 312, Bottom: 145, Right: 407
 - A face is located at pixel location Top: 92, Left: 156, Bottom: 171, Right: 235
 - A face is located at pixel location Top: 100, Left: 380, Bottom: 179, Right: 459
I found 3 face(s) in frame #253.
 - A face is located at pixel location Top: 50, Left: 312, Bottom: 145, Right: 407
 - A face is located at pixel location Top: 92, Left: 156, Bottom: 171, Right: 235
 - A face is located at pixel location Top: 100, Left: 380, Bottom: 179, Right: 459
I found 3 face(s) in frame #254.
 - A face is located at pixel location Top: 50, Left: 312, Bottom: 145, Right: 407
 - A face is located at pixel location Top: 92, Left: 156, Bottom: 171, Right: 235
 - A face is located at pixel location Top: 100, Left: 380, Bottom: 179, Right: 459
I found 3 face(s) in frame #255.
 - A face is located at pixel location Top: 50, Left: 312, Bottom: 145, Right: 407
 - A face is located at pixel location Top: 92, Left: 164, Bottom: 171, Right: 243
 - A face is located at pixel location Top: 100, Left: 380, Bottom: 179, Right: 459
````

## 4. Parameters

This program support the following argument and option.

### Argument

|Argument|Description|
|:---|:---|
|-b\|--batchsize|Number of batch size|