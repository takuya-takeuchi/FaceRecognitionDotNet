$Current = $PSScriptRoot

$FaceRecognitionDotNetRoot = Split-Path $Current -Parent
$SourceRoot = Join-Path $FaceRecognitionDotNetRoot src
$FaceRecognitionDotNetProjectRoot = Join-Path $SourceRoot FaceRecognitionDotNet

$DocFx = Join-Path $Current docfx | `
         Join-Path -ChildPath docfx.exe

Set-Location $FaceRecognitionDotNetRoot
& ${DocFx} init -q -o docs
Set-Location $Current