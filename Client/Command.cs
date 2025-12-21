namespace Client;

public class Command(string description, string color)
{
    public string Description { get; } = description;
    public string Color { get; } = color;
}