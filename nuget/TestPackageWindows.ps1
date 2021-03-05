#***************************************
#Arguments
#***************************************
#Arguments
#%1: Version of Release (1.2.3.0)
#***************************************
Param([Parameter(
      Mandatory=$False,
      Position = 1
      )][string]
      $Version
)

Set-StrictMode -Version Latest

$OperatingSystem="windows"
$OperatingSystemVersion="10"

# Store current directory
$Current = Get-Location

$BuildTargets = ( "FaceRecognitionDotNet",
                  "FaceRecognitionDotNet.CUDA92",
                  "FaceRecognitionDotNet.CUDA100",
                  "FaceRecognitionDotNet.CUDA101",
                  "FaceRecognitionDotNet.CUDA102",
                  "FaceRecognitionDotNet.CUDA110",
                  "FaceRecognitionDotNet.CUDA111",
                  "FaceRecognitionDotNet.CUDA112",
                  "FaceRecognitionDotNet.MKL"
                )

foreach($BuildTarget in $BuildTargets)
{
   $versionStr = $Version
   $package = $BuildTarget

   if ([string]::IsNullOrEmpty($versionStr))
   {
      $packages = Get-ChildItem "${Current}/*" -include *.nupkg | `
                  Where-Object -FilterScript {$_.Name -match "${package}\.([0-9\.]+).nupkg"} | `
                  Sort-Object -Property Name -Descending
      foreach ($file in $packages)
      {
         Write-Host $file -ForegroundColor Blue
      }

      foreach ($file in $packages)
      {
         $file = Split-Path $file -leaf
         $file = $file -replace "${package}\.",""
         $file = $file -replace "\.nupkg",""
         $versionStr = $file
         break
      }

      if ([string]::IsNullOrEmpty($versionStr))
      {
         Write-Host "Version is not specified" -ForegroundColor Red
         exit -1
      }
   }

   $command = ".\\TestPackage.ps1 -Package $BuildTarget -Version $versionStr -OperatingSystem $OperatingSystem -OperatingSystemVersion $OperatingSystemVersion"
   Invoke-Expression $command

   if ($lastexitcode -ne 0)
   {
      Set-Location -Path $Current
      exit -1
   }
}