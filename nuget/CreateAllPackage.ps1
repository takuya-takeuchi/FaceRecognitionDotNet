$targets = @(
   "CPU",
   "CUDA92",
   "CUDA100",
   "CUDA101",
   "CUDA102",
   "CUDA110",
   "CUDA111",
   "CUDA112",
   "MKL"
)

$ScriptPath = $PSScriptRoot
$FaceRecognitionDotNetRoot = Split-Path $ScriptPath -Parent

$source = Join-Path $FaceRecognitionDotNetRoot src | `
          Join-Path -ChildPath FaceRecognitionDotNet
dotnet restore ${source}
dotnet build -c Release ${source}

foreach ($target in $targets)
{
   pwsh CreatePackage.ps1 $target
}