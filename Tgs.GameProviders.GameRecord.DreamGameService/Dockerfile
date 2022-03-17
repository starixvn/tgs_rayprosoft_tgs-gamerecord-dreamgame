#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
RUN dotnet nuget remove source 'Microsoft Visual Studio Offline Packages' || true
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["Tgs.GameProviders.GameRecord.DreamGameService/Tgs.GameProviders.GameRecord.DreamGameService.csproj", "Tgs.GameProviders.GameRecord.DreamGameService/"]
RUN dotnet restore "Tgs.GameProviders.GameRecord.DreamGameService/Tgs.GameProviders.GameRecord.DreamGameService.csproj"
COPY . .
WORKDIR "/src/Tgs.GameProviders.GameRecord.DreamGameService"
RUN dotnet build "Tgs.GameProviders.GameRecord.DreamGameService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.GameProviders.GameRecord.DreamGameService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameProviders.GameRecord.DreamGameService.dll"]