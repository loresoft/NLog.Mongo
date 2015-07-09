@echo off
NuGet.exe update "Source\net45\NLog.Mongo.net45.sln"
NuGet.exe update "Source\net40\NLog.Mongo.net40.sln"

msbuild master.proj -t:Refresh