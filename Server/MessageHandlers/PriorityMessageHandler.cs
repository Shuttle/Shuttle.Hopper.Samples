using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.MessageHandlers;

public class PriorityMessageHandler : IMessageHandler<PriorityMessage>
{
    public Task HandleAsync(PriorityMessage message, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/message/{nameof(PriorityMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}' (additional 'priority' inbox)", HandlerType.ClassMessage)}");

        return Task.CompletedTask;
    }
}