#***************************************
#Arguments
#%1: Test Package (FaceRecognitionDotNet.CUDA92)
#%2: Version of Release (1.2.3.0)
#***************************************
Param([Parameter(
      Mandatory=$True,
      Position = 1
      )][string]
      $Package,

      [Parameter(
      Mandatory=$True,
      Position = 2
      )][string]
      $Version,

      [Parameter(
      Mandatory=$True,
      Position = 3
      )][string]
      $OperatingSystem,

      [Parameter(
      Mandatory=$True,
      Position = 4
      )][string]
      $OperatingSystemVersion
)

Set-StrictMode -Version Latest

function RunTest($BuildTargets, $DependencyHash)
{
   foreach($BuildTarget in $BuildTargets)
   {
      $package = $BuildTarget.Package

      # Test
      $WorkDir = Join-Path $FaceRecognitionDotNetRoot work
      $NugetDir = Join-Path $FaceRecognitionDotNetRoot nuget
      $TestDir = Join-Path $NugetDir artifacts | `
                  Join-Path -ChildPath test | `
                  Join-Path -ChildPath $package | `
                  Join-Path -ChildPath $Version | `
                  Join-Path -ChildPath $OperatingSystem

      if (!(Test-Path "$WorkDir")) {
         New-Item "$WorkDir" -ItemType Directory > $null
      }
      if (!(Test-Path "$TestDir")) {
         New-Item "$TestDir" -ItemType Directory > $null
      }

      $NativeTestDir = Join-Path $FaceRecognitionDotNetRoot test | `
                        Join-Path -ChildPath FaceRecognitionDotNet.Tests

      $TargetDir = Join-Path $WorkDir FaceRecognitionDotNet.Tests
      if (Test-Path "$TargetDir") {
         Remove-Item -Path "$TargetDir" -Recurse -Force
      }

      Copy-Item "$NativeTestDir" "$WorkDir" -Recurse

      Set-Location -Path "$TargetDir"

      # delete local project reference
      dotnet remove reference ..\..\src\DlibDotNet\src\DlibDotNet\DlibDotNet.csproj > $null
      dotnet remove reference ..\..\src\FaceRecognitionDotNet\FaceRecognitionDotNet.csproj > $null

      # restore package from local nuget pacakge
      # And drop stdout message
      dotnet add package $package -v $VERSION --source "$NugetDir" > $null

      # Copy Dependencies
      $OutDir = Join-Path $TargetDir bin | `
                  Join-Path -ChildPath Release | `
                  Join-Path -ChildPath netcoreapp2.0      
      if (!(Test-Path "$OutDir")) {
         New-Item "$OutDir" -ItemType Directory > $null
      }

      if ($IsWindows)
      {
         if ($DependencyHash.Contains($package))
         {
            foreach($Dependency in $DependencyHash[$package])
            {
               Copy-Item "$Dependency" "$OutDir"
            }
         }
      }

      $ErrorActionPreference = "silentlycontinue"
      dotnet test -c Release -r "$TestDir" --logger trx

      if ($lastexitcode -eq 0) {
         Write-Host "Test Successful" -ForegroundColor Green
      } else {
         Write-Host "Test Fail for $package" -ForegroundColor Red
         Set-Location -Path $Current
         exit -1
      }

      $ErrorActionPreference = "continue"

      # move to current
      Set-Location -Path "$Current"

      # to make sure, delete
      if (Test-Path "$WorkDir") {
         Remove-Item -Path "$WorkDir" -Recurse -Force
      }
   }
}

$BuildTargets = @()
$BuildTargets += New-Object PSObject -Property @{Target = "cpu";  Architecture = 64; CUDA = 0;   Package = "FaceRecognitionDotNet"         }
$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 92;  Package = "FaceRecognitionDotNet.CUDA92"  }
$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 100; Package = "FaceRecognitionDotNet.CUDA100" }
$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 101; Package = "FaceRecognitionDotNet.CUDA101" }
$BuildTargets += New-Object PSObject -Property @{Target = "mkl";  Architecture = 64; CUDA = 0;   Package = "FaceRecognitionDotNet.MKL"     }


# For FaceRecognitionDotNet.CUDA92
$tmp92 = New-Object 'System.Collections.Generic.List[string]'
$tmp92.Add("$env:CUDA_PATH_V9_2\bin\cublas64_92.dll")
$tmp92.Add("$env:CUDA_PATH_V9_2\bin\cudnn64_7.dll")
$tmp92.Add("$env:CUDA_PATH_V9_2\bin\curand64_92.dll")
$tmp92.Add("$env:CUDA_PATH_V9_2\bin\cusolver64_92.dll")

# For FaceRecognitionDotNet.CUDA100
$tmp100 = New-Object 'System.Collections.Generic.List[string]'
$tmp100.Add("$env:CUDA_PATH_V10_0\bin\cublas64_100.dll")
$tmp100.Add("$env:CUDA_PATH_V10_0\bin\cudnn64_7.dll")
$tmp100.Add("$env:CUDA_PATH_V10_0\bin\curand64_100.dll")
$tmp100.Add("$env:CUDA_PATH_V10_0\bin\cusolver64_100.dll")

# For FaceRecognitionDotNet.CUDA101
$tmp101 = New-Object 'System.Collections.Generic.List[string]'
$tmp101.Add("$env:CUDA_PATH_V10_1\bin\cublas64_10.dll")
$tmp101.Add("$env:CUDA_PATH_V10_1\bin\cudnn64_7.dll")
$tmp101.Add("$env:CUDA_PATH_V10_1\bin\curand64_10.dll")
$tmp101.Add("$env:CUDA_PATH_V10_1\bin\cusolver64_10.dll")

# For mkl
$tmpmkl = New-Object 'System.Collections.Generic.List[string]'
$tmpmkl.Add("$env:MKL_WIN\redist\intel64_win\mkl\mkl_core.dll")
$tmpmkl.Add("$env:MKL_WIN\redist\intel64_win\mkl\mkl_intel_thread.dll")
$tmpmkl.Add("$env:MKL_WIN\redist\intel64_win\mkl\mkl_avx2.dll")
$tmpmkl.Add("$env:MKL_WIN\redist\intel64_win\compiler\libiomp5md.dll")

$DependencyHash = @{"FaceRecognitionDotNet.CUDA92"  = $tmp92;
                    "FaceRecognitionDotNet.CUDA100" = $tmp100;
                    "FaceRecognitionDotNet.CUDA101" = $tmp101;
                    "FaceRecognitionDotNet.MKL"     = $tmpmkl}

# Store current directory
$Current = Get-Location
$FaceRecognitionDotNetRoot = (Split-Path (Get-Location) -Parent)

$targets = $BuildTargets.Where({$PSItem.Package -eq $Package})
RunTest $targets $DependencyHash

# Move to Root directory
Set-Location -Path $Current