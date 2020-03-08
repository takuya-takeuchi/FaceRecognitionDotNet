# ![Alt text](nuget/face48.png "FaceRecognitionDotNet") FaceRecognitionDotNet [![GitHub license](https://img.shields.io/github/license/mashape/apistatus.svg)]() [![codecov](https://codecov.io/gh/takuya-takeuchi/FaceRecognitionDotNet/branch/master/graph/badge.svg)](https://codecov.io/gh/takuya-takeuchi/FaceRecognitionDotNet)

The world's simplest facial recognition api for .NET  
This repository is porting https://github.com/ageitgey/face_recognition by C#.

This package supports cross platform, Windows, Linux and MacOSX!!

|Package|OS|x86|x64|ARM|ARM64|Nuget|
|---|---|---|---|---|---|---|
|FaceRecognitionDotNet (CPU)|Windows|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet)|
||Linux|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet)|
||OSX|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet)|
|FaceRecognitionDotNet for CUDA 9.2|Windows|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.CUDA92.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.CUDA92)|
||Linux|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.CUDA92.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.CUDA92)|
||OSX|-|-|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.CUDA92.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.CUDA92)|
|FaceRecognitionDotNet for CUDA 10.0|Windows|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.CUDA100.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.CUDA100)|
||Linux|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.CUDA100.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.CUDA100)|
||OSX|-|-|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.CUDA100.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.CUDA100)|
|FaceRecognitionDotNet for CUDA 10.1|Windows|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.CUDA101.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.CUDA101)|
||Linux|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.CUDA101.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.CUDA101)|
||OSX|-|-|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.CUDA101.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.CUDA101)|
|FaceRecognitionDotNet for Intel MKL|Windows|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.MKL.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.MKL)|
||Linux|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.MKL.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.MKL)|
||OSX|-|✓|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet.MKL.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet.MKL)|
|FaceRecognitionDotNet for ARM|Windows|-|-|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet-ARM.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet-ARM)|
||Linux|-|-|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet-ARM.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet-ARM)|
||OSX|-|-|-|-|[![NuGet version](https://img.shields.io/nuget/v/FaceRecognitionDotNet-ARM.svg)](https://www.nuget.org/packages/FaceRecognitionDotNet-ARM)|

##### :warning: FaceRecognitionDotNet for ARM is not tested yet

## Support API

|face_recognition API|Corresponding API|Note|
|----|----|:----|
|batch_face_locations|BatchFaceLocations||
|compare_faces|CompareFaces||
|face_distance|FaceDistance||
|face_encodings|FaceEncodings||
|face_landmarks|FaceLandmarks|And support **Helen dataset** :warning:|
|face_locations|FaceLocations||
|load_image_file|LoadImageFile||
|-|CropFaces|Crop image with specified locations|
|-|LoadImage|From memory data|
|-|PredictAge|Use **Adience Benchmark Of Unfiltered Faces For Gender And Age Classification dataset** :warning:|
|-|PredictGender|Use **UTKFace dataset** :warning:|
|-|PredictProbabilityAge|Use **Adience Benchmark Of Unfiltered Faces For Gender And Age Classification dataset** :warning:|
|-|PredictProbabilityGender|Use **UTKFace dataset** :warning:|

##### :warning: Warning

You must train dataset by yourself.
I will **NOT** provide pretrained model file due to avoiding license issue.
You can check the following examples to train dataset.

* tools/AgeTraining
* tools/GenderTraining
* tools/HelenTraining

## Demo

#### Face Recognition

<img src="images/1.png" width="480"/>

<img src="images/2.png" width="480"/>

#### Face Landmark

<img src="images/3.jpg" width="240"/>

#### Age and Gender Classification

<img src="examples/CustomClassificationDemo/images/result.png" width="240"/>

## Dependencies Libraries and Products

#### [face_recognition](https://github.com/ageitgey/face_recognition/)

> **License:** The MIT License
>
> **Author:** Adam Geitgey
> 
> **Principal Use:** The world's simplest facial recognition api for Python and the command line. Main goal of FaceRecognitionDotNet is what ports face_recognition by C#.

#### [face_recognition_models](https://github.com/ageitgey/face_recognition_models/)

> **License:** Creative Commons Zero v1.0 Universal License
>
> **Author:** Adam Geitgey
> 
> **Principal Use:** Trained models for the face_recognition python library

#### [dlib](http://dlib.net/)

> **License:** Boost Software License
>
> **Author:** Davis E. King
> 
> **Principal Use:** A toolkit for making real world machine learning and data analysis applications in C++.

#### [DlibDotNet](https://github.com/takuya-takeuchi/DlibDotNet/)

> **License:** The MIT License
>
> **Author:** Takuya Takeuchi
> 
> **Principal Use:** Use dlib interface via .NET. This library is developed by this owner.

#### [OpenCVSharp](https://github.com/shimat/opencvsharp/)

> **License:** The BSD 3-Clause License
>
> **Author:** shimat
> 
> **Principal Use:** Loading image data by opencv wrapper for example

