FROM microsoft/dotnet:runtime-deps
EXPOSE 80
WORKDIR /app
COPY ./dist/docker .
ENTRYPOINT [ "./oas" ]