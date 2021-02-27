$Current = $PSScriptRoot

$FaceRecognitionDotNetRoot = Split-Path $Current -Parent
$SourceRoot = Join-Path $Current src
$FaceRecognitionDotNetProjectRoot = Join-Path $SourceRoot FaceRecognitionDotNet
$DocumentDir = Join-Path $FaceRecognitionDotNetProjectRoot docfx
$Json = Join-Path $DocumentDir docfx.json

$DocFx = Join-Path $Current docfx | `
         Join-Path -ChildPath docfx.exe

& ${DocFx} "${Json}" --serve

Set-Location $Current