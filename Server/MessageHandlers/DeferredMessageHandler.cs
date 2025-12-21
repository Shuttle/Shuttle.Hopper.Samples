using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.MessageHandlers;

public class DeferredMessageHandler : IMessageHandler<DeferredMessage>
{
    public Task ProcessMessageAsync(IHandlerContext<DeferredMessage> context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/message/{nameof(DeferredMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", HandlerType.ClassMessage)}");

        return Task.CompletedTask;
    }
}