dotnet build -c Release ..\src\FaceRecognitionDotNet

nuget pack FaceRecognitionDotNet-CPU.nuspec
nuget pack FaceRecognitionDotNet.CUDA-92.nuspec
nuget pack FaceRecognitionDotNet.CUDA-100.nuspec
nuget pack FaceRecognitionDotNet-ARM.nuspec