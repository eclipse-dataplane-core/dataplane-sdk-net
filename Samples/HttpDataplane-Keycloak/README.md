# Example: HTTP Dataplane with Keycloak

This example implements a data plane using HTTP as data transport, that fetches data from a private backend API and
makes it available via a public endpoint.
Keycloak is used as authentication provider for the Dataplane Signaling Protocol between data plane and control plane.

## Overview

The project implements a DataPlane application that handles data transfer operations via the Dataplane Signaling
Protocol (DSP) with secure authentication using Keycloak.

Typically, a data plane that implements the DSP protocol communicates with a control plane that sends the DSP
messages. Here, the "control plane" is implemented by sending plain HTTP requests, see the
accompanying [Postman collection](./Resources/Postman) for details.

## Getting started

The easiest way to run this example is to use Docker Compose:

```shell
NUGET_USERNAME=<user> NUGET_PASSWORD=<github-pat> docker compose up --build
```

The `NUGET_USERNAME` and `NUGET_PASSWORD` environment variables are required to download the required NuGet packages.
Please refer
to [the GitHub documentation](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry#authenticating-to-the-container-registry)
for details.

## Application configuration

If Docker Compose is used, the Keycloak instance is configured automatically mapping/importing the
provided [Keycloak Realm file](./Resources/Keycloak/dataplane-api-realm.json).

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

## Transferring data to consumers via the Public API

In addition to the DPS API, which is used by the control plane to interact with the data plane, the data plane also
exposes an HTTP endpoint which data consumers use to transfer data. We'll call this API the "public API" of the data
plane, and of course it needs to be secured as well.

In the `DataFlowStartMessage` message, the control plane sends the source data address of the data set that should be
transmitted. In this example, this is a demo HTTP URL:

```json lines
{
  // ...
  "sourceDataAddress": {
    "@type": "HttpData",
    "properties": {
      "baseUrl": "https://jsonplaceholder.typicode.com/comments/42",
    }
  }
}

```

and the data plane responds with a `DataFlowResponseResponse` message, which contains the HTTP endpoint address where
the data can be consumed plus an API key (`"token"` field).

### Public API endpoint authentication

Everytime the data plane receives a start command from the control plane, it generates an API key with which the data
consumer can query the public API. Note that this API key can only be used for one particular data flow.

The following is an example response from the data plane's DPS endpoint:

```json lines
{
  "dataFlowId": "test-process2",
  "dataAddress": {
    "properties": {
      "url": "http://localhost:8080/api/v1/public/test-process2",
      "token": "79c0476c-b1c5-4809-9659-1171089e1a56"
    },
    "@id": "d20c8265-9bd7-4c63-b43d-57e0d1fe135d",
    // ... other data address fields
  },
  // ... other response fields
}
```

With this, the data consumer can now use the `url` and `token` fields to transfer data to the data plane:

```shell
curl -X GET 'http://localhost:8080/api/v1/public/test-process2' -H 'x-api-key: 79c0476c-b1c5-4809-9659-1171089e1a56'
```

That way, the Public API acts as _a proxy for the (potentially private) source data address._

## Disclaimer

Do **NOT** use this example in production environments. It foregoes most security measures, intentionally violates
best practices and takes many short-cuts to keep the example simple and clear.
