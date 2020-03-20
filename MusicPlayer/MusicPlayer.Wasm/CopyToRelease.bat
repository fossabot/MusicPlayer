@echo off

set projectFolder=.
set outputFolder=.\bin\Upload

echo.^/----------------------------^\
echo.^| Get managedpath value ...  ^|
echo.^\----------------------------^/
echo.

echo Read config File ...
for /f "tokens=*" %%a in (%projectFolder%\bin\Debug\netstandard2.0\dist\uno-config.js) do (
    echo.%%a | findstr /C:"uno_remote_managedpath">nul && (
        echo Found managedpath parameter
        for /f "tokens=1-2 delims=()= " %%A in ("%%a") do (
            set managedpath=%%~B
        )
    )
)

echo Extracting managedpath value ...
set managedpath=%managedpath:";=%
echo managedpath = "%managedpath%"

echo.
echo.^/----------------------------------^\
echo.^| ^Copy all files to Production ... ^|
echo.^\----------------------------------^/
echo.

dir /b /a:d "%projectFolder%\bin\Debug\netstandard2.0\dist"|findstr /b "managed-" >"%temp%\managed-files.tmp"
xcopy "%projectFolder%\bin\Debug\netstandard2.0\dist" "%outputFolder%\" /exclude:%temp%\managed-files.tmp /y /s
xcopy "%projectFolder%\bin\Debug\netstandard2.0\dist\%managedpath%" "%outputFolder%\managed\" /y /s

echo.
echo.^/-------------------------------^\
echo.^| ^Replace managedpath value ... ^|
echo.^\-------------------------------^/

call :FindReplace "%managedpath%" "managed" "%outputFolder%\uno-config.js"
call :FindReplace "%managedpath%" "managed" "%outputFolder%\service-worker.js"

echo.
echo.^/--------------------^\
echo.^| Fix wrong Logo ... ^|
echo.^\--------------------^/

call :FindReplace "https://nv-assets.azurewebsites.net/logos/uno.png" "./Assets/Logo.svg" "%outputFolder%\uno-bootstrap.js"

echo.
echo.^/------------------------------------------^\
echo.^| Project has been copied successfully ... ^|
echo.^\------------------------------------------^/
echo.

pause

exit /b 

:FindReplace <findstr> <replstr> <file>
set tmp="%temp%\tmp.txt"
If not exist %temp%\_.vbs call :MakeReplace
for /f "tokens=*" %%a in ('dir "%3" /s /b /a-d /on') do (
  for /f "usebackq" %%b in (`Findstr /mic:"%~1" "%%a"`) do (
    echo(&Echo Replacing "%~1" with "%~2" in file %%~nxa
    <%%a cscript //nologo %temp%\_.vbs "%~1" "%~2">%tmp%
    if exist %tmp% move /Y %tmp% "%%~dpnxa">nul
  )
)
del %temp%\_.vbs

exit /b

:MakeReplace
>%temp%\_.vbs echo with Wscript
>>%temp%\_.vbs echo set args=.arguments
>>%temp%\_.vbs echo .StdOut.Write _
>>%temp%\_.vbs echo Replace(.StdIn.ReadAll,args(0),args(1),1,-1,1)
>>%temp%\_.vbs echo end with