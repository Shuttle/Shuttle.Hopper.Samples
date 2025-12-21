namespace Messages.v1;

public class RequestMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
}