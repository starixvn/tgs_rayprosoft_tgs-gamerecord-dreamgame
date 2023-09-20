#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.DreamGame/Tgs.GameRecord.DreamGame.csproj", "Tgs.GameRecord.DreamGame/"]
COPY ["Tgs.GameRecord.DreamGame.Core/Tgs.GameRecord.DreamGame.Core.csproj", "Tgs.GameRecord.DreamGame.Core/"]
RUN dotnet restore "Tgs.GameRecord.DreamGame/Tgs.GameRecord.DreamGame.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.DreamGame"
RUN dotnet build "Tgs.GameRecord.DreamGame.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.GameRecord.DreamGame.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.DreamGame.dll"]