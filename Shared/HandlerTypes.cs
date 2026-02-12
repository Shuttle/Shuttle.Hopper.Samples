using Spectre.Console;

namespace Shared;

public class HandlerTypes
{
    public static List<HandlerType> All = [HandlerType.DelegateMessage, HandlerType.DelegateContextMessage, HandlerType.ClassMessage, HandlerType.ClassContextMessage];

    public static HandlerType Select()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<HandlerType>()
                .Title("How should messages be handled:")
                .AddChoices(All)
                .UseConverter(handlerType => handlerType switch
                {
                    HandlerType.DelegateMessage => Colors.Apply("Delegate (message)", handlerType),
                    HandlerType.DelegateContextMessage => Colors.Apply("Delegate (context message)", handlerType),
                    HandlerType.ClassMessage => Colors.Apply("Class (message)", handlerType),
                    HandlerType.ClassContextMessage => Colors.Apply("Class (context message)", handlerType),
                    _ => throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType, null)
                })
        );
    }
}