FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["nuget.config", "./"]
COPY [".nuget/local/", ".nuget/local/"]

COPY ["src/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation.csproj", "src/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/"]
COPY ["src/LanguageCardsBot.Contracts.Cards/LanguageCardsBot.Contracts.Cards.csproj", "src/LanguageCardsBot.Contracts.Cards/"]

RUN dotnet restore "src/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation.csproj" --configfile /src/nuget.config

COPY . .

WORKDIR "/src/src/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation"
RUN dotnet build "./LanguageCardsBot.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LanguageCardsBot.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app/

COPY --from=publish /app/publish .

RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "LanguageCardsBot.Presentation.dll"]