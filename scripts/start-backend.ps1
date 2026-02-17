# Start backend in current terminal
Set-Location "$PSScriptRoot\.."
dotnet run --project HrSystem.Api --no-build --urls http://localhost:55330
