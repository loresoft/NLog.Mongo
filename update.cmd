@echo off
NuGet.exe update "Source\net45\NLog.Mongo.net45.sln"

msbuild master.proj -t:Refresh