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
        [int]
        $Age,

        [Parameter(Mandatory=$True, Position = 2)]
        [int]
        $Gender,

        [Parameter(Mandatory=$True, Position = 3)]
        [int]
        $Race
    )

    $GenderArray = @("Male", "Female")
    $RaceArray   = @("White", "Black", "Asian", "Indian", "Others")

    $data = New-Object PSObject | Select-Object Age, Gender, Race
    $data.Age    = $Age
    $data.Gender = $GenderArray[$Gender]
    $data.Race   = $RaceArray[$Race]

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

    $images = Get-ChildItem $InputDirectory -File
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

    $trainDatum = @()
    $testDatum = @()

    $train = 1
    foreach($file in $images)
    {
        $filename = [System.IO.Path]::GetFileName($file)
        if ($filename -match "(?<age>[0-9]+)_(?<gender>[0-1]{1})_(?<race>[0-4]{1})_(?<datetime>[0-9]+).jpg")
        {
            $age    = $Matches.age
            $gender = $Matches.gender
            $race   = $Matches.race
            $data = create-data -Age $age -Gender $gender -Race $race

            if ($train -le $trainCount)
            {
                Write-Host "${filename} > train"
                Copy-Item $file $trainDirectory
                $train++

                $trainDatum += $data
            }
            else
            {
                Write-Host "${filename} > test"
                Copy-Item $file $testDirectory

                $testDatum += $data
            }
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