echo off

@call Env.bat
@set Model=%Dataset%\result\
@set Output=%Dataset%\result\eval
@set Range=10

@set Image=%OrgDatasetRoot%\AFW\AFW_134212_1_4.jpg
@set Mat=%OrgDatasetRoot%\AFW\AFW_134212_1_4.mat
@set Landmark=%OrgDatasetRoot%\landmarks\AFW\AFW_134212_1_4_pts.mat
dotnet run -c Release -- eval --image "%Image%" ^
                              --mat %Mat% ^
                              --landmark %Landmark% ^
                              --roll "%Model%300w-lp-roll-%Parameter%.dat" ^
                              --pitch "%Model%300w-lp-pitch-%Parameter%.dat" ^
                              --yaw "%Model%300w-lp-yaw-%Parameter%.dat" ^
                              --output %Output%

@set Image=%OrgDatasetRoot%\IBUG\IBUG_image_003_1_5.jpg
@set Mat=%OrgDatasetRoot%\IBUG\IBUG_image_003_1_5.mat
@set Landmark=%OrgDatasetRoot%\landmarks\IBUG\IBUG_image_003_1_5_pts.mat
dotnet run -c Release -- eval --image "%Image%" ^
                              --mat %Mat% ^
                              --landmark %Landmark% ^
                              --roll "%Model%300w-lp-roll-%Parameter%.dat" ^
                              --pitch "%Model%300w-lp-pitch-%Parameter%.dat" ^
                              --yaw "%Model%300w-lp-yaw-%Parameter%.dat" ^
                              --output %Output%

@set Image=%OrgDatasetRoot%\HELEN\HELEN_232194_1_8.jpg
@set Mat=%OrgDatasetRoot%\HELEN\HELEN_232194_1_8.mat
@set Landmark=%OrgDatasetRoot%\landmarks\HELEN\HELEN_232194_1_8_pts.mat
dotnet run -c Release -- eval --image "%Image%" ^
                              --mat %Mat% ^
                              --landmark %Landmark% ^
                              --roll "%Model%300w-lp-roll-%Parameter%.dat" ^
                              --pitch "%Model%300w-lp-pitch-%Parameter%.dat" ^
                              --yaw "%Model%300w-lp-yaw-%Parameter%.dat" ^
                              --output %Output%

@set Image=%OrgDatasetRoot%\LFPW\LFPW_image_test_0003_9.jpg
@set Mat=%OrgDatasetRoot%\LFPW\LFPW_image_test_0003_9.mat
@set Landmark=%OrgDatasetRoot%\landmarks\LFPW\LFPW_image_test_0003_9_pts.mat
dotnet run -c Release -- eval --image "%Image%" ^
                              --mat %Mat% ^
                              --landmark %Landmark% ^
                              --roll "%Model%300w-lp-roll-%Parameter%.dat" ^
                              --pitch "%Model%300w-lp-pitch-%Parameter%.dat" ^
                              --yaw "%Model%300w-lp-yaw-%Parameter%.dat" ^
                              --output %Output%
                        