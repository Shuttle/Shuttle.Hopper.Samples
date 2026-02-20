using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.ContextMessageHandlers;

public class DeferredMessageHandler : IContextMessageHandler<DeferredMessage>
{
    public Task HandleAsync(IHandlerContext<DeferredMessage> context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/context message/{nameof(DeferredMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", HandlerType.ClassContextMessage)}");

        return Task.CompletedTask;
    }
}