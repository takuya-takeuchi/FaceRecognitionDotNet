set PROJECT=test\FaceRecognitionDotNet.Tests\FaceRecognitionDotNet.Tests
set PACKAGECPU=FaceRecognitionDotNet
set PACKAGECUDA=FaceRecognitionDotNet.CUDA102
set NUGETDIR=%cd%\nuget

dotnet remove %PROJECT%.csproj package %PACKAGECPU%
dotnet remove %PROJECT%.csproj package %PACKAGECUDA%
dotnet add %PROJECT%.csproj package %PACKAGECUDA% --source "%NUGETDIR%"

dotnet test test\FaceRecognitionDotNet.Tests\FaceRecognitionDotNet.Tests.csproj -c Release