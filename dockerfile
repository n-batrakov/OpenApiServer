FROM microsoft/dotnet:runtime-deps
EXPOSE 80
WORKDIR /app
COPY ./oas .
ENTRYPOINT [ "./oas" ]