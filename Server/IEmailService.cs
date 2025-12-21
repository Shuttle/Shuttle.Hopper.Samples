namespace Server;

public interface IEmailService
{
    Task SendAsync(Guid id);
}