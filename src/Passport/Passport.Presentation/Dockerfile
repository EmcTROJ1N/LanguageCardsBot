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
COPY ["src/Passport/Passport.Presentation/Passport.Presentation.csproj", "src/Passport/Passport.Presentation/"]
COPY ["src/Passport/Passport.Application/Passport.Application.csproj", "src/Passport/Passport.Application/"]
COPY ["src/Passport/Passport.Domain/Passport.Domain.csproj", "src/Passport/Passport.Domain/"]
COPY ["src/Passport/Passport.Infrastructure/Passport.Infrastructure.csproj", "src/Passport/Passport.Infrastructure/"]
RUN dotnet nuget update source github \
        --username x \
        --password "$GITHUB_TOKEN" \
        --store-password-in-clear-text && \
    dotnet restore "src/Passport/Passport.Presentation/Passport.Presentation.csproj"
COPY . .
WORKDIR "/src/src/Passport/Passport.Presentation"
RUN dotnet build "./Passport.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Passport.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Passport.Presentation.dll"]
