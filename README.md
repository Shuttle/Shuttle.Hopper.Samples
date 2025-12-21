# Shuttle.Hopper.Samples

Samples that illustrate how to get started with various messaing patterns in Shuttle.Hopper.

## Azurite

This sample makes use of [Shuttle.Esb.AzureStorageQueues](https://github.com/Shuttle/Shuttle.Esb.AzureStorageQueues) for the message queues.  Local Azure Storage Queues should be provided by [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite).

## Kafka

The streaming sample makes use of [Kafka](https://kafka.apache.org/).

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