Param()

# import class and function
$ScriptPath = $PSScriptRoot
$DlibDotNetRoot = Split-Path $ScriptPath -Parent
$NugetPath = Join-Path $DlibDotNetRoot "nuget" | `
             Join-Path -ChildPath "BuildUtils.ps1"
import-module $NugetPath -function *

$OperatingSystem="linux"
$Distribution="ubuntu"
$DistributionVersion="16"
$AndroidVersion="28.0.3-r20-jdk8"
$AndroidNativeApiLevel="23"

# Store current directory
$Current = Get-Location
$DlibDotNetRoot = (Split-Path (Get-Location) -Parent)
$DlibDotNetSourceRoot = Join-Path $DlibDotNetRoot src
$DockerDir = Join-Path $Current docker

Set-Location -Path $DockerDir

$DockerFileDir = Join-Path $DockerDir build  | `
                 Join-Path -ChildPath $Distribution | `
                 Join-Path -ChildPath $DistributionVersion

$BuildSourceHash = [Config]::GetBinaryLibraryLinuxHash()

# https://docs.microsoft.com/ja-jp/xamarin/cross-platform/cpp/
# arm64-v8a
# armeabi-v7a
# x86
# x86_64
$BuildTargets = @()
$BuildTargets += New-Object PSObject -Property @{ Platform = "android"; Target = "arm"; Architecture = 64; RID = "arm64-v8a"   }
$BuildTargets += New-Object PSObject -Property @{ Platform = "android"; Target = "arm"; Architecture = 32; RID = "armeabi-v7a" }
$BuildTargets += New-Object PSObject -Property @{ Platform = "android"; Target = "arm"; Architecture = 32; RID = "x86"         }
$BuildTargets += New-Object PSObject -Property @{ Platform = "android"; Target = "arm"; Architecture = 64; RID = "x86_64"      }

$dockername = "dlibdotnet/build/$Distribution/$DistributionVersion/android/$AndroidVersion"
$imagename  = "dlibdotnet/devel/$Distribution/$DistributionVersion/android/$AndroidVersion"

Write-Host "Start 'docker build -t $dockername $DockerFileDir --build-arg IMAGE_NAME=""$imagename""'" -ForegroundColor Green
docker build --force-rm=true -t $dockername $DockerFileDir --build-arg IMAGE_NAME="$imagename"

if ($lastexitcode -ne 0)
{
   Set-Location -Path $Current
   exit -1
}

foreach($BuildTarget in $BuildTargets)
{
   $platform = $BuildTarget.Platform
   $target = $BuildTarget.Target
   $architecture = $BuildTarget.Architecture
   $rid = $BuildTarget.RID

   $setting =
   @{
      'ANDROID_ABI' = $rid;
      'ANDROID_NATIVE_API_LEVEL' = $AndroidNativeApiLevel
   }

   # Build binary
   foreach ($key in $BuildSourceHash.keys)
   {
      $option = [Config]::Base64Encode((ConvertTo-Json -Compress $setting))
      $Config = [Config]::new($DlibDotNetRoot, "Release", $target, $architecture, $platform, $option)
      $libraryDir = Join-Path "artifacts" $Config.GetArtifactDirectoryName()
      $build = $Config.GetBuildDirectoryName($OperatingSystem)
      Write-Host "Start 'docker run --rm -v ""$($DlibDotNetRoot):/opt/data/DlibDotNet"" -e LOCAL_UID=$(id -u $env:USER) -e LOCAL_GID=$(id -g $env:USER) -t $dockername'" -ForegroundColor Green
      docker run --rm `
                  -v "$($DlibDotNetRoot):/opt/data/DlibDotNet" `
                  -e "LOCAL_UID=$(id -u $env:USER)" `
                  -e "LOCAL_GID=$(id -g $env:USER)" `
                  -t "$dockername" $key $target $architecture $platform $option
   
      if ($lastexitcode -ne 0)
      {
         Set-Location -Path $Current
         exit -1
      }
   }

   # Copy output binary
   foreach ($key in $BuildSourceHash.keys)
   {
      $srcDir = Join-Path $DlibDotNetSourceRoot $key
      $srcDir = $Config.GetStoreDriectory($srcDir)
      
      $dll = $BuildSourceHash[$key]
      $dstDir = Join-Path $Current $libraryDir

      CopyToArtifact -srcDir $srcDir -build $build -libraryName $dll -dstDir $dstDir -rid $rid
   }
}

# Move to Root directory 
Set-Location -Path $Current