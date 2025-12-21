using Spectre.Console;

namespace Shared;

public class HandlerTypes
{
    public static List<HandlerType> All = [HandlerType.DelegateDirectMessage, HandlerType.DelegateMessage, HandlerType.ClassDirectMessage, HandlerType.ClassMessage];

    public static HandlerType Select()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<HandlerType>()
                .Title("How should messages be handled:")
                .AddChoices(All)
                .UseConverter(handlerType => handlerType switch
                {
                    HandlerType.DelegateDirectMessage => Colors.Apply("Delegate (direct message)", handlerType),
                    HandlerType.DelegateMessage => Colors.Apply("Delegate (message)", handlerType),
                    HandlerType.ClassDirectMessage => Colors.Apply("Class (direct message)", handlerType),
                    HandlerType.ClassMessage => Colors.Apply("Class (message)", handlerType),
                    _ => throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType, null)
                })
        );
    }
}