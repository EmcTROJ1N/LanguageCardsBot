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
COPY ["src/Cards.Presentation/Cards.Presentation.csproj", "src/Cards.Presentation/"]
COPY ["src/Cards.Domain/Cards.Domain.csproj", "src/Cards.Domain/"]
COPY ["src/Cards.Infrastructure/Cards.Infrastructure.csproj", "src/Cards.Infrastructure/"]
RUN dotnet restore "src/Cards.Presentation/Cards.Presentation.csproj" --configfile /src/nuget.config
COPY . .
WORKDIR "/src/src/Cards.Presentation"
RUN dotnet build "./Cards.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cards.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cards.Presentation.dll"]
