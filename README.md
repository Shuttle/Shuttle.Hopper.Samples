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

## OpenTelemetry

`Client`, `Server` and `Subscriber` are all wired up with `Shuttle.Hopper.OpenTelemetry` and `Shuttle.Pipelines.OpenTelemetry` (see `Shared/OpenTelemetryExtensions.cs`), exporting both traces and metrics over OTLP to whatever is listening at the `OpenTelemetry:Endpoint` setting in each app's `appsettings.json` (defaults to `http://localhost:4317`).

Start a local, free, open-source OTLP collector and viewer with:

```bash
docker compose up -d
```

This runs [`grafana/otel-lgtm`](https://github.com/grafana/docker-otel-lgtm), a single container bundling an OTLP collector with Grafana (traces via Tempo, metrics via Prometheus, logs via Loki) for viewing what comes in. Once it's up, open [http://localhost:3000](http://localhost:3000) (user `admin`, password `admin`) and use **Explore** to query:

- Traces in the **Tempo** data source - search by service name (`Shuttle.Hopper.Samples.Client` / `.Server` / `.Subscriber`) to see the pipeline-level and per-message spans, including the send → receive trace linkage across processes.
- Metrics in the **Prometheus** data source - metric names are prefixed `hopper_*` (from `Shuttle.Hopper.OpenTelemetry`) and `pipelines_*` (from `Shuttle.Pipelines.OpenTelemetry`) once Prometheus's dot-to-underscore naming conversion is applied.

Run `docker compose down` to stop it when you're done.