# Face Search

Compare face search algorithm, Linear Searcg (kNN) and Annoy (Approximate Nearest Neighbor Oh Yeah).
Approximate Nearest Neighbor may not reproduce exact solution but not it is very fast for large mount dataset.

## How to use?

## 1. Preparation

This sample requires test images and model files.

## 2. Build

1. Open command prompt and change to &lt;FaceSearch_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet/DlibDotNet.csproj
$ dotnet remove reference ../../src/FaceRecognitionDotNet/FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````

## 3. Run

1. Open command prompt and change to &lt;FaceSearch_dir&gt;
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

Basically, Annoy search would be fast if number of face encodings are enough large.
We tested CelebA dataset.
In this case, Annoy is faster than 20x.

````cmd
$ dotnet run -c Release -- --model models --directory D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba --topK 5
Start: Get face encodings
D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\000003.jpg does not have any face
...
D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\202565.jpg does not have any face
Total: 196694 faces
Finish: Get face encodings [7754376 ms]

Start: Annoy Search
Start: Add encoding
Finish: Add encoding [333 ms]
Start: Build index
Finish: Build index [23419 ms]
Start: Query: D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\000001.jpg
Finish: Query [9 ms]
1: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\000001.jpg: 0]
127791: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\131731.jpg: 0.244089941516112]
93452: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\096324.jpg: 0.435246313240628]
51552: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\053184.jpg: 0.467181515554196]
66085: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\068154.jpg: 0.471912339319797]
Finish: Annoy Search

Start: Linear Search
Start: Query: D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\000001.jpg
Finish: Query [216 ms]
1: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\000001.jpg: 0]
127791: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\131731.jpg: 0.244089941516112]
105128: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\108341.jpg: 0.403897508108627]
93452: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\096324.jpg: 0.435246313240628]
82155: [D:\Works\Dataset\CelebA\CelebA\Img\img_align_celeba\084705.jpg: 0.462124549181885]
Finish: Linear Search
````