@echo off
:BEGIN
fxc /T fx_5_0 /Fc Error.txt %1
choice /M "Recompile this shader?"

rem The following lovely command print out the last errorlevel
rem echo.%ERRORLEVEL%

if %ERRORLEVEL% equ 1 goto BEGIN
if %ERRORLEVEL% equ 2 goto END

cls
:END