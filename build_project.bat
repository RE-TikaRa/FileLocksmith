@echo off
setlocal

pushd "%~dp0"

set "VSWHERE_DIR=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer"
if not exist "%VSWHERE_DIR%\vswhere.exe" goto :vswhere_missing

pushd "%VSWHERE_DIR%"
for /f "usebackq delims=" %%i in (`"vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do set "VSINSTALL=%%i"
popd
if "%VSINSTALL%"=="" goto :msbuild_install_missing

set "MSBUILD=%VSINSTALL%\MSBuild\Current\Bin\MSBuild.exe"
if not exist "%MSBUILD%" goto :msbuild_missing

"%MSBUILD%" src\modules\FileLocksmith\FileLocksmithUI\FileLocksmithUI.csproj /restore /p:Configuration=Release /p:Platform=x64 /p:RunAnalyzers=false /p:RunCodeAnalysis=false /p:EnableNETAnalyzers=false /p:EnforceCodeStyleInBuild=false /p:TreatWarningsAsErrors=false
if errorlevel 1 exit /b 1

popd
endlocal
exit /b 0

:vswhere_missing
echo VSWHERE not found: %VSWHERE_DIR%\vswhere.exe
exit /b 1

:msbuild_install_missing
echo MSBuild install not found.
exit /b 1

:msbuild_missing
echo MSBuild not found: %MSBUILD%
exit /b 1
