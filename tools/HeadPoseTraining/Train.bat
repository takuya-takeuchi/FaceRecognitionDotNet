echo off

@call Env.bat

@mkdir "%Output%"
dotnet run -c Release -- train --dataset "%Dataset%" ^
                               --gamma %Gamma% ^
                               --tolerance %Tolerance% ^
                               --range %Range%^
                               --output %Output%
rem                               --output %Output% > "%Output%\log.txt" 2>&1
                        