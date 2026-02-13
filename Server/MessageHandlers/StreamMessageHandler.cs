using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.MessageHandlers;

public class StreamMessageHandler : IMessageHandler<StreamMessage>
{
    public Task HandleAsync(StreamMessage message, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/direct message/{nameof(StreamMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}' / index = {message.Index}", HandlerType.ClassMessage)}");

        return Task.CompletedTask;
    }
}