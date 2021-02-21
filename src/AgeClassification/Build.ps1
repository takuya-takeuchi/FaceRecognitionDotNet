#***************************************
#Arguments
#%1: Build Configuration (Release/Debug)
#%2: Target (cpu/cuda/mkl/arm)
#%3: Architecture (32/64)
#%4: Platform (desktop,android/ios/uwp)
#%5: Optional Argument
#   if Target is cuda, CUDA version if Target is cuda [90/91/92/100/101]
#   if Target is mkl and Windows, IntelMKL directory path
#***************************************
Param
(
   [Parameter(
   Mandatory=$True,
   Position = 1
   )][string]
   $Configuration,

   [Parameter(
   Mandatory=$True,
   Position = 2
   )][string]
   $Target,

   [Parameter(
   Mandatory=$True,
   Position = 3
   )][int]
   $Architecture,

   [Parameter(
   Mandatory=$False,
   Position = 4
   )][string]
   $Platform,

   [Parameter(
   Mandatory=$False,
   Position = 5
   )][string]
   $Option
)

# import class and function
$ScriptPath = $PSScriptRoot
Write-Host "Build "(Split-Path $ScriptPath -Leaf) -ForegroundColor Green

$SrcPath = Split-Path $ScriptPath -Parent
$FaceRecognitionDotNetRoot = Split-Path $SrcPath -Parent
$NugetPath = Join-Path $FaceRecognitionDotNetRoot "nuget" | `
             Join-Path -ChildPath "BuildUtils.ps1"
import-module $NugetPath -function *

$Config = [Config]::new($FaceRecognitionDotNetRoot, $Configuration, $Target, $Architecture, $Platform, $Option)
Build -Config $Config