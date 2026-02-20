using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.ContextMessageHandlers;

public class PublishMessageHandler : IContextMessageHandler<PublishMessage>
{
    public async Task HandleAsync(IHandlerContext<PublishMessage> context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/context message/{nameof(PublishMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", HandlerType.ClassContextMessage)}");

        await context.PublishAsync(new MessagePublished
        {
            Id = context.Message.Id
        }, cancellationToken);
    }
}