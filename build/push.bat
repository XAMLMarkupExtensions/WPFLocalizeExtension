@echo off

set PWD="%CD%"
set NUGET=%PWD%\NuGet.exe

set /P APIKEY=Enter your Api Key from https://www.nuget.org/account/ApiKeys: 

if "%APIKEY%"=="" GOTO ERROR

%NUGET% push %PWD%\..\*.nupkg %APIKEY% -Source https://www.nuget.org/api/v2/package
GOTO END

:ERROR
echo EXIT: No api key was entered

:END