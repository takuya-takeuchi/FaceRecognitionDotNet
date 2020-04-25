# Custom Classification Demo

This example for age and gender classification.

## How to use?

## 1. Preparation

### model files

The model files are used for FaceRecognitonDotNet.

And you must generate the following model files and copy to &lt;CustomClassificationDemo_dir/models&gt; directory.

* adience-age-network.dat
  * Generate by **examples\AgeTraining**
* utkface-gender-network.dat
  * Generate by **examples\GenderTraining**

## 2. Build

1. Open command prompt and change to &lt;CustomClassificationDemo_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````
3. Copy ***DlibDotNetNative.dll***, ***DlibDotNetNativeDnn.dll***, ***DlibDotNetNativeDnnAgeClassification.dll*** and ***DlibDotNetNativeDnnGenderClassification.dll***  to output directory; &lt;CustomClassificationDemo_dir&gt;\bin\Release\netcoreapp2.0.
   * if you use FaceRecognitionDotNet with CUDA, you must copy also cuda libraries.

## 3. Run

1. Open command prompt and change to &lt;CustomClassificationDemo_dir&gt;

````
$ dotnet run -c Release
````

![Result](images/result.png "Result")