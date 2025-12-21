using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.MessageHandlers;

public class RequestMessageHandler : IMessageHandler<RequestMessage>
{
    public async Task ProcessMessageAsync(IHandlerContext<RequestMessage> context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/message/{nameof(RequestMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", HandlerType.ClassMessage)}");

        await context.SendAsync(new ResponseMessage
        {
            Id = context.Message.Id
        }, messageBuilder => messageBuilder.Reply(), cancellationToken);
    }
}