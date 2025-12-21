using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.MessageHandlers;

public class StreamMessageHandler : IMessageHandler<StreamMessage>
{
    public Task ProcessMessageAsync(IHandlerContext<StreamMessage> context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/message/{nameof(StreamMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}' / index = {context.Message.Index}", HandlerType.ClassDirectMessage)}");

        return Task.CompletedTask;
    }
}