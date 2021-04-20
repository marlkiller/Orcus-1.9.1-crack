@echo off
COPY %1 temp.exe
echo "temp file copied"
"..\..\..\packages\ILMerge.2.14.1208\tools\ILMerge.exe" "/t:winexe /keyfile:"..\..\OrcusStrongKey.snk /v4 /out:Orcus.Server.CommandLine.exe temp.exe FluentCommandLineParser.dll Mono.Data.Sqlite.dll Mono.Security.dll Orcus.Server.Core.dll Orcus.Shared.dll ICSharpCode.SharpZipLib.dll NLog.dll"
DEL temp.exe