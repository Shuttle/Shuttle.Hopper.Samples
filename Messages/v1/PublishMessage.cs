namespace Messages.v1;

public class PublishMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
}