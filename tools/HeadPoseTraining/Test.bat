echo off

@call Env.bat

@set Pose=roll
@set Model=%ModelRoot%\result\300w-lp-%Pose%-%Parameter%.dat
@echo Start %Pose%
dotnet run -c Release -- test --dataset "%Dataset%" ^
                              --model %Model% ^
                              --pose %Pose% ^
                              --range %Range% > "%Output%\test_%Pose%_log.txt" 2>&1

@set Pose=pitch
@set Model=%ModelRoot%\result\300w-lp-%Pose%-%Parameter%.dat
@echo Start %Pose%
dotnet run -c Release -- test --dataset "%Dataset%" ^
                              --model %Model% ^
                              --pose %Pose% ^
                              --range %Range% > "%Output%\test_%Pose%_log.txt" 2>&1

@set Pose=yaw
@set Model=%ModelRoot%\result\300w-lp-%Pose%-%Parameter%.dat
@echo Start %Pose%
dotnet run -c Release -- test --dataset "%Dataset%" ^
                              --model %Model% ^
                              --pose %Pose% ^
                              --range %Range% > "%Output%\test_%Pose%_log.txt" 2>&1
                        