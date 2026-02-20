using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.ContextMessageHandlers;

public class EmailMessageHandler(IEmailService emailService) : IContextMessageHandler<EmailMessage>
{
    public async Task HandleAsync(IHandlerContext<EmailMessage> context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/context message/{nameof(EmailMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(context.Message.Id.ToString())}'", HandlerType.ClassContextMessage)}");

        await emailService.SendAsync(context.Message.Id);
    }
}