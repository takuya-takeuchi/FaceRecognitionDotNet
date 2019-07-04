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

$OperatingSystem="linux"
$Distribution="ubuntu"
$DistributionVersion="16"

# Store current directory
$Current = Get-Location
$FaceRecognitionDotNetRoot = (Split-Path (Get-Location) -Parent)
$FaceRecognitionDotNetSourceRoot = Join-Path $FaceRecognitionDotNetRoot src
$DockerDir = Join-Path $FaceRecognitionDotNetRoot nuget | `
             Join-Path -ChildPath docker

Set-Location -Path $DockerDir

$DockerFileDir = Join-Path $DockerDir test  | `
                 Join-Path -ChildPath $Distribution | `
                 Join-Path -ChildPath $DistributionVersion

$ArchitectureHash = @{32 = "x86"; 64 = "x64"}

$BuildTargets = @()
$BuildTargets += New-Object PSObject -Property @{Target = "cpu";  Architecture = 64; CUDA = 0;   Package = "FaceRecognitionDotNet"         }
$BuildTargets += New-Object PSObject -Property @{Target = "mkl";  Architecture = 64; CUDA = 0;   Package = "FaceRecognitionDotNet.MKL"     }
#$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 92;  Package = "FaceRecognitionDotNet.CUDA92"  }
#$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 100; Package = "FaceRecognitionDotNet.CUDA100" }
#$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 101; Package = "FaceRecognitionDotNet.CUDA101" }

foreach($BuildTarget in $BuildTargets)
{
  $target = $BuildTarget.Target
  $architecture = $BuildTarget.Architecture
  $cudaVersion = $BuildTarget.CUDA
  $package = $BuildTarget.Package
  $options = New-Object 'System.Collections.Generic.List[string]'
  if ($target -ne "cuda")
  {
     $dockername = "facerecognition/test/$Distribution/$DistributionVersion/$Target"
     $imagename  = "dlibdotnet/runtime/$Distribution/$DistributionVersion/$Target"
  }
  else
  {
     $dockername = "facerecognition/test/$Distribution/$DistributionVersion/$Target/$cudaVersion"
     $cudaVersion = ($cudaVersion / 10).ToString("0.0")
     $imagename  = "dlibdotnet/runtime/$Distribution/$DistributionVersion/$Target/$cudaVersion"
  }

  Write-Host "Start docker build -q -t $dockername $DockerFileDir --build-arg IMAGE_NAME=""$imagename""" -ForegroundColor Green
  docker build -q -t $dockername $DockerFileDir --build-arg IMAGE_NAME="$imagename"

  Write-Host "Start docker run --rm -v ""$($FaceRecognitionDotNetRoot):/opt/data/FaceRecognitionDotNet"" -t ""$dockername"" $Version $package $Distribution $DistributionVersion" -ForegroundColor Green
  docker run --rm `
             -v "$($FaceRecognitionDotNetRoot):/opt/data/FaceRecognitionDotNet" `
             -t "$dockername" $Version $package $Distribution $DistributionVersion

  if ($lastexitcode -eq 0) {
     Write-Host "Test Successful" -ForegroundColor Green
  } else {
     Write-Host "Test Fail for $package" -ForegroundColor Red
     Set-Location -Path $Current
     exit -1
  }
}

# Move to Root directory 
Set-Location -Path $Current