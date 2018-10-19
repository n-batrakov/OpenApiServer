#!/bin/bash

runtimes=(win-x64 linux-x64 osx-x64)
version=$(cat .version)
mkdir -p dist/zip

for runtime in ${runtimes[@]}; do
    name=oas-${version}-${runtime}

    dotnet publish ./src/App -r $runtime /property:Version=$version -o ../../dist/$name

    cd ./dist

    if [ $runtime = win-x64 ]; then
        zip -r ./zip/$name.zip $name
    else 
        tar -zcvf ./zip/$name.tag.gz $name
    fi
    
    cd ..
done