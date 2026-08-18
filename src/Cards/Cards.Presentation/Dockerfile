FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_TOKEN
WORKDIR /src

COPY ["Directory.Packages.props", "Directory.Build.props", "nuget.config", "stylecop.json", "./"]

COPY ["src/Cards/Cards.Presentation/Cards.Presentation.csproj", "src/Cards/Cards.Presentation/"]
COPY ["src/Cards/Cards.Domain/Cards.Domain.csproj", "src/Cards/Cards.Domain/"]
COPY ["src/Cards/Cards.Infrastructure/Cards.Infrastructure.csproj", "src/Cards/Cards.Infrastructure/"]

RUN dotnet nuget update source github \
    --username "x" \
    --password "$GITHUB_TOKEN" \
    --store-password-in-clear-text \
    --configfile /src/nuget.config

RUN dotnet restore "src/Cards/Cards.Presentation/Cards.Presentation.csproj" --configfile /src/nuget.config
COPY . .
WORKDIR "/src/src/Cards/Cards.Presentation"
RUN dotnet build "./Cards.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cards.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cards.Presentation.dll"]
