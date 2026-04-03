using Messages.v1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.MessageHandlers;
using Shared;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.Kafka;
using Shuttle.Hopper.SqlServer.Subscription;
using Spectre.Console;

namespace Server;

internal class Program
{
    private static async Task Main()
    {
        var handlerType = HandlerTypes.Select();

        await Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddUserSecrets<Program>()
                    .Build();

                services
                    .AddSingleton<IConfiguration>(configuration)
                    .AddSingleton<IEmailService, EmailService>();

                var hopperBuilder = services
                    .AddHopper(options =>
                    {
                        configuration.GetSection(HopperOptions.SectionName).Bind(options);

                        options.DeferredMessageProcessingHalted += (eventArgs, _) =>
                        {
                            Console.WriteLine($"[deferred processing halted] : until = {eventArgs.RestartAt}");

                            return Task.CompletedTask;
                        };

                        options.DeferredMessageProcessingAdjusted += (eventArgs, _) =>
                        {
                            Console.WriteLine($"[deferred processing adjusted] : next = {eventArgs.NextProcessingAt}");

                            return Task.CompletedTask;
                        };
                    })
                    .UseAzureStorageQueues(builder =>
                    {
                        builder.Configure("hopper-samples", options =>
                        {
                            options.ConnectionString = configuration.GetConnectionString("Azurite")!;
                        });
                    })
                    .UseKafka(builder =>
                    {
                        builder.Configure("local", options =>
                        {
                            options.BootstrapServers = "localhost:9092";
                            options.EnableAutoCommit = true;
                            options.EnableAutoOffsetStore = true;
                            options.NumPartitions = 1;
                            options.UseCancellationToken = false;
                            options.ConsumeTimeout = TimeSpan.FromMilliseconds(25);
                        });
                    })
                    .UseSqlServerSubscription(options =>
                    {
                        options.ConnectionString = configuration.GetConnectionString("Hopper")!;
                    });

                switch (handlerType)
                {
                    case HandlerType.DelegateMessage:
                    {
                        hopperBuilder
                            .AddMessageHandler((DeferredMessage message) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/direct message/{nameof(DeferredMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", handlerType)}");

                                return Task.CompletedTask;
                            })
                            .AddMessageHandler(async (EmailMessage message, IEmailService emailService) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/direct message/{nameof(EmailMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", handlerType)}");

                                await emailService.SendAsync(message.Id);
                            })
                            .AddMessageHandler(async (RequestMessage message, IBus bus) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/direct message/{nameof(RequestMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", handlerType)}");

                                await bus.SendAsync(new ResponseMessage
                                {
                                    Id = message.Id
                                }, messageBuilder => messageBuilder.WithRecipient("azuresq://hopper-samples/hopper-client-work"));
                            })
                            .AddMessageHandler(async (PublishMessage message, IBus bus) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/direct message/{nameof(PublishMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", handlerType)}");

                                await bus.PublishAsync(new MessagePublished
                                {
                                    Id = message.Id
                                });
                            })
                            .AddMessageHandler((StreamMessage message) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/direct message/{nameof(StreamMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}' / index = {message.Index}", handlerType)}");

                                return Task.CompletedTask;
                            });

                        break;
                    }
                    case HandlerType.DelegateContextMessage:
                    {
                        hopperBuilder
                            .AddMessageHandler((IHandlerContext<DeferredMessage> context) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/message/{nameof(DeferredMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", handlerType)}");

                                return Task.CompletedTask;
                            })
                            .AddMessageHandler(async (IHandlerContext<EmailMessage> context, IEmailService emailService) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/message/{nameof(EmailMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", handlerType)}");

                                await emailService.SendAsync(context.Message.Id);
                            })
                            .AddMessageHandler(async (IHandlerContext<RequestMessage> context) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/message/{nameof(RequestMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", handlerType)}");

                                await context.SendAsync(new ResponseMessage
                                {
                                    Id = context.Message.Id
                                }, messageBuilder => messageBuilder.AsReply());
                            })
                            .AddMessageHandler(async (IHandlerContext<PublishMessage> context) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/direct message/{nameof(PublishMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", handlerType)}");

                                await context.PublishAsync(new MessagePublished
                                {
                                    Id = context.Message.Id
                                });
                            })
                            .AddMessageHandler((IHandlerContext<StreamMessage> context) =>
                            {
                                AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/message/{nameof(StreamMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}' / index = {context.Message.Index}", handlerType)}");

                                return Task.CompletedTask;
                            });

                        break;
                    }
                    case HandlerType.ClassMessage:
                    {
                        hopperBuilder
                            .AddMessageHandler<DeferredMessageHandler>()
                            .AddMessageHandler<EmailMessageHandler>()
                            .AddMessageHandler<RequestMessageHandler>()
                            .AddMessageHandler<PublishMessageHandler>()
                            .AddMessageHandler<StreamMessageHandler>();

                        break;
                    }
                    case HandlerType.ClassContextMessage:
                    {
                        hopperBuilder
                            .AddMessageHandler<ContextMessageHandlers.DeferredMessageHandler>()
                            .AddMessageHandler<ContextMessageHandlers.EmailMessageHandler>()
                            .AddMessageHandler<ContextMessageHandlers.RequestMessageHandler>()
                            .AddMessageHandler<ContextMessageHandlers.PublishMessageHandler>()
                            .AddMessageHandler<ContextMessageHandlers.StreamMessageHandler>();

                        break;
                    }
                }
            })
            .Build()
            .RunAsync();
    }
}