dotnet publish SE.Launcher.csproj -c Release -r win-x64 --self-contained true --output ./bin/publish/Win64
dotnet publish SE.Launcher.csproj -c Release -r linux-x64 --self-contained true --output ./bin/publish/Linux64
dotnet publish SE.Launcher.csproj -c Release -r osx-x64 --self-contained true --output ./bin/publish/OSX64
pause