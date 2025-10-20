# Sample: Streaming based on NATS

## Description

This sample demonstrates how to use NATS Streaming for reliable messaging and event streaming. It showcases the setup
and usage of NATS Streaming in a simple application as well as the necessary components to integrate it with the data
plane SDK.

Keycloak is used as an authentication provider to authenticate the Dataplane Signaling Protocol between data plane and
control plane.

## Overview

There are several components in this sample:

- the `Provider` project: implements a data plane that publishes messages to a NATS topic as part of a data flow
- the `Consumer` project: implements a data plane that subscribes to a NATS topic
- a `docker-compose.yaml` file: convenience to start all components (NATS, Postgres, Keycloak)
- the `TestRunner` project: contains some tests that are executed by the CI pipeline
-

## Getting Started

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

## Transferring data to consumers via NATS

Both the provider and the consumer are set up to share data based on a NATS topic. Once all components are up and
running, all we need to do is to "simulate" a control plane sending DPS messages to either of them:

1. Send `/prepare` to consumer
2. Send `/start` to provider. Notice, that upon receiving `/start`, the provider will start publishing messages to the
   NATS topic.
3. Send `/:id/started` to consumer. This notifies the consumer that the data flow has started.
4. To verify the data exchange, simply inspect the logs of the consumer container. It should print out something like
   this
   ```text
   info: Consumer.Nats.NatsSubscriber[0]
   Received {"data":"Event 4","num":5}

   info: Consumer.Nats.NatsSubscriber[0]
   Received {"data":"Event 5","num":6}

   info: Consumer.Nats.NatsSubscriber[0]
   Received {"data":"Event 6","num":7}
   ```
5. Send `/:id/terminate` to provider. This interrupts the data flow and stops publishing messages to the NATS topic and
   frees all resources.
6. Send `/:id/terminate` to consumer. This notifies the consumer that the data flow has terminated and frees all
   resources.

Please use the [Postman collection](./Resources/Postman/StreamingPull.postman_collection.json) included with this
sample.

## Disclaimer

Do **NOT** use this example in production environments. It foregoes most security measures, intentionally violates
some best practices and takes many short-cuts to keep the example simple and clear.