using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.MessageHandlers;

public class PublishMessageHandler(IBus bus) : IMessageHandler<PublishMessage>
{
    public async Task HandleAsync(PublishMessage message, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/message/{nameof(PublishMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", HandlerType.ClassMessage)}");

        await bus.PublishAsync(new MessagePublished
        {
            Id = message.Id
        }, cancellationToken);
    }
}