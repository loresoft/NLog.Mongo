@echo off
"Source\.nuget\NuGet.exe" update "Source\net40\NLog.Mongo\NLog.Mongo.netfx40.csproj" -r "Source\net40\packages"
"Source\.nuget\NuGet.exe" update "Source\net45\NLog.Mongo\NLog.Mongo.netfx45.csproj" -r "Source\net45\packages"

"Source\.nuget\NuGet.exe" update "Source\net40\NLog.Mongo.ConsoleTest\NLog.Mongo.ConsoleTest.netfx40.csproj" -r "Source\net40\packages"
"Source\.nuget\NuGet.exe" update "Source\net45\NLog.Mongo.ConsoleTest\NLog.Mongo.ConsoleTest.netfx45.csproj" -r "Source\net45\packages"

msbuild master.proj -t:Refresh