# Example: HTTP Dataplane with Keycloak

This example implements a data plane using HTTP as data transport, and Keycloak as authentication provider.

## Overview

The project implements a DataPlane application that handles data transfer operations via the Dataplane Signaling
Protocol (DSP) with secure authentication using Keycloak.

Typically, a data plane that implements the DSP protocol communicates with a control plane that sends the DSP
messages. Here, the "control plane" is implemented by sending plain HTTP requests, see the
accompanying [Postman collection](./postman) for details.

## Getting started

The easiest way to run this example is to use Docker Compose:

```shell
NUGET_USERNAME=<user> NUGET_PASSWORD=<github-pat> docker compose up --build
```

the `NUGET_USERNAME` and `NUGET_PASSWORD` environment variables are required to download the required NuGet packages.
Please refer
to [the GitHub documentation](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry#authenticating-to-the-container-registry)
for details.

## Application configuration

If Docker Compose is used, the Keycloak instance is configured automatically mapping/importing the
provided [Keycloak Realm file](./keycloak/dataplane-api-realm.json).

In addition, Keycloak is configured to use port 8088 for the HTTP endpoint, rather than the default 8080. This is
because 8080 is the default convention in .NET Core/ASP.NET Core.

The relevant application configuration is located in the [appsettings.json](./appsettings.json) file:

```json
{
  "JwtSettings": {
    "Authority": "http://keycloak:8088/realms/dataplane-signaling-api",
    "Issuer": "http://localhost:8088/realms/dataplane-signaling-api",
    "Audience": "dataplane-signaling-api"
  }
}
```

If configured, the application persists its business objects in a PostgreSQL database. Again, using Docker Compose,
the database is configured automatically.

The relevant application configuration is located in the [appsettings.json](./appsettings.json) file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=SdkApi;Username=postgres;Password=postgres"
  }
}
```

Furthermore, the SDK must be configured to use the PostgreSQL database (rather than in-memory):

```csharp
 var sdk = new DataPlaneSdk
        {
            DataFlowStore = () => CreatePostgres(configuration, config.RuntimeId),
           // other configuration
        };
```

