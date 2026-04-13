# Setup script for dev environment
Write-Host "Restoring dotnet packages..."
dotnet restore

Write-Host "Installing frontend npm packages..."
cd frontend
if (Test-Path package-lock.json) { npm ci } else { npm install }
cd ..
Write-Host "Setup complete."