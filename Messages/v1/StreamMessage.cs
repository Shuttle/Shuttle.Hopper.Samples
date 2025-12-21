namespace Messages.v1;

public class StreamMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Index { get; set; }
}