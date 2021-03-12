Param()

# import class and function
$ScriptPath = $PSScriptRoot
$FaceRecognitionDotNetRoot = Split-Path $ScriptPath -Parent
$NugetPath = Join-Path $FaceRecognitionDotNetRoot "nuget" | `
             Join-Path -ChildPath "BuildUtils.ps1"
import-module $NugetPath -function *

$OperatingSystem="centos"
$Distribution="centos"
$DistributionVersion="7"

# Store current directory
$Current = Get-Location
$FaceRecognitionDotNetRoot = (Split-Path (Get-Location) -Parent)
$FaceRecognitionDotNetSourceRoot = Join-Path $FaceRecognitionDotNetRoot src
$DockerDir = Join-Path $FaceRecognitionDotNetRoot docker

Set-Location -Path $DockerDir

$DockerFileDir = Join-Path $DockerDir build | `
                 Join-Path -ChildPath $Distribution | `
                 Join-Path -ChildPath $DistributionVersion

$BuildSourceHash = [Config]::GetBinaryLibraryLinuxHash()

# https://github.com/dotnet/coreclr/issues/9265
# linux-x86 does not support
$BuildTargets = @()
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cpu";  Architecture = 64; Postfix = "/x64"; RID = "$OperatingSystem-x64";   CUDA = 0   }
# $BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cpu";  Architecture = 32; Postfix = "/x86"; RID = "$OperatingSystem-x86";   CUDA = 0   }
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "mkl";  Architecture = 64; Postfix = "/x64"; RID = "$OperatingSystem-x64";   CUDA = 0   }
# $BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "mkl";  Architecture = 32; Postfix = "/x86"; RID = "$OperatingSystem-x86";   CUDA = 0   }
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cuda"; Architecture = 64; Postfix = "";     RID = "$OperatingSystem-x64";   CUDA = 92  }
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cuda"; Architecture = 64; Postfix = "";     RID = "$OperatingSystem-x64";   CUDA = 100 }
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cuda"; Architecture = 64; Postfix = "";     RID = "$OperatingSystem-x64";   CUDA = 101 }
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cuda"; Architecture = 64; Postfix = "";     RID = "$OperatingSystem-x64";   CUDA = 102 }
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cuda"; Architecture = 64; Postfix = "";     RID = "$OperatingSystem-x64";   CUDA = 110 }
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cuda"; Architecture = 64; Postfix = "";     RID = "$OperatingSystem-x64";   CUDA = 111 }
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cuda"; Architecture = 64; Postfix = "";     RID = "$OperatingSystem-x64";   CUDA = 112 }
#$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "arm";  Architecture = 64; Postfix = "64";   RID = "$OperatingSystem-arm64"; CUDA = 0   }
#$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "arm";  Architecture = 32; Postfix = "";     RID = "$OperatingSystem-arm";   CUDA = 0   }

foreach($BuildTarget in $BuildTargets)
{
   $platform = $BuildTarget.Platform
   $target = $BuildTarget.Target
   $architecture = $BuildTarget.Architecture
   $rid = $BuildTarget.RID
   $cudaVersion = $BuildTarget.CUDA
   $postfix = $BuildTarget.Postfix

   if ($target -ne "cuda")
   {
      $option = ""
      
      $dockername = "dlibdotnet/build/$Distribution/$DistributionVersion/$Target" + $postfix
      $imagename  = "dlibdotnet/devel/$Distribution/$DistributionVersion/$Target" + $postfix
   }
   else
   {
      $option = $cudaVersion

      $cudaVersion = ($cudaVersion / 10).ToString("0.0")
      $dockername = "dlibdotnet/build/$Distribution/$DistributionVersion/$Target/$cudaVersion"
      $imagename  = "dlibdotnet/devel/$Distribution/$DistributionVersion/$Target/$cudaVersion"
   }

   $Config = [Config]::new($FaceRecognitionDotNetRoot, "Release", $target, $architecture, $platform, $option)
   $libraryDir = Join-Path "artifacts" $Config.GetArtifactDirectoryName()
   $build = $Config.GetBuildDirectoryName($OperatingSystem)

   Write-Host "Start 'docker build -t $dockername $DockerFileDir --build-arg IMAGE_NAME=""$imagename""'" -ForegroundColor Green
   docker build --force-rm=true -t $dockername $DockerFileDir --build-arg IMAGE_NAME="$imagename"

   if ($lastexitcode -ne 0)
   {
      Set-Location -Path $Current
      exit -1
   }

   # Build binary
   foreach ($key in $BuildSourceHash.keys)
   {
      Write-Host "Start 'docker run --rm -v ""$($FaceRecognitionDotNetRoot):/opt/data/FaceRecognitionDotNet"" -e LOCAL_UID=$(id -u $env:USER) -e LOCAL_GID=$(id -g $env:USER) -t $dockername'" -ForegroundColor Green
      if ($Config.HasStoreDriectory())
      {
         $storeDirecotry = $Config.GetRootStoreDriectory()
         docker run --rm `
                     -v "$($storeDirecotry):/opt/data/builds" `
                     -v "$($FaceRecognitionDotNetRoot):/opt/data/FaceRecognitionDotNet" `
                     -e "LOCAL_UID=$(id -u $env:USER)" `
                     -e "LOCAL_GID=$(id -g $env:USER)" `
                     -e "CIBuildDir=/opt/data/builds" `
                     -t "$dockername" $key $target $architecture $platform $option
      }
      else
      {
         docker run --rm `
                     -v "$($FaceRecognitionDotNetRoot):/opt/data/FaceRecognitionDotNet" `
                     -e "LOCAL_UID=$(id -u $env:USER)" `
                     -e "LOCAL_GID=$(id -g $env:USER)" `
                     -t "$dockername" $key $target $architecture $platform $option
      }
   
      if ($lastexitcode -ne 0)
      {
         Set-Location -Path $Current
         exit -1
      }
   }

   # Copy output binary
   foreach ($key in $BuildSourceHash.keys)
   {
      $srcDir = Join-Path $FaceRecognitionDotNetSourceRoot $key
      $srcDir = $Config.GetStoreDriectory($srcDir)
      
      $dll = $BuildSourceHash[$key]
      $dstDir = Join-Path $Current $libraryDir

      CopyToArtifact -srcDir $srcDir -build $build -libraryName $dll -dstDir $dstDir -rid $rid
   }
}

# Move to Root directory 
Set-Location -Path $Current
