using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.DirectMessageHandlers;

public class DeferredMessageHandler : IDirectMessageHandler<DeferredMessage>
{
    public Task ProcessMessageAsync(DeferredMessage message, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/direct message/{nameof(DeferredMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", HandlerType.ClassDirectMessage)}");

        return Task.CompletedTask;
    }
}