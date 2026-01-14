FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EnglishCardsBot.Presentation/EnglishCardsBot.Presentation/EnglishCardsBot.Presentation.csproj", "src/EnglishCardsBot.Presentation/EnglishCardsBot.Presentation/"]
COPY ["src/EnglishCardsBot.Application/EnglishCardsBot.Application/EnglishCardsBot.Application.csproj", "src/EnglishCardsBot.Application/EnglishCardsBot.Application/"]
COPY ["src/EnglishCardsBot.Domain/EnglishCardsBot.Domain/EnglishCardsBot.Domain.csproj", "src/EnglishCardsBot.Domain/EnglishCardsBot.Domain/"]
COPY ["src/EnglishCardsBot.Infrastructure/EnglishCardsBot.Infrastructure/EnglishCardsBot.Infrastructure.csproj", "src/EnglishCardsBot.Infrastructure/EnglishCardsBot.Infrastructure/"]
RUN dotnet restore "src/EnglishCardsBot.Presentation/EnglishCardsBot.Presentation/EnglishCardsBot.Presentation.csproj"
COPY . .
WORKDIR "/src/src/EnglishCardsBot.Presentation/EnglishCardsBot.Presentation"
RUN dotnet build "EnglishCardsBot.Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnglishCardsBot.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/data
ENTRYPOINT ["dotnet", "EnglishCardsBot.Presentation.dll"]

