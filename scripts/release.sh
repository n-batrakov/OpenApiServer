#!/bin/bash

runtimes=(win-x64 linux-x64 osx-x64)
version=$(cat .version)
mkdir -p dist/zip

for runtime in ${runtimes[@]}; do
    name=oas-${version}-${runtime}

    dotnet publish ./src/App -r $runtime /property:Version=$version -o ../../dist/$runtime/oas

    cd ./dist/$runtime

    if [ $runtime = win-x64 ]; then
        zip -r ../zip/$name.zip oas
    else 
        tar -zcvf ../zip/$name.tar.gz oas
    fi
    
    cd ../..
done