@echo off
setlocal
rem Compile qp.cs to dist\qp.exe with the version from VERSION stamped in.
rem Uses the C# compiler that ships inside Windows, so no SDK is needed.

set "ROOT=%~dp0"
set "CSC=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo build: csc.exe not found under %SystemRoot%\Microsoft.NET
    exit /b 1
)

set /p VERSION=<"%ROOT%VERSION"
echo %VERSION%| findstr /r /x "[0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*" >nul || (
    echo build: VERSION must be MAJOR.MINOR.PATCH with nothing else on the line
    exit /b 1
)
if not exist "%ROOT%dist" mkdir "%ROOT%dist"

set "VERSION_CS=%ROOT%dist\Version.cs"
> "%VERSION_CS%" echo using System.Reflection;
>> "%VERSION_CS%" echo [assembly: AssemblyTitle("QuickPrompt")]
>> "%VERSION_CS%" echo [assembly: AssemblyProduct("QuickPrompt")]
>> "%VERSION_CS%" echo [assembly: AssemblyDescription("Open Claude Code in the current folder with a prompt already sent.")]
>> "%VERSION_CS%" echo [assembly: AssemblyVersion("%VERSION%")]
>> "%VERSION_CS%" echo [assembly: AssemblyFileVersion("%VERSION%")]
>> "%VERSION_CS%" echo [assembly: AssemblyInformationalVersion("%VERSION%")]

"%CSC%" -nologo -target:exe -platform:anycpu -optimize+ -out:"%ROOT%dist\qp.exe" "%ROOT%qp.cs" "%VERSION_CS%" || exit /b 1
echo Built dist\qp.exe %VERSION%
