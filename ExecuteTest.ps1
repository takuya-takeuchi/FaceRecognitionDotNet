$CodecovVersion = "1.12.4"

# check Codecov Token
$token = $env:CODECOV_TOKEN
if ([string]::IsNullOrEmpty($token))
{
    Write-Host "Environmental Value 'CODECOV_TOKEN' is not defined." -ForegroundColor Red
    exit -1
}

Write-Host "Environmental Value 'CODECOV_TOKEN' is ${token}." -ForegroundColor Green

# install coverlet
dotnet tool install --global coverlet.console > $null
dotnet add test\FaceRecognitionDotNet.Tests\FaceRecognitionDotNet.Tests.csproj package coverlet.msbuild > $null
dotnet add test\FaceRecognitionDotNet.Tests\FaceRecognitionDotNet.Tests.csproj package coverlet.collector > $null
# install codecov but it is not used from test project
dotnet add test\FaceRecognitionDotNet.Tests\FaceRecognitionDotNet.Tests.csproj package Codecov --version $CodecovVersion > $null

Write-Host "Start Test and collect Coverage." -ForegroundColor Green
# https://github.com/tonerdo/coverlet/blob/master/Documentation/MSBuildIntegration.md
dotnet test test\FaceRecognitionDotNet.Tests\FaceRecognitionDotNet.Tests.csproj -v=normal `
            /p:CollectCoverage=true `
            /p:CoverletOutputFormat=opencover `
            /p:Exclude="[DlibDotNet]*"
Write-Host "End Test and collect Coverage." -ForegroundColor Green

$path = (dotnet nuget locals global-packages --list).Replace('info : global-packages: ', '').Trim()
if ($path)
{
    $path = (dotnet nuget locals global-packages --list).Replace('global-packages: ', '').Trim()
}
$path =  Join-Path $path "codecov" | `
         Join-Path -ChildPath $CodecovVersion

if ($global:IsWindows)
{
    $path = Join-Path $path "tools\codecov.exe"
}
elseif ($global:IsLinux)
{
    $path = Join-Path $path "tools/linux-x64/codecov"
}
elseif ($global:IsMacOS)
{
    $path = Join-Path $path "tools/osx-x64/codecov"
}

& $path -f test\FaceRecognitionDotNet.Tests\coverage.opencover.xml -t $token