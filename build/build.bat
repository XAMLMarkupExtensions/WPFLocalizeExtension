@echo off

echo **********************************************
echo **********************************************
echo ************ BUILD AS RELEASE ****************
echo **********************************************
echo **********************************************
pause

set PWD="%CD%"
set NUGET=%PWD%\NuGet.exe

DEL %PWD%\..\*.nupkg

%NUGET% pack -Verbosity detailed -OutputDirectory %PWD%\.. %PWD%\..\WpfLocalizeExtension.nuspec