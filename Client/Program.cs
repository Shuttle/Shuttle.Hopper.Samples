using Messages.v1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Spectre.Console;

namespace Client;

internal class Program
{
    private static async Task Main()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddServiceBus(builder =>
            {
                configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);
            })
            .AddAzureStorageQueues(builder =>
            {
                builder.AddOptions("azure", new()
                {
                    ConnectionString = "UseDevelopmentStorage=true;"
                });
            });

        var commandOptions = new Dictionary<string, Command>
        {
            ["deferred"] = new("Send a deferred message (waits 5 seconds)", "wheat4", 1),
            ["email"] = new("Send simulated e-mail processing (demonstrates dependency injection)", "lightslategrey", 1),
            ["request"] = new("Send request message (will receive response)", "darkseagreen", 1),
            ["exit"] = new("(exit)", "darkmagenta", 100)
        };

        var selectedKey = string.Empty;

        await using var serviceBus = await services.BuildServiceProvider().GetRequiredService<IServiceBus>().StartAsync();

        while (selectedKey != "exit")
        {
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
            }
        }
    }
}