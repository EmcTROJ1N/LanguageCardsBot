FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Packages.props", "Directory.Build.props", "nuget.config", "stylecop.json", "./"]
COPY [".nuget/local/", ".nuget/local/"]

COPY ["src/LanguageCardsBot/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation.csproj", "src/LanguageCardsBot/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/"]

RUN dotnet restore "src/LanguageCardsBot/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation/LanguageCardsBot.Presentation.csproj" --configfile /src/nuget.config

COPY . .

WORKDIR "/src/src/LanguageCardsBot/EnglishCardsBot.Presentation/LanguageCardsBot.Presentation"
RUN dotnet build "./LanguageCardsBot.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LanguageCardsBot.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app/

COPY --from=publish /app/publish .

RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "LanguageCardsBot.Presentation.dll"]