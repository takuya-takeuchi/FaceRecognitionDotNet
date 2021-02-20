Param()

# import class and function
$ScriptPath = $PSScriptRoot
$FaceRecognitionDotNetRoot = Split-Path $ScriptPath -Parent
$NugetPath = Join-Path $FaceRecognitionDotNetRoot "nuget" | `
             Join-Path -ChildPath "BuildUtils.ps1"
import-module $NugetPath -function *

$OperatingSystem="osx"

# Store current directory
$Current = Get-Location
$FaceRecognitionDotNetRoot = (Split-Path (Get-Location) -Parent)
$DlibDotNetRoot = Join-Path $FaceRecognitionDotNetRoot src | `
                  Join-Path -ChildPath DlibDotNet
$FaceRecognitionDotNetSourceRoot = Join-Path $FaceRecognitionDotNetRoot src

$BuildSourceHash = [Config]::GetBinaryLibraryOSXHash()

# https://docs.microsoft.com/ja-jp/dotnet/core/rid-catalog#macos-rids
# osx-x86 does not support
$BuildTargets = @()
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cpu";  Architecture = 64; RID = "$OperatingSystem-x64";   CUDA = 0   }
#$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "cpu";  Architecture = 32; RID = "$OperatingSystem-x86";   CUDA = 0   }
$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "mkl";  Architecture = 64; RID = "$OperatingSystem-x64";   CUDA = 0   }
#$BuildTargets += New-Object PSObject -Property @{ Platform = "desktop"; Target = "mkl";  Architecture = 32; RID = "$OperatingSystem-x86";   CUDA = 0   }

foreach($BuildTarget in $BuildTargets)
{
   $platform = $BuildTarget.Platform
   $target = $BuildTarget.Target
   $architecture = $BuildTarget.Architecture
   $rid = $BuildTarget.RID

   $Config = [Config]::new($FaceRecognitionDotNetRoot, "Release", $target, $architecture, $platform, $option)
   $libraryDir = Join-Path "artifacts" $Config.GetArtifactDirectoryName()
   $build = $Config.GetBuildDirectoryName($OperatingSystem)

   foreach ($key in $BuildSourceHash.keys)
   {
      $srcDir = Join-Path $FaceRecognitionDotNetSourceRoot $key

      # Move to build target directory
      Set-Location -Path $srcDir

      $arc = $Config.GetArchitectureName()
      Write-Host "Build $key [$arc] for $target" -ForegroundColor Green
      Build -Config $Config

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