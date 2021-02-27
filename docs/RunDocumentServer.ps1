$Current = $PSScriptRoot

$FaceRecognitionDotNetRoot = Split-Path $Current -Parent
$SourceRoot = Join-Path $FaceRecognitionDotNetRoot src
$FaceRecognitionDotNetProjectRoot = Join-Path $SourceRoot FaceRecognitionDotNet
$DocumentDir = Join-Path $FaceRecognitionDotNetProjectRoot docfx
$Json = Join-Path $Current docfx.json

docfx "${Json}" --serve
Set-Location $Current