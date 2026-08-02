using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shuttle.Hopper.OpenTelemetry;
using Shuttle.Pipelines;
using Shuttle.Pipelines.OpenTelemetry;

namespace Shared;

public static class OpenTelemetryExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Wires up Shuttle.Pipelines.OpenTelemetry metrics (HopperBuilder.AddOpenTelemetry() already
        /// covers HopperOptions metrics + pipeline tracing) and registers the OpenTelemetry SDK itself,
        /// exporting both signals to the OTLP collector started by docker-compose.yml.
        /// </summary>
        public IServiceCollection AddSampleOpenTelemetry(IConfiguration configuration, string serviceName)
        {
            services.AddOptions<PipelineOptions>().Configure(options => options.AddOpenTelemetryMetrics());

            var endpoint = new Uri(configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddSource(HopperTelemetry.ActivitySourceName)
                    .AddOtlpExporter(otlp => otlp.Endpoint = endpoint))
                .WithMetrics(metrics => metrics
                    .AddMeter(HopperTelemetry.MeterName)
                    .AddMeter(PipelineTelemetry.MeterName)
                    .AddOtlpExporter(otlp => otlp.Endpoint = endpoint));

            return services;
        }
    }
}
