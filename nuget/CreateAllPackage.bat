dotnet restore ..\src\FaceRecognitionDotNet
dotnet build -c Release ..\src\FaceRecognitionDotNet

pwsh CreatePackage.ps1 CPU
pwsh CreatePackage.ps1 CUDA92
pwsh CreatePackage.ps1 CUDA100
pwsh CreatePackage.ps1 CUDA101
pwsh CreatePackage.ps1 CUDA102
pwsh CreatePackage.ps1 CUDA110
pwsh CreatePackage.ps1 CUDA111
pwsh CreatePackage.ps1 MKL