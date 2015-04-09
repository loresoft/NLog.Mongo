@echo off
Nuget.exe restore "Source\net45\NLog.Mongo.net45.sln"
Nuget.exe restore "Source\net40\NLog.Mongo.net40.sln"

NuGet.exe install MSBuildTasks -OutputDirectory .\Tools\ -ExcludeVersion -NonInteractive
