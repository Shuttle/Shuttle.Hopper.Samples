using Shared;
using Spectre.Console;

namespace Server;

public class EmailService : IEmailService
{
    public async Task SendAsync(Guid id)
    {
        AnsiConsole.MarkupLine($"{Colors.Apply("[SENDING E-MAIL] : ", "grey")}{Colors.Apply($"id = '{id.ToString()}'", "skyblue1")}");

        await Task.Delay(TimeSpan.FromSeconds(3)); // simulate communication wait time

        AnsiConsole.MarkupLine($"{Colors.Apply("[E-MAIL SENT] : ", "grey")}{Colors.Apply($"id = '{id.ToString()}'", "skyblue1")}");
    }
}