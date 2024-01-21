#! /usr/bin/env bash
set -uvx
set -e
cwd=`pwd`
#ts=`date "+%Y.%m%d.%H%M.%S"`
#version=`date "+%y.%m.%d.%H%M%S"`
version=`date "+%y.%m.%d.%H%M"`
uuid=`uuidgen`
cd $cwd
rm -rf obj bin
sed -i -e "s/<Version>.*<\/Version>/<Version>${version}<\/Version>/g" mdock.csproj
#dotnet build -f net462 -c Release mdock.csproj
dotnet restore mdock.sln -p:Configuration=Release -p:Platform="Any CPU"
MSBuild.exe mdock.sln -p:Configuration=Release -p:Platform="Any CPU"
cd $cwd
rm -rf vc_redist.x64.*
#wget --no-check-certificate https://aka.ms/vs/17/release/vc_redist.x64.exe
#dark -x vc_redist.x64.tmp vc_redist.x64.exe
rm -rf vc_redist.x86.*
wget --no-check-certificate https://aka.ms/vs/17/release/vc_redist.x86.exe
dark -x vc_redist.x86.tmp vc_redist.x86.exe
#cd $cwd/bin/x64/Release/net462/
cd $cwd/bin/Release/net462/
#7z x -y $cwd/vc_redist.x64.tmp/AttachedContainer/packages/vcRuntimeMinimum_amd64/cab1.cab
#for f in *.dll_amd64; do
#  mv $f ${f%.*}.dll
#done
7z x -y $cwd/vc_redist.x86.tmp/AttachedContainer/packages/vcRuntimeMinimum_x86/cab1.cab
for f in *.dll_x86; do
  mv $f ${f%.*}.dll
done
#rm -rf C:/Users/user/Desktop/mdock
#mkdir -p C:/Users/user/Desktop/mdock
#cp -rp * C:/Users/user/Desktop/mdock/
#cd $cwd
#cp -rp MicrosoftEdgeWebview2Setup.exe C:/Users/user/Desktop/mdock/
#cd C:/Users/user/Desktop/
#rm -rf mdock*.zip
7z a -tzip $USERPROFILE/OneDrive/mdock-v${version}.zip *
#cp -rp mdock-v${version}.zip C:/Users/user/OneDrive/software/
cd $cwd
git add .
git-put
