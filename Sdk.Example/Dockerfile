FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["Sdk.Example/Sdk.Example.csproj", "Sdk.Example/"]
COPY ["Sdk.Core/Sdk.Core.csproj", "Sdk.Core/"]
RUN dotnet restore "Sdk.Example/Sdk.Example.csproj"
COPY . .
WORKDIR "/src/Sdk.Example"
RUN dotnet build "./Sdk.Example.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Sdk.Example.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sdk.Example.dll"]
