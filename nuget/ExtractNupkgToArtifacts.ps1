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

$PublishTargets = @{ "FaceRecognitionDotNet"="cpu";
                     "FaceRecognitionDotNet.CUDA92"="cuda-92";
                     "FaceRecognitionDotNet.CUDA100"="cuda-100";
                     "FaceRecognitionDotNet.CUDA101"="cuda-101";
                     "FaceRecognitionDotNet.CUDA102"="cuda-102";
                     "FaceRecognitionDotNet.CUDA110"="cuda-110";
                     "FaceRecognitionDotNet.CUDA111"="cuda-111";
                     "FaceRecognitionDotNet.CUDA112"="cuda-112";
                     "FaceRecognitionDotNet.MKL"="mkl";
                  }

$Token = $env:FaceRecognitionDotNetNugetToken
if ([string]::IsNullOrWhitespace($Token))
{
    Write-Host "nuget token is missing" -ForegroundColor Red
    exit
}

# Precheck whether all package is present
foreach ($key in $PublishTargets.keys)
{
    $value = $PublishTargets[$key]
    
    $Package = Join-Path $PSScriptRoot "${key}.${Version}.nupkg"
    if (!(Test-Path ${Package}))
    {
        Write-Host "${Package} is missing" -ForegroundColor Red
        exit
    }

    Expand-Archive -Path "${Package}" -DestinationPath tmp
    $runtime = Join-Path tmp runtimes
    $artifacts = Join-Path artifacts ${value} | `
                 Join-Path -ChildPath runtimes
    Copy-Item "${runtime}/*" "${artifacts}" -Recurse -Force
    Remove-Item tmp -Recurse -Force
}