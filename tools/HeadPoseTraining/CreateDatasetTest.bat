@call Env.bat
@set CURRENT=%cd%

@cd tools
pwsh CreateDataset.ps1 %OrgDatasetRoot% %TrainRate% %Output% %Max%

@cd %CURRENT%