dotnet build -c Release ..\src\FaceRecognitionDotNet

nuget pack FaceRecognitionDotNet-CPU.nuspec 
nuget pack FaceRecognitionDotNet-CUDA.nuspec