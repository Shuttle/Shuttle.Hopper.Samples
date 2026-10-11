# Shuttle.Hopper.Samples

Samples that illustrate how to get started with various messaging patterns in Shuttle.Hopper.

## Azurite

This sample makes use of [Shuttle.Hopper.AzureStorageQueues](https://github.com/Shuttle/Shuttle.Hopper.AzureStorageQueues) for the message queues.  Local Azure Storage Queues should be provided by [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite).

## Additional inbox

The `Server` processes an additional inbox named `priority` next to its primary inbox. It is registered using `AddInbox("priority")`, and its work queue is configured under `Shuttle:Hopper:AdditionalInboxes` in the `Server` project's `appsettings.json`:

```json
"AdditionalInboxes": {
  "priority": {
    "WorkTransportUri": "azuresq://hopper-samples/hopper-server-priority-work",
    "ThreadCount": 1
  }
}
```

The `Client` sends a `PriorityMessage` using the `priority` command. A message route in the `Client` project's `appsettings.json` sends it to the `hopper-server-priority-work` queue, where the additional inbox's own processor threads handle it. The primary inbox remains the server's identity, so replies and published events still use `hopper-server-work`.

A second additional inbox named `stream`, registered using `AddInbox("stream")` and configured next to `priority` under `AdditionalInboxes`, consumes the `kafka://local/stream-consumer-work` topic. Neither additional inbox sets its own `DeferredTransportUri` or `ErrorTransportUri`, so both use those of the primary inbox. The `Client` routes `StreamMessage` to it using the `stream` command, which shows that an additional inbox may use a different transport (Kafka) from the primary inbox (Azure Storage Queues).

## Kafka

The `stream` additional inbox makes use of [Shuttle.Hopper.Kafka](https://github.com/Shuttle/Shuttle.Hopper.Kafka) and therefore requires a running [Kafka](https://kafka.apache.org/) broker for both the `Client` and the `Server`, since the `Server` always registers the `stream` inbox.  A local broker may be started using `kafka.cmd`:

```
docker run -d --name broker -p 9092:9092 apache/kafka:latest
```

## Sql Server

You will also need to create and configure a Sql Server database for the Publish/Subscribe sample.

```
docker run --network development --restart always -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<password>" -p 1433:1433 --name sql --hostname sql -v c:\sql.data:/var/opt/mssql/data -d mcr.microsoft.com/mssql/server:2022-latest
```

> Create a new database called **Hopper**

## Server / Subscriber

Right-click on the `Server` project and select `Manage User Secrets`.

Add the following connection string to the `Hopper` database:

```json
{
  "ConnectionStrings": {
    "Hopper": "server=.;database=Hopper;user id=sa;password=<password>;TrustServerCertificate=true"
  }
}
```

Do the same for the `Subscriber` project.