# Face Encoding Performance

This example measures performance of calculating for face encodings.

## How to use?

## 1. Preparation

This sample requires test image and model files.

## 2. Build

1. Open command prompt and change to &lt;FaceEncodingPerformance_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````
2. Copy ***DlibDotNet.dll***, ***DlibDotNet.Native.dll*** and ***DlibDotNet.Dnn.dll*** to output directory; &lt;FaceEncodingPerformance_dir&gt;\bin\Release\netcoreapp2.0.
   * if you use FaceRecognitionDotNet with CUDA, you must copy also cuda libraries.

## 3. Run

1. Open command prompt and change to &lt;FaceEncodingPerformance_dir&gt;
1. Type the following sample command
````
$ dotnet run -c Release -- "-l=100" "-f=lenna.jpg" "-m=D:\Works\Models"
Total: 3656 [ms], Average: 36 [ms]
````