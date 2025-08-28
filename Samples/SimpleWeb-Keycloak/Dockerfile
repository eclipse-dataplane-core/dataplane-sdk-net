# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything and restore
# (Assumes this Dockerfile sits in the project directory next to the .csproj)
COPY . .

ARG NUGET_USERNAME
ARG NUGET_PASSWORD

RUN dotnet nuget add source "https://nuget.pkg.github.com/metaform/index.json" \
    --name Metaform \
    --username $NUGET_USERNAME \
    --password $NUGET_PASSWORD \
    --store-password-in-clear-text \
 && dotnet restore SimpleWeb-Keycloak.csproj \
 && dotnet nuget remove source Metaform

# Publish (framework-dependent)
RUN dotnet publish SimpleWeb-Keycloak.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Configure ASP.NET Core to listen on port 8080 and set environment
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

# Copy published output
COPY --from=build /app/publish ./

# Expose HTTP
EXPOSE 8080

# Start the app
ENTRYPOINT ["dotnet", "./SimpleWeb-Keycloak.dll"]