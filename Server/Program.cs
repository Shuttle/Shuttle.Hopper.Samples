using Messages.v1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Spectre.Console;

namespace Server;

internal class Program
{
    static async Task Main(string[] args)
    {
        var handlerType = HandlerTypes.Select();

        await Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                services
                    .AddSingleton<IConfiguration>(configuration)
                    .AddServiceBus(builder =>
                    {
                        configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);

                        builder.Options.AddMessageHandlers = false;

                        builder.Options.DeferredMessageProcessingHalted += (eventArgs, _) =>
                        {
                            Console.WriteLine($"[deferred processing halted] : until = {eventArgs.RestartDateTime}");

                            return Task.CompletedTask;
                        };

                        builder.Options.DeferredMessageProcessingAdjusted += (eventArgs, _) =>
                        {
                            Console.WriteLine($"[deferred processing adjusted] : next = {eventArgs.NextProcessingDateTime}");

                            return Task.CompletedTask;
                        };

                        switch (handlerType)
                        {
                            case HandlerType.DelegateMessage:
                                {
                                    builder.AddMessageHandler(async (RegisterMember message) =>
                                    {
                                        AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/message/{nameof(RegisterMember)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", handlerType)}");

                                        await Task.CompletedTask;
                                    });
                                    break;
                                }
                            case HandlerType.DelegateContext:
                                {
                                    builder.AddMessageHandler(async (IHandlerContext<RegisterMember> context) =>
                                    {
                                        AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/context/{nameof(RegisterMember)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", handlerType)}");

                                        await Task.CompletedTask;
                                    });
                                    break;
                                }
                        }
                    })
                    .AddAzureStorageQueues(builder =>
                    {
                        builder.AddOptions("azure", new()
                        {
                            ConnectionString = configuration.GetConnectionString("azure")!
                        });
                    });
            })
            .Build()
            .RunAsync();
    }
}
