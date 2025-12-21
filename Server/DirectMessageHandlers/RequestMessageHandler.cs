using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.DirectMessageHandlers;

public class RequestMessageHandler(IServiceBus serviceBus) : IDirectMessageHandler<RequestMessage>
{
    public async Task ProcessMessageAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/direct message/{nameof(RequestMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", HandlerType.ClassDirectMessage)}");

        await serviceBus.SendAsync(new ResponseMessage
        {
            Id = message.Id
        }, messageBuilder => messageBuilder.WithRecipient("azuresq://hopper-samples/hopper-client-work"), cancellationToken);
    }
}