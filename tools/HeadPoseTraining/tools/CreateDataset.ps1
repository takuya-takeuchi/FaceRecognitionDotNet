Param
(
    [Parameter(Mandatory=$True, Position = 1)]
    [string]
    $InputDirectory,

    [Parameter(Mandatory=$True, Position = 2)]
    [int]
    $TrainRate,

    [Parameter(Mandatory=$True, Position = 3)]
    [string]
    $OutputDirectory,

    [Parameter(Mandatory=$False, Position = 4)]
    [int]
    $Max = 0
)

function main
{
    Param
    (
        [Parameter(Mandatory=$True, Position = 1)]
        [string]
        $InputDirectory,

        [Parameter(Mandatory=$True, Position = 2)]
        [int]
        $TrainRate,

        [Parameter(Mandatory=$True, Position = 3)]
        [string]
        $OutputDirectory,

        [Parameter(Mandatory=$False, Position = 4)]
        [int]
        $Max = 0
    )

    if (!(Test-Path $InputDirectory))
    {
        Write-Host "Error: '${InputDirectory}' does not exist"
        exit -1
    }

    if (!(($TrainRate -gt 0) -And ($TrainRate -lt 10)))
    {
        Write-Host "Error: '${TrainRate}' must be more than 0 and less than 10"
        exit -1
    }

    $images = Get-ChildItem $InputDirectory -Recurse -File -include *.jpg | Where-Object {$_.FullName -notlike "*_Flip*"} 
    $images = $images | Sort-Object {Get-Random}
    if ($Max -ne 0 -And $Max -le $images.Length)
    {
        $images = $images[0..($Max-1)]
    }

    $trainCount = ($images.Length * $TrainRate) / 10

    $trainDirectory = Join-Path $OutputDirectory train
    New-Item $trainDirectory -ItemType Directory -Force
    $testDirectory = Join-Path $OutputDirectory test
    New-Item $testDirectory -ItemType Directory -Force

    $train = 1
    foreach($file in $images)
    {
        $basename = [System.IO.Path]::GetFileNameWithoutExtension($file)
        $filename = [System.IO.Path]::GetFileName($file)
        $rootPath = $file | split-path
        $dirName = [System.IO.Path]::GetFileName($rootPath)

        $landmarkFile = $basename + "_pts.mat"
        $landmark = Join-Path $InputDirectory landmarks | `
                    Join-Path -ChildPath $dirName | `
                    Join-Path -ChildPath $landmarkFile

        if ($train -le $trainCount)
        {
            Write-Host "$dirName/${filename} > train"

            $dstDirectory = Join-Path $trainDirectory $dirName
            if(!(Test-Path($dstDirectory)))
            {
                New-Item $dstDirectory -ItemType Directory
            }

            Copy-Item $file $dstDirectory
            $filenew = [System.IO.Path]::ChangeExtension($file, ".mat")
            Copy-Item $filenew $dstDirectory
            Copy-Item $landmark $dstDirectory
            $train++
        }
        else
        {
            Write-Host "$dirName/${filename} > test"

            $dstDirectory = Join-Path $testDirectory $dirName
            if(!(Test-Path($dstDirectory)))
            {
                New-Item $dstDirectory -ItemType Directory
            }

            Copy-Item $file $dstDirectory
            $filenew = [System.IO.Path]::ChangeExtension($file, ".mat")
            Copy-Item $filenew $dstDirectory
            Copy-Item $landmark $dstDirectory
        }
    }
}

main -InputDirectory $InputDirectory -TrainRate $TrainRate -OutputDirectory $OutputDirectory -Max $Max