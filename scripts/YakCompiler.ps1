param (
    [Parameter(Mandatory=$true, HelpMessage="The input directory")]
    [Alias("Path")]
    [string]$inputDir,

    [Parameter(Mandatory=$false, HelpMessage="The output location")]
    [Alias("DestinationPath")]
    [string]$outputDir = $inputDir,

    [Parameter(Mandatory=$false, HelpMessage="What Config was Project built for?")]
    [Alias("Config")]
    [string]$configuration="Debug",

    [Parameter(Mandatory=$false, HelpMessage="Push to yak server?")]
    [bool]$publish=$false
)

$yakExe = "C:\Program Files\Rhino 7\System\Yak.exe"
if (!$IsWindows)
{
    $yakExe = "/Applications/Rhino 8.app/Contents/Resources/bin/yak"
}

foreach($yakFile in Get-ChildItem "$outputDir\*.yak")
{
    Remove-Item $yakFile
}

$originalLocation = Get-Location
Set-Location $inputDir

& $yakExe build

if ($inputDir -ne $outputDir)
{
    Copy-Item -Path "$inputDir\*.yak" -DestinationPath $outputDir
}

Set-Location $originalLocation