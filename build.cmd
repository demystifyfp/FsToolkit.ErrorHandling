echo Restoring dotnet tools...
dotnet tool restore

dotnet fake build -t %*