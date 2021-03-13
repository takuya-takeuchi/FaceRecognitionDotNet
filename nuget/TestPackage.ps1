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

function Clear-PackageCache([string]$Package, [string]$Version)
{
   $ret = (dotnet nuget locals global-packages --list)
   $index = $ret.IndexOf('info : global-packages: ')
   if ($index -ne -1)
   {
      $path = $ret.Replace('info : global-packages: ', '').Trim()
   }
   else
   {
      $path = $ret.Replace('global-packages: ', '').Trim()
   }
   $path =  Join-Path $path $Package.ToLower() | `
            Join-Path -ChildPath $Version.ToLower()
   if (Test-Path $path)
   {
      Write-Host "[Info] Remove '$path'" -Foreground Green
      Remove-Item -Path "$path" -Recurse -Force
   }
   else
   {
      Write-Host "[Info] Missing '$path'" -Foreground Yellow
   }
}

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

      $UnitTestDir = Join-Path $FaceRecognitionDotNetRoot test | `
                        Join-Path -ChildPath FaceRecognitionDotNet.Tests

      $TargetDir = Join-Path $WorkDir FaceRecognitionDotNet.Tests
      if (Test-Path "$TargetDir") {
         Remove-Item -Path "$TargetDir" -Recurse -Force 2> $null
      }

      $TargetDirTestImages = Join-Path $TargetDir TestImages
      if (Test-Path "$TargetDirTestImages") {
         Remove-Item -Path "$TargetDirTestImages" -Recurse -Force
      }

      $UnitTestDirTestImages = Join-Path $UnitTestDir TestImages
      $UnitTestDirSources = Join-Path $UnitTestDir "*.cs"
      $UnitTestDirProject = Join-Path $UnitTestDir "FaceRecognitionDotNet.Tests.csproj"
      Copy-Item "$UnitTestDirTestImages" "$TargetDirTestImages" -Recurse
      Copy-Item "$UnitTestDirSources" "$TargetDir" -Recurse
      Copy-Item "$UnitTestDirProject" "$TargetDir" -Recurse
      

      Set-Location -Path "$TargetDir"

      # delete local project reference
      dotnet remove reference ..\..\src\DlibDotNet\src\DlibDotNet\DlibDotNet.csproj > $null
      dotnet remove reference ..\..\src\FaceRecognitionDotNet\FaceRecognitionDotNet.csproj > $null

      Write-Host "[Info] Clear-PackageCache" -Foreground Yellow
      Clear-PackageCache -Package $package -Version $VERSION

      # restore package from local nuget pacakge
      # And drop stdout message
      dotnet add package $package -v $VERSION --source "$NugetDir" > $null

      # Copy Dependencies
      if ($global:IsWindows)
      {
         # Get framework version
         $re = New-Object regex("<TargetFramework>(?<version>[^<]+)</TargetFramework>")
         $match = $re.Matches((Get-Content "${UnitTestDirProject}"))
         $version = $match[0].Groups["version"]

         # Just in case, deploy symbolic link to possbile output directory
         $OutDirs = @((Join-Path $TargetDir bin | Join-Path -ChildPath Release | Join-Path -ChildPath $version),
                      (Join-Path $TargetDir bin | Join-Path -ChildPath x64     | Join-Path -ChildPath Release | Join-Path -ChildPath $version),
                      (Join-Path $TargetDir bin | Join-Path -ChildPath x86     | Join-Path -ChildPath Release | Join-Path -ChildPath $version)
         )

         foreach ($OutDir in $OutDirs)
         {
            if (!(Test-Path "$OutDir"))
            {
               New-Item "$OutDir" -ItemType Directory > $null
            }

            if ($DependencyHash.Contains($package))
            {
               foreach($Dependency in $DependencyHash[$package])
               {
                  $FileName = [System.IO.Path]::GetFileName("$Dependency")
                  New-Item -Value "$Dependency" -Path "$OutDir" -Name "$FileName" -ItemType SymbolicLink > $null
               }
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
$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 102; Package = "FaceRecognitionDotNet.CUDA102" }
$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 110; Package = "FaceRecognitionDotNet.CUDA110" }
$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 111; Package = "FaceRecognitionDotNet.CUDA111" }
$BuildTargets += New-Object PSObject -Property @{Target = "cuda"; Architecture = 64; CUDA = 112; Package = "FaceRecognitionDotNet.CUDA112" }
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

# For FaceRecognitionDotNet.CUDA102
$tmp102 = New-Object 'System.Collections.Generic.List[string]'
$tmp102.Add("$env:CUDA_PATH_V10_2\bin\cublas64_10.dll")
$tmp102.Add("$env:CUDA_PATH_V10_2\bin\cudnn64_7.dll")
$tmp102.Add("$env:CUDA_PATH_V10_2\bin\curand64_10.dll")
$tmp102.Add("$env:CUDA_PATH_V10_2\bin\cusolver64_10.dll")

# For FaceRecognitionDotNet.CUDA110
$tmp110 = New-Object 'System.Collections.Generic.List[string]'
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cublas64_11.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cublasLt64_11.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cudnn_adv_infer64_8.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cudnn_adv_train64_8.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cudnn_cnn_infer64_8.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cudnn_cnn_train64_8.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cudnn_ops_infer64_8.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cudnn_ops_train64_8.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cudnn64_8.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\curand64_10.dll")
$tmp110.Add("$env:CUDA_PATH_V11_0\bin\cusolver64_10.dll")

# For FaceRecognitionDotNet.CUDA111
$tmp111 = New-Object 'System.Collections.Generic.List[string]'
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cublas64_11.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cublasLt64_11.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cudnn_adv_infer64_8.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cudnn_adv_train64_8.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cudnn_cnn_infer64_8.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cudnn_cnn_train64_8.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cudnn_ops_infer64_8.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cudnn_ops_train64_8.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cudnn64_8.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\curand64_10.dll")
$tmp111.Add("$env:CUDA_PATH_V11_1\bin\cusolver64_11.dll")

# For FaceRecognitionDotNet.CUDA111
$tmp112 = New-Object 'System.Collections.Generic.List[string]'
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cublas64_11.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cublasLt64_11.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cudnn_adv_infer64_8.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cudnn_adv_train64_8.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cudnn_cnn_infer64_8.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cudnn_cnn_train64_8.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cudnn_ops_infer64_8.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cudnn_ops_train64_8.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cudnn64_8.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\curand64_10.dll")
$tmp112.Add("$env:CUDA_PATH_V11_2\bin\cusolver64_11.dll")

# For mkl
$tmpmkl = New-Object 'System.Collections.Generic.List[string]'
$tmpmkl.Add("$env:MKL_WIN\redist\intel64_win\mkl\mkl_core.dll")
$tmpmkl.Add("$env:MKL_WIN\redist\intel64_win\mkl\mkl_intel_thread.dll")
$tmpmkl.Add("$env:MKL_WIN\redist\intel64_win\mkl\mkl_avx2.dll")
$tmpmkl.Add("$env:MKL_WIN\redist\intel64_win\compiler\libiomp5md.dll")

$DependencyHash = @{"FaceRecognitionDotNet.CUDA92"  = $tmp92;
                    "FaceRecognitionDotNet.CUDA100" = $tmp100;
                    "FaceRecognitionDotNet.CUDA101" = $tmp101;
                    "FaceRecognitionDotNet.CUDA102" = $tmp102;
                    "FaceRecognitionDotNet.CUDA110" = $tmp110;
                    "FaceRecognitionDotNet.CUDA111" = $tmp111;
                    "FaceRecognitionDotNet.CUDA112" = $tmp112;
                    "FaceRecognitionDotNet.MKL"     = $tmpmkl}

# Store current directory
$Current = Get-Location
$FaceRecognitionDotNetRoot = (Split-Path (Get-Location) -Parent)

$targets = $BuildTargets.Where({$PSItem.Package -eq $Package})
RunTest $targets $DependencyHash

# Move to Root directory
Set-Location -Path $Current