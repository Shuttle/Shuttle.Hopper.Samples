using Messages.v1;
using Shared;
using Shuttle.Hopper;
using Spectre.Console;
using System;

namespace Server.DirectMessageHandlers;

public class EmailMessageHandler(IEmailService emailService) : IDirectMessageHandler<EmailMessage>
{
    public async Task ProcessMessageAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply($"[class/direct message/{nameof(EmailMessage)}] : ", "grey")}{Colors.Apply($"id = '{Markup.Escape(message.Id.ToString())}'", HandlerType.ClassDirectMessage)}");
        
        await emailService.SendAsync(message.Id);
    }
}