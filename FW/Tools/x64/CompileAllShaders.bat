set SHADERDIRS=..\..\Resources\
REM 
for /f %%a IN ('dir /s /b %SHADERDIRS%\*.fx') do (
	fxc /T fx_5_0 %%a 
)
pause