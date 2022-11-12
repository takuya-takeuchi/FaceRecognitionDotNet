# Face Search

Compare face search algorithm, Linear Searcg (kNN) and Annoy.

## How to use?

## 1. Preparation

This sample requires test images and model files.

## 2. Build

1. Open command prompt and change to &lt;Encoding_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet/DlibDotNet.csproj
$ dotnet remove reference ../../src/FaceRecognitionDotNet/FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````

## 3. Run

1. Open command prompt and change to &lt;Encoding_dir&gt;
1. Type the following sample command

````
$ dotnet run -c Release -- --model models --directory images --topK 5
Start: Get face encodings
test\000003.jpg does not have any face
test\000004.jpg does not have any face
test\000036.jpg does not have any face
test\000067.jpg does not have any face
Total: 96 faces
Finish: Get face encodings

Start: Annoy Search
Start: Add encoding
Finish: Add encoding [2 ms]
Start: Build index
Finish: Build index [2 ms]
Start: Query: test\000001.jpg
Finish: Query [9 ms]
1: [test\000001.jpg: 0]
95: [test\000099.jpg: 0.626607159241616]
96: [test\000100.jpg: 0.699352741710065]
42: [test\000045.jpg: 0.699802253518985]
70: [test\000074.jpg: 0.701357934078195]
Finish: Annoy Search

Start: Linear Search
Start: Query: test\000001.jpg
Finish: Query [2 ms]
1: [test\000001.jpg: 0]
95: [test\000099.jpg: 0.626607159241616]
96: [test\000100.jpg: 0.699352741710065]
42: [test\000045.jpg: 0.699802253518985]
70: [test\000074.jpg: 0.701357934078195]
Finish: Linear Search
````

Basically, Annoy search would be fast if face encodings are enough large.
We tested CelebA dataset.
In this case, 