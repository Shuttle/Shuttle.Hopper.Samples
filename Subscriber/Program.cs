using Messages.v1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;
using Shuttle.Core.Contract;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.SqlServer.Subscription;
using Spectre.Console;

namespace Subscriber;

internal class Program
{
    private static async Task Main()
    {
        await Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddUserSecrets<Program>()
                    .Build();

                services
                    .AddSingleton<IConfiguration>(configuration)
                    .AddHopper(hopperBuilder =>
                    {
                        configuration.GetSection(HopperOptions.SectionName).Bind(hopperBuilder.Options);

                        hopperBuilder
                            .UseAzureStorageQueues(builder =>
                            {
                                builder.AddOptions("hopper-samples", new()
                                {
                                    ConnectionString = Guard.AgainstEmpty(configuration.GetConnectionString("Azurite"))
                                });
                            })
                            .UseSqlServerSubscription(builder =>
                            {
                                builder.Options.ConnectionString = configuration.GetConnectionString("Hopper")!;
                            })
                            .AddSubscription<MessagePublished>()
                            .AddMessageHandler(async (MessagePublished message) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/direct message/{nameof(MessagePublished)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", HandlerType.DelegateDirectMessage)}");

                                await Task.CompletedTask;
                            });
                    });
            })
            .Build()
            .RunAsync();
    }
}