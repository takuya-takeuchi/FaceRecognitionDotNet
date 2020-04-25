# Rgb vs Bgr

This example demonstrate about result of RGB image and BGR image. 

`FaceRecognitionDotNet.LoadImage` can accept raw bitmap data but developer must take care of colorspace.
For example, 24bit colorspace of windows bitmap file data has BGR colorspace. 
You can see this if using `System.Drawing.Image.FromFile`, `System.Drawing.Bitmap.LockBits` and checking memory data from `System.Drawing.Imaging.BitmapData.Scan0`. 

However, `FaceRecognitionDotNet.LoadImageFile` returns `FaceRecognitionDotNet.Image` contains RGB colorspace. 
It is specification of `face_recognition`. To be exact, some of `dlib` interface expects RGB colorspace. 

Therefore, some function returns different results if colorspace of source image are different. 
Especially, `FaceRecognitionDotNet.FaceRecognition.FaceEncodings` returns non-negligible diffrence. 
Even though creating `FaceRecognitionDotNet.FaceEncoding` from same face location and image but different colorspace, these face encodings have unacceptable distance. 

In conclusion, developer should shall whether raw bitmap data is RGB colorspace.

## How to use?

## 1. Preparation

This sample requires model files.

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
FaceLocations (Hog)
         File [LTRB]: 218, 219, 373, 374
          RGB [LTRB]: 218, 219, 373, 374
          BGR [LTRB]: 218, 219, 373, 374

FaceEncodings by File Location (Hog)
         vs  RGB [Distance]: 0
         vs  BGR [Distance]: 0.202584867566029
FaceEncodings by RGB Location (Hog)
         vs File [Distance]: 0
         vs  BGR [Distance]: 0.202584867566029
FaceEncodings by BGR Location (Hog)
         vs File [Distance]: 0.202584867566029
         vs  RGB [Distance]: 0.202584867566029

FaceLocations (Cnn)
         File [LTRB]: 198, 215, 340, 356
          RGB [LTRB]: 198, 215, 340, 356
          BGR [LTRB]: 212, 229, 354, 371

FaceEncodings by File Location (Cnn)
         vs  RGB [Distance]: 0
         vs  BGR [Distance]: 0.182024898919954
FaceEncodings by RGB Location (Cnn)
         vs File [Distance]: 0
         vs  BGR [Distance]: 0.182024898919954
FaceEncodings by BGR Location (Cnn)
         vs File [Distance]: 0.182024898919954
         vs  RGB [Distance]: 0.182024898919954
````