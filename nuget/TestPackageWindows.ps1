#***************************************
#Arguments
#***************************************
#Arguments
#%1: Version of Release (1.2.3.0)
#***************************************
Param([Parameter(
      Mandatory=$True,
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
                  "FaceRecognitionDotNet.MKL"
                )

foreach($BuildTarget in $BuildTargets)
{
   $command = ".\\TestPackage.ps1 -Package $BuildTarget -Version $Version -OperatingSystem $OperatingSystem -OperatingSystemVersion $OperatingSystemVersion"
   Invoke-Expression $command

   if ($lastexitcode -ne 0)
   {
      Set-Location -Path $Current
      exit -1
   }
}