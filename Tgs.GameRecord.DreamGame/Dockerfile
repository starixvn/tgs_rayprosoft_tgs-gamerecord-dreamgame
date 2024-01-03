#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.DreamGame/Tgs.GameRecord.DreamGame.csproj", "Tgs.GameRecord.DreamGame/"]
COPY ["Tgs.GameRecord.DreamGame.Common/Tgs.GameRecord.DreamGame.Common.csproj", "Tgs.GameRecord.DreamGame.Common/"]
COPY ["Tgs.GameRecord.DreamGame.Core/Tgs.GameRecord.DreamGame.Core.csproj", "Tgs.GameRecord.DreamGame.Core/"]
RUN dotnet restore "./Tgs.GameRecord.DreamGame/./Tgs.GameRecord.DreamGame.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.DreamGame"
RUN dotnet build "./Tgs.GameRecord.DreamGame.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GameRecord.DreamGame.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.DreamGame.dll"]