# Benchmark

This example measures performance of calculating for face encodings.  
This sample program is ported by C# from https://github.com/ageitgey/face_recognition/blob/master/examples/benchmark.py.

## How to use?

## 1. Preparation

This sample requires test image and model files.

## 2. Build

1. Open command prompt and change to &lt;Benchmark_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````
2. Copy ***DlibDotNet.dll***, ***DlibDotNet.Native.dll*** and ***DlibDotNet.Dnn.dll*** to output directory; &lt;Benchmark_dir&gt;\bin\Release\netcoreapp2.0.
   * if you use FaceRecognitionDotNet with CUDA, you must copy also cuda libraries.

## 3. Run

1. Open command prompt and change to &lt;Benchmark_dir&gt;
1. Type the following sample command
````
$ dotnet run -c Release -- "-m=models"
Benchmarks

Timings at 240p:
 - Face locations: 0.0268s (37.31 fps)
 - Face landmarks: 0.0014s (714.29 fps)
 - Encode face (inc. landmarks): 0.0210s (47.62 fps)
 - End-to-end: 0.0484s (20.66 fps)

Timings at 480p:
 - Face locations: 0.1068s (9.36 fps)
 - Face landmarks: 0.0014s (714.29 fps)
 - Encode face (inc. landmarks): 0.0202s (49.50 fps)
 - End-to-end: 0.1308s (7.65 fps)

Timings at 720p:
 - Face locations: 0.2416s (4.14 fps)
 - Face landmarks: 0.0014s (714.29 fps)
 - Encode face (inc. landmarks): 0.0206s (48.54 fps)
 - End-to-end: 0.2700s (3.70 fps)

Timings at 1080p:
 - Face locations: 0.5430s (1.84 fps)
 - Face landmarks: 0.0016s (625.00 fps)
 - Encode face (inc. landmarks): 0.0206s (48.54 fps)
 - End-to-end: 0.5774s (1.73 fps)
````

## 4. Parameters

This program support the following argument and option.

### Argument

|Argument|Description|
|:---|:---|
|-m\|--model|Directory path includes model files|
|-c\|--cnn|Use Cnn|

## 5. Other

### Why is Encode face too slow?

The reason ***face_recognition*** can achieve high performance is using ***Intel Math Kernel Library***.  
If you can use Intel Math Kernel Library, you can build ***DlibDotNet.Native.Dnn*** by linking Intel Math Kernel Library.