#!/bin/bash

VERSION=$1
PACKAGE=$2
OS=$3
OSVERSION=$4

FRDNROOT=/opt/data/FaceRecognitionDotNet
NUGETDIR=${FRDNROOT}//nuget

cd ${NUGETDIR}

pwsh ./TestPackage.ps1 $PACKAGE $VERSION $OS $OSVERSION