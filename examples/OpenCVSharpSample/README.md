# OpenCVSharp Sample

This sample requires to use CUDA.

## How to use?

## 1. Preparation

This sample requires model files.

## 2. Build

1. Open command prompt and change to &lt;OpenCVSharpSample_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````

## 3. Run

1. Open command prompt and change to &lt;OpenCVSharpSample_dir&gt;
1. Type the following sample command
````
$ dotnet run -c Release -- "-m=models"
Distance: 0.350636343769049 for obama-240p.jpg
Distance: 0.361864784857372 for obama-480p.jpg
Distance: 0.37696759951267 for obama-720p.jpg
Distance: 0.373414821495045 for obama-1080p.jpg
````

## 4. Parameters

This program support the following argument and option.

### Argument

|Argument|Description|
|:---|:---|
|-m\|--model|Directory path includes model files|