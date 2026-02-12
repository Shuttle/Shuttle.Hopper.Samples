using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;

namespace Server.MessageHandlers;

public class EmailMessageHandler(IEmailService emailService) : IMessageHandler<EmailMessage>
{
    public async Task ProcessMessageAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/direct message/{nameof(EmailMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", HandlerType.ClassMessage)}");
        
        await emailService.SendAsync(message.Id);
    }
}