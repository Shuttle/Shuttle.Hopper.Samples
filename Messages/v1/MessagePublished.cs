namespace Messages.v1;

public class MessagePublished
{
    public Guid Id { get; set; } = Guid.NewGuid();
}