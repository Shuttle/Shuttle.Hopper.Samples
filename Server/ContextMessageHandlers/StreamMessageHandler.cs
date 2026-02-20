using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.ContextMessageHandlers;

public class StreamMessageHandler : IContextMessageHandler<StreamMessage>
{
    public Task HandleAsync(IHandlerContext<StreamMessage> context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/context message/{nameof(StreamMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}' / index = {context.Message.Index}", HandlerType.ClassMessage)}");

        return Task.CompletedTask;
    }
}