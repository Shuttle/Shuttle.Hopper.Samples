namespace Client;

public class Command(string description, string color, int displayOrder)
{
    public string Description { get; } = description;
    public string Color { get; } = color;
    public int DisplayOrder { get; } = displayOrder;
}