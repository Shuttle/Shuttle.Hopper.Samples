using Messages.v1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Spectre.Console;
using System;
using Shuttle.Hopper.Kafka;
using Shuttle.Hopper.SqlServer.Subscription;

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
                    .AddSingleton<IEmailService, EmailService>()
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
                    .AddSqlServerSubscription(builder =>
                    {
                        builder.Options.ConnectionString = configuration.GetConnectionString("Hopper")!;
                    })
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
                            case HandlerType.DelegateDirectMessage:
                            {
                                builder
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
                                    .AddMessageHandler(async (RequestMessage message, IServiceBus serviceBus) =>
                                    {
                                        AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/direct message/{nameof(RequestMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", handlerType)}");

                                        await serviceBus.SendAsync(new ResponseMessage
                                        {
                                            Id = message.Id
                                        }, messageBuilder => messageBuilder.WithRecipient("azuresq://hopper-samples/hopper-client-work"));
                                    })
                                    .AddMessageHandler(async (PublishMessage message, IServiceBus serviceBus) =>
                                    {
                                        AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/direct message/{nameof(PublishMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", handlerType)}");

                                        await serviceBus.PublishAsync(new MessagePublished
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
                            case HandlerType.DelegateMessage:
                            {
                                builder
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
                                        }, messageBuilder => messageBuilder.Reply());
                                    })
                                    .AddMessageHandler((IHandlerContext<StreamMessage> context) =>
                                    {
                                        AnsiConsole.MarkupLine($"{Colors.Apply($"[delegate/message/{nameof(StreamMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}' / index = {context.Message.Index}", handlerType)}");

                                        return Task.CompletedTask;
                                    });

                                    break;
                            }
                            case HandlerType.ClassDirectMessage:
                            {
                                builder
                                    .AddMessageHandler<DirectMessageHandlers.DeferredMessageHandler>()
                                    .AddMessageHandler<DirectMessageHandlers.EmailMessageHandler>()
                                    .AddMessageHandler<DirectMessageHandlers.RequestMessageHandler>()
                                    .AddMessageHandler<DirectMessageHandlers.StreamMessageHandler>();

                                break;
                            }
                            case HandlerType.ClassMessage:
                            {
                                builder
                                    .AddMessageHandler<MessageHandlers.DeferredMessageHandler>()
                                    .AddMessageHandler<MessageHandlers.EmailMessageHandler>()
                                    .AddMessageHandler<MessageHandlers.RequestMessageHandler>()
                                    .AddMessageHandler<MessageHandlers.StreamMessageHandler>();

                                break;
                            }
                        }
                    })
                    .AddAzureStorageQueues(builder =>
                    {
                        builder.AddOptions("hopper-samples", new()
                        {
                            ConnectionString = configuration.GetConnectionString("Azurite")!
                        });
                    });
            })
            .Build()
            .RunAsync();
    }
}