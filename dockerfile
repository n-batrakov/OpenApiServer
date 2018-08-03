FROM microsoft/dotnet:runtime-deps
EXPOSE 80
WORKDIR /oas
COPY ./oas .
CMD ["./ITExpert.OpenApi.Server run --port 80 --config /oas/oas.config.json"]