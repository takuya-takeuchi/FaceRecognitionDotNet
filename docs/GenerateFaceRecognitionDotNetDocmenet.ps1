$Current = $PSScriptRoot

$FaceRecognitionDotNetRoot = Split-Path $Current -Parent
$SourceRoot = Join-Path $FaceRecognitionDotNetRoot src
$FaceRecognitionDotNetProjectRoot = Join-Path $SourceRoot FaceRecognitionDotNet

docfx init -q -o docs
Set-Location $Current