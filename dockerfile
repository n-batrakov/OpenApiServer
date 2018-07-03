FROM microsoft/dotnet:runtime-deps AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:latest AS restore
WORKDIR /sln
COPY . .
RUN dotnet restore -r linux-x64

FROM restore as build
WORKDIR /sln/src/App
RUN dotnet publish -c Release -r linux-x64 --self-contained -o publish -nowarn:msb3202,nu1503,cs1591 --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /sln/src/App/publish .
CMD ["./ITExpert.OpenApi"]