$targets = @(
   "",
   ".CUDA111",
   ".MKL"
)

$ScriptPath = $PSScriptRoot
Set-Location $ScriptPath

foreach ($target in $targets)
{
    dotnet remove reference ..\..\src\DlibDotNet\src\DlibDotNet\DlibDotNet.csproj > $null
    dotnet remove reference ..\..\src\FaceRecognitionDotNet\FaceRecognitionDotNet.csproj > $null
    dotnet add package "FaceRecognitionDotNet${target}"
    $image = Join-Path $ScriptPath "obama-240p.jpg"
    dotnet run -c Release -- --model ${env:FaceRecognitionDotNetModelDir} --image "${image}" > "FaceRecognitionDotNet${target}.log"
    git checkout .
}