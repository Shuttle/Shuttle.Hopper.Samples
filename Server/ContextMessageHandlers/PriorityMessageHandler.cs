using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.ContextMessageHandlers;

public class PriorityMessageHandler : IContextMessageHandler<PriorityMessage>
{
    public Task HandleAsync(IHandlerContext<PriorityMessage> context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/context message/{nameof(PriorityMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}' (additional 'priority' inbox)", HandlerType.ClassContextMessage)}");

        return Task.CompletedTask;
    }
}