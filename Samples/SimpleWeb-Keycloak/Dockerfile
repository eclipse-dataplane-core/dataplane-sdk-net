# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything and restore
# (Assumes this Dockerfile sits in the project directory next to the .csproj)
COPY . .
RUN dotnet restore

# Publish (framework-dependent)
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Configure ASP.NET Core to listen on port 8080 (container default) and set environment
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    APP_DLL=SimpleWeb_Keycloak.dll

# Copy published output
COPY --from=build /app/publish ./

# Expose HTTP
EXPOSE 8080

# Start the app
# Using a shell form to allow $APP_DLL to be configurable without editing the Dockerfile
ENTRYPOINT ["/bin/sh", "-c", "dotnet /app/$APP_DLL"]
