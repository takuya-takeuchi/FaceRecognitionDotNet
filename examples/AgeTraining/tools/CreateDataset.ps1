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

function create-data
{
    Param
    (
        [Parameter(Mandatory=$True, Position = 1)]
        [string]
        $UserId,

        [Parameter(Mandatory=$True, Position = 2)]
        [string]
        $OriginalImage,

        [Parameter(Mandatory=$True, Position = 3)]
        [string]
        $FaceId,

        [Parameter(Mandatory=$True, Position = 3)]
        [string]
        $Age
    )

    # "aligned/7153718@N04/landmark_aligned_face.2282.11597935265_29bcdfa4a5_o.jpg"
    $data = New-Object PSObject | Select-Object Path, Age
    $data.Path = Join-Path aligned $UserId | `
                 Join-Path -ChildPath "landmark_aligned_face.${FaceId}.${OriginalImage}"
    $data.Age  = $Age

    return $data
}

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

    $datum = @()
    $path = Join-Path $InputDirectory 'fold_?_data.txt'
    $fold_data_files = Get-ChildItem $path
    foreach($fold_data_file in $fold_data_files)
    {
        foreach ($line in Get-Content $fold_data_file)
        {
            if ($line -match "(?<user_id>[0-9@N]+)\t(?<original_image>[a-z0-9_]+\.jpg)\t(?<face_id>[0-9]+)\t(?<age>\([0-9]+, [0-9]+\)).+")
            {
                $user_id        = $Matches.user_id
                $original_image = $Matches.original_image
                $face_id        = $Matches.face_id
                $age            = $Matches.age
                $data = create-data -UserId $user_id -OriginalImage $original_image -FaceId $face_id -Age $age
                $datum += $data
            }
        }
    }

    $datum = $datum | Sort-Object {Get-Random}
    if ($Max -ne 0 -And $Max -le $datum.Length)
    {
        $datum = $datum[0..($Max-1)]
    }

    $trainCount = ($datum.Length * $TrainRate) / 10

    $trainDirectory = Join-Path $OutputDirectory train
    New-Item $trainDirectory -ItemType Directory -Force
    $testDirectory = Join-Path $OutputDirectory test
    New-Item $testDirectory -ItemType Directory -Force

    $trainDatum = @()
    $testDatum = @()

    $train = 1
    foreach($data in $datum)
    {
        $path = Join-Path $InputDirectory $data.Path
        $age  = $data.Age

        if ($train -le $trainCount)
        {
            Write-Host "${path} > train"

            $dst = Join-Path $trainDirectory $data.Path
            $dst = Split-Path $dst -parent
            if (!(Test-Path -Path $dst))
            {
                New-Item $dst -ItemType directory
            }

            Copy-Item $path $dst
            $train++

            $trainDatum += $data
        }
        else
        {
            Write-Host "${path} > test"

            $dst = Join-Path $testDirectory $data.Path
            $dst = Split-Path $dst -parent
            if (!(Test-Path -Path $dst))
            {
                New-Item $dst -ItemType directory
            }
            
            Copy-Item $path $dst

            $testDatum += $data
        }
    }

    $trainCsv = Join-Path $OutputDirectory train.csv
    New-Item $trainDirectory -ItemType Directory -Force
    $testCsv = Join-Path $OutputDirectory test.csv
    New-Item $testDirectory -ItemType Directory -Force
    $trainDatum | Export-Csv $trainCsv -Encoding Default
    $testDatum | Export-Csv $testCsv -Encoding Default
}

main -InputDirectory $InputDirectory -TrainRate $TrainRate -OutputDirectory $OutputDirectory -Max $Max