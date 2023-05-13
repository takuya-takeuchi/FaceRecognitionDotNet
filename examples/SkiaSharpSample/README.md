# SkiaSharpSharp Sample

This sample requires to use CUDA.

## How to use?

## 1. Preparation

This sample requires model files.

## 2. Build

1. Open command prompt and change to &lt;SkiaSharpSharpSample_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.Extensions.Skia.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet add package FaceRecognitionDotNet.Extensions.Skia
$ dotnet build -c Release
````

## 3. Run

1. Open command prompt and change to &lt;SkiaSharpSharpSample_dir&gt;
1. Type the following sample command
````
$ dotnet run -c Release -- "-m=models"
Distance: 0.342289226629347 for obama-240p.jpg
Distance: 0.346749102653167 for obama-480p.jpg
Distance: 0.369195738248585 for obama-720p.jpg
Distance: 0.361052041208253 for obama-1080p.jpg
````

## 4. Parameters

This program support the following argument and option.

### Argument

|Argument|Description|
|:---|:---|
|-m\|--model|Directory path includes model files|