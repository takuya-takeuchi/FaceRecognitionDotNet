#!/bin/bash

VERSION=$1
PACKAGE=$2
OS=$3
OSVERSION=$4

FRDNROOT=/opt/data/FaceRecognitionDotNet
WORK=/opt/data/work
TESTDIR=${FRDNROOT}/nuget/artifacts/test/${PACKAGE}.${VERSION}/${OS}/${OSVERSION}


mkdir -p ${WORK}
mkdir -p ${TESTDIR}    

cp -Rf ${FRDNROOT}/test/FaceRecognitionDotNet.Tests ${WORK}
cd ${WORK}/FaceRecognitionDotNet.Tests
  
# delete local project reference
dotnet remove reference ../../src/DlibDotNet/src/DlibDotNet/DlibDotNet.csproj > /dev/null 2>&1
dotnet remove reference ../../src/FaceRecognitionDotNet/FaceRecognitionDotNet.csproj > /dev/null 2>&1

# restore package from local nuget pacakge
# And drop stdout message
dotnet add package $PACKAGE -v $VERSION --source ${FRDNROOT}/nuget/ > /dev/null 2>&1

dotnet test -c Release -r ${TESTDIR} --logger trx

if [ $? -ne 0 ]; then
   exit -1
fi

# move to current
cd $CURDIR

# to make sure, delete
if [ -e ${WORK} ]; then
   rm -Rf ${WORK}
fi