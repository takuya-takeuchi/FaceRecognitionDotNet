$Current = $PSScriptRoot

$FaceRecognitionDotNetRoot = Split-Path $Current -Parent
$SourceRoot = Join-Path $FaceRecognitionDotNetRoot src
$FaceRecognitionDotNetProjectRoot = Join-Path $SourceRoot FaceRecognitionDotNet
$DocumentDir = Join-Path $FaceRecognitionDotNetProjectRoot docfx
$Json = Join-Path $Current docfx.json

$DocFx = Join-Path $Current docfx | `
         Join-Path -ChildPath docfx.exe

Set-Location $FaceRecognitionDotNetRoot
& ${DocFx} "${Json}" --serve
Set-Location $Current