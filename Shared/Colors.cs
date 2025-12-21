using Spectre.Console;

namespace Shared;

public static class Colors
{
    public static string Apply(string text, HandlerType handlerType)
    {
        return handlerType switch
        {
            HandlerType.DelegateDirectMessage => $"[paleturquoise1]{text}[/]",
            HandlerType.DelegateMessage => $"[darkseagreen1]{text}[/]",
            HandlerType.ClassDirectMessage => $"[darkseagreen2_1]{text}[/]",
            HandlerType.ClassMessage => $"[palegreen1_1]{text}[/]",
            _ => throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType, null)
        };
    }

    public static string Apply(string text, string color)
    {
        return $"[{color}]{Markup.Escape(text)}[/]";
    }
}