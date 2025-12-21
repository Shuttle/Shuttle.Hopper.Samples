using Spectre.Console;

namespace Shared;

public class HandlerTypes
{
    public static List<HandlerType> All = [HandlerType.DelegateMessage, HandlerType.DelegateContext, HandlerType.ClassMessage, HandlerType.ClassContext];

    public static HandlerType Select()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<HandlerType>()
                .Title("How should messages be handled:")
                .AddChoices(All)
                .UseConverter(handlerType => handlerType switch
                {
                    HandlerType.DelegateMessage => Colors.Apply("Delegate (Message)", handlerType),
                    HandlerType.DelegateContext => Colors.Apply("Delegate (Context)", handlerType),
                    HandlerType.ClassMessage => Colors.Apply("Class (Message)", handlerType),
                    HandlerType.ClassContext => Colors.Apply("Class (Context)", handlerType),
                    _ => throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType, null)
                })
        );
    }
}