$dllDir = "BetterChatBox\bin\Release\netstandard2.1\"
$dllName = "BetterChatBox.dll"
$zipName = "release.zip"

$dllPath = $dllDir + $dllName
$files = @(
    $dllPath,
    "README.md",
    "manifest.json"
    "icon.png"
    "CHANGELOG.md"
)

Write-Output "Running dotnet build in Release mode"
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed creating zip. Exiting."
    exit 1
}

$zipPath = Resolve-Path $zipName 
if (Test-Path $zipPath) {
    Write-Output ($zipName + " already exists, removing...")
    Remove-Item $zipPath
}

Write-Output ("Compressing archive to " + ($zipPath))
Compress-Archive -Path $files -DestinationPath $zipPath
Write-Output "Success!"
