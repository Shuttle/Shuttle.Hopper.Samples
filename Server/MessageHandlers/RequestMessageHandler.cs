using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.MessageHandlers;

public class RequestMessageHandler(IBus bus) : IMessageHandler<RequestMessage>
{
    public async Task HandleAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/direct message/{nameof(RequestMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", HandlerType.ClassMessage)}");

        await bus.SendAsync(new ResponseMessage
        {
            Id = message.Id
        }, messageBuilder => messageBuilder.WithRecipient("azuresq://hopper-samples/hopper-client-work"), cancellationToken);
    }
}