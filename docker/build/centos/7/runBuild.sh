#!/bin/bash

Source=$1
TARGET=$2
ARCH=$3
PLATFORM=$4
FRDNROOT=/opt/data/FaceRecognitionDotNet

if [ $# -eq 5 ]; then
   OPTION=$5
fi

CONFIG=Release

# create non-root user
NON_ROOT_USER=user
USER_ID=${LOCAL_UID:-9001}
GROUP_ID=${LOCAL_GID:-9001}
echo "Starting with UID : $USER_ID, GID: $GROUP_ID"
useradd -u $USER_ID -o -m $NON_ROOT_USER
groupmod -g $GROUP_ID $NON_ROOT_USER
export HOME=/home/$NON_ROOT_USER

cd ${FRDNROOT}/src/${Source}
export TARGETRID=centos
exec /usr/local/bin/gosu $NON_ROOT_USER pwsh Build.ps1 ${CONFIG} ${TARGET} ${ARCH} ${PLATFORM} ${OPTION}