$env:hostDirectory = "C:\emulator\bind-mount"

if (-not (Test-Path $env:hostDirectory)) {
    New-Item -ItemType Directory -Path $env:hostDirectory
    Write-Host "Folder created: $env:hostDirectory"
} else {
    Write-Host "Folder already exists: $env:hostDirectory"
}

docker-compose -f docker-compose-azure-cosmosdb.yaml up