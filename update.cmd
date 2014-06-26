@echo off
"Source\.nuget\NuGet.exe" update "Source\NLog.Mongo\NLog.Mongo.netfx40.csproj" -r "Source\packages"
git checkout  "Source\NLog.Mongo\packages.config"
"Source\.nuget\NuGet.exe" update "Source\NLog.Mongo\NLog.Mongo.netfx45.csproj" -r "Source\packages"

"Source\.nuget\NuGet.exe" update "Source\NLog.Mongo.ConsoleTest\NLog.Mongo.ConsoleTest.netfx40.csproj" -r "Source\packages"
git checkout  "Source\NLog.Mongo.ConsoleTest\packages.config"
"Source\.nuget\NuGet.exe" update "Source\NLog.Mongo.ConsoleTest\NLog.Mongo.ConsoleTest.netfx45.csproj" -r "Source\packages"

msbuild master.proj -t:Refresh