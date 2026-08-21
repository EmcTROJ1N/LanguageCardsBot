FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5050

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_TOKEN
WORKDIR /src
COPY ["Directory.Packages.props", "Directory.Build.props", "nuget.config", "stylecop.json", "./"]
COPY ["src/ApiGateway/ApiGateway.csproj", "src/ApiGateway/"]
RUN dotnet nuget update source github \
    --username "x" \
    --password "$GITHUB_TOKEN" \
    --store-password-in-clear-text \
    --configfile /src/nuget.config
RUN dotnet restore "src/ApiGateway/ApiGateway.csproj" --configfile /src/nuget.config
COPY . .
WORKDIR "/src/src/ApiGateway"
RUN dotnet build "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
