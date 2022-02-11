param
(
    [parameter(Mandatory=$true)]
    [String] $rootDirectory,
    
    [parameter(Mandatory=$true)]
    [string] $outputDirectory,
    
    [parameter(Mandatory=$true)]
    [float] $trainingRatio
)

if (!(Test-Path("${rootDirectory}")))
{
    Write-Host "${rootDirectory} is missing" -ForegroundColor Red
    exit -1
}

$directories = @(
    "anger",
    "contempt",
    "disgust",
    "fear",
    "happiness",
    "neutrality",
    "sadness",
    "surprise"
)

foreach ($directory in $directories)
{
    $path = Join-Path "${rootDirectory}" $directory
    if (!(Test-Path("${path}")))
    {
        Write-Host "${path} is missing" -ForegroundColor Red
        exit -2
    }
}

if ($trainingRatio -lt 0 -Or $trainingRatio -gt 1.0)
{
    Write-Host "trainingRatio must be 0 <= and <= 1.0" -ForegroundColor Red
    exit -3
}

$training = @()
$test = @()

foreach ($directory in $directories)
{
    $path = Join-Path "${rootDirectory}" $directory
    $files = Get-ChildItem -Path "${path}" -Recurse -Include *.png
    $count = ($files | Measure-Object).Count
    Write-Host "${directory} Total: ${count} files" -ForegroundColor Green

    # generate random list to sort file list
    $indexes = 0..$count
    $indexes = $indexes | Sort-Object {Get-Random}

    $traningCount = [int]($count * $trainingRatio)
    $testCount = $count - $traningCount
    $total = $traningCount + $testCount
    if ($total -ne $count)
    {
        $traningCount += 1
    }

    Write-Host "`tTraining Data: ${traningCount} files" -ForegroundColor Green
    Write-Host "`t    Test Data: ${testCount} files" -ForegroundColor Green

    $indexes = 0..$count
    foreach ($index in $indexes)
    {
        $file = @($files)[$index]
        $name = $file.Name
        if (!($name))
        {
            continue
        }
        $file = Join-Path $directory $name

        if ($traningCount -gt $index)
        {
            $training += $file        
        }
        else
        {
            $test += $file  
        }
    }
}

$traningCount = ($training | Measure-Object).Count
$testCount = ($test | Measure-Object).Count
Write-Host ""
Write-Host "All Training Data: ${traningCount} files" -ForegroundColor Green
Write-Host "    All Test Data: ${testCount} files" -ForegroundColor Green

$inputs = @()
$inputs += New-Object PSObject -Property @{Input = $training;  FileName = "train.txt" }
$inputs += New-Object PSObject -Property @{Input = $test;      FileName = "test.txt" }

Write-Host ""
foreach ($input in $inputs)
{
    $source    = $input.Input
    $fileName   = $input.FileName

    # shuffle
    $source = $source | Get-Random -Count ([int]::MaxValue)

    $path = Join-Path "${outputDirectory}" $fileName

    Write-Host "Write to ${path}" -ForegroundColor Green
    $source | Out-File -Encoding Default "tmp.txt"
    $lfText = [System.IO.File]::ReadAllText("tmp.txt").Replace("`r`n","`n")
    [System.IO.File]::WriteAllText("${path}", $lfText)

    Remove-Item "tmp.txt" -Force | Out-Null
}
