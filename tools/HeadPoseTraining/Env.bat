echo off

@set TrainRate=8
@set Max=5000
@set SUB=_5000
@set DATE=20200510
@set OrgDatasetRoot=D:\Works\Data\300W_LP
@set DatasetRoot=D:\Works\Dataset\300W_LP\FaceRecognitionDotNet\HeadPose
@set Dataset=%DatasetRoot%\%DATE%%SUB%
@set ModelRoot=%Dataset%
@set Output=%DatasetRoot%\%DATE%%SUB%\result
@set Model=%Output%
@set Tolerance=0.001
@set Gamma=0.1
@set Range=10
@set Parameter=krls_%Tolerance%_%Gamma%