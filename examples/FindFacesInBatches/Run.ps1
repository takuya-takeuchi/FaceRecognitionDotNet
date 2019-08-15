Param([Parameter(
      Mandatory=$False,
      Position = 1
      )][int]
      $BatchSize = 128
)

$Version = "4.1.0.20190416"
$WindowsPackage = "OpenCvSharp4.runtime.win"
$LinuxPackage = "OpenCvSharp4.runtime.ubuntu.18.04-x64"

if ($global:IsWindows)
{
	dotnet remove package $LinuxPackage > $null
	dotnet add package $WindowsPackage -v $Version > $null
	dotnet run -c Release -- "-b=$BatchSize"
}
elseif ($global:IsLinux)
{
	dotnet remove package $WindowsPackage > $null
	dotnet add package $LinuxPackage -v $Version > $null
	dotnet run -c Release -- "-b=$BatchSize"
}