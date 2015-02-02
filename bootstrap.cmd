@echo off
Nuget.exe restore "Source\NLog.Mongo.netfx45.sln"
Nuget.exe restore "Source\NLog.Mongo.netfx40.sln"

NuGet.exe install MSBuildTasks -OutputDirectory .\Tools\ -ExcludeVersion -NonInteractive
