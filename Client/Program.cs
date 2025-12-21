using Messages.v1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.Kafka;
using Spectre.Console;

namespace Client;

internal class Program
{
    private static async Task Main()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddKafka(builder =>
            {
                builder.AddOptions("local", new()
                {
                    BootstrapServers = "localhost:9092",
                    EnableAutoCommit = true,
                    EnableAutoOffsetStore = true,
                    NumPartitions = 1,
                    UseCancellationToken = false,
                    ConsumeTimeout = TimeSpan.FromMilliseconds(25)
                });
            })
            .AddServiceBus(builder =>
            {
                configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);

                builder
                    .AddMessageHandler((ResponseMessage message) =>
                    {
                        AnsiConsole.MarkupLine($"{Colors.Apply($"[{nameof(ResponseMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", HandlerType.ClassDirectMessage)}");

                        return Task.CompletedTask;
                    });
            })
            .AddAzureStorageQueues(builder =>
            {
                builder.AddOptions("hopper-samples", new()
                {
                    ConnectionString = "UseDevelopmentStorage=true;"
                });
            });

        var commandOptions = new Dictionary<string, Command>
        {
            ["deferred"] = new("Send a deferred message (waits 5 seconds)", "wheat4"),
            ["email"] = new("Send simulated e-mail processing (demonstrates dependency injection)", "lightslategrey"),
            ["request"] = new("Send request message (will receive response)", "darkseagreen"),
            ["publish"] = new("Send publish message (the published message will be handled by the subscriber)", "wheat1"),
            ["stream"] = new("Produce stream messages", "darkolivegreen1_1"),
            ["exit"] = new("(exit)", "darkmagenta")
        };

        var selectedKey = string.Empty;

        await using var serviceBus = await services.BuildServiceProvider().GetRequiredService<IServiceBus>().StartAsync();

        while (selectedKey != "exit")
        {
            AnsiConsole.Clear();

            switch (selectedKey)
            {
                case "deferred":
                {
                    Show("Sent a 'DeferredMessage`...");
                    break;
                }
                case "email":
                {
                    Show("Sent an 'EmailMessage`...");
                    break;
                }
                case "request":
                {
                    Show("Sent a 'RequestMessage`...");
                    break;
                }
                case "publish":
                {
                    Show("Sent a 'PublishMessage`...");
                    break;
                }
                case "stream":
                {
                    Show("Produced 'StreamMessage` instances...");
                    break;
                }
            }

            selectedKey = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the type of message to send:")
                    .AddChoices(commandOptions.Keys)
                    .UseConverter(key => Colors.Apply(commandOptions[key].Description, commandOptions[key].Color))
            );

            switch(selectedKey)
            {
                case "deferred":
                {
                    await serviceBus.SendAsync(new DeferredMessage(), builder => builder.Defer(DateTime.Now.AddSeconds(5)));
                    break;
                }
                case "email":
                {
                    await serviceBus.SendAsync(new EmailMessage());
                    break;
                }
                case "request":
                {
                    await serviceBus.SendAsync(new RequestMessage());
                    break;
                }
                case "publish":
                {
                    await serviceBus.SendAsync(new PublishMessage());
                    break;
                }
                case "stream":
                {
                    for (var i = 1; i < 51; i++)
                    {
                        await serviceBus.SendAsync(new StreamMessage
                        {
                            Index = i
                        });
                    }

                    break;
                }
            }
        }
    }

    private static void Show(string message)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[{DateTime.UtcNow:O}] : ", "grey39")}{Colors.Apply(message, "grey58")}");
    }
}