using Messages.v1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.Kafka;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace Client;

internal class Program
{
    private static readonly List<LogEntry> LogEntries = [];

    private static ListView _outputListView = null!;
    private static IServiceBus? _serviceBus;

    private static void Log(string message, Color color)
    {
        Application.MainLoop.Invoke(() =>
        {
            LogEntries.Add(new($"[{DateTime.Now:HH:mm:ss}] {message}", color));
            _outputListView.SetSource(LogEntries.ToList());

            if (LogEntries.Count <= 0)
            {
                return;
            }

            _outputListView.SelectedItem = LogEntries.Count - 1;
            _outputListView.EnsureSelectedItemVisible();
        });
    }

    private static void Main()
    {
        Application.Init();

        var defaultScheme = new ColorScheme
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
            Focus = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
            HotNormal = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Black),
            HotFocus = Application.Driver.MakeAttribute(Color.BrightCyan, Color.Gray)
        };

        var top = Application.Top;
        top.ColorScheme = defaultScheme;

        var promptWin = new Window("Message Prompts")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(40),
            ColorScheme = defaultScheme
        };

        var outputWin = new Window("System Output (Press Ctrl+Q to Exit)")
        {
            X = 0,
            Y = Pos.Bottom(promptWin),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = defaultScheme
        };

        var commands = new List<Command>
        {
            new() { Key = "deferred", Description = "Send a deferred message (waits 5 seconds)", Color = Color.Brown },
            new() { Key = "email", Description = "Send simulated e-mail processing (demonstrates dependency injection)", Color = Color.Gray },
            new() { Key = "request", Description = "Send request message (will receive response)", Color = Color.Green },
            new() { Key = "publish", Description = "Send publish message (the published message will be handled by the subscriber)", Color = Color.BrightYellow },
            new() { Key = "stream", Description = "Produce stream messages", Color = Color.BrightGreen },
            new() { Key = "exit", Description = "(exit)", Color = Color.Magenta }
        };

        var commandListView = new ListView(commands)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            CanFocus = true,
            ColorScheme = defaultScheme
        };

        commandListView.RowRender += args =>
        {
            if (commandListView.SelectedItem == args.Row)
            {
                return;
            }

            args.RowAttribute = new Attribute(commands[args.Row].Color, Color.Black);
        };

        _outputListView = new(LogEntries)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            CanFocus = false,
            ColorScheme = defaultScheme
        };

        _outputListView.RowRender += args =>
        {
            args.RowAttribute = new Attribute(LogEntries[args.Row].Foreground, Color.Black);
        };

        promptWin.Add(commandListView);
        outputWin.Add(_outputListView);
        top.Add(promptWin, outputWin);

        Task.Run(async () =>
        {
            try
            {
                Log("Initializing services...", Color.Cyan);
                var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                var services = new ServiceCollection()
                    .AddSingleton<IConfiguration>(configuration)
                    .AddHopper(hopperBuilder =>
                    {
                        configuration.GetSection(HopperOptions.SectionName).Bind(hopperBuilder.Options);

                        hopperBuilder
                            .UseAzureStorageQueues(builder =>
                            {
                                builder.AddOptions("hopper-samples", new() { ConnectionString = "UseDevelopmentStorage=true;" });
                            })
                            .UseKafka(builder =>
                            {
                                builder.AddOptions("local", new()
                                {
                                    BootstrapServers = "localhost:9092",
                                    EnableAutoCommit = true,
                                    EnableAutoOffsetStore = true,
                                    NumPartitions = 1,
                                    UseCancellationToken = false,
                                    ConsumeTimeout = TimeSpan.FromMilliseconds(25)
                                });
                            })
                            .AddMessageHandler((ResponseMessage message) =>
                            {
                                Log($"[RECV] Response ID: {message.Id}", Color.BrightGreen);
                                return Task.CompletedTask;
                            });
                    });

                var provider = services.BuildServiceProvider();
                _serviceBus = await provider.GetRequiredService<IServiceBus>().StartAsync();
                Log("Service Bus Started. Select a command above.", Color.BrightCyan);
            }
            catch (Exception ex)
            {
                Log($"STARTUP ERROR: {ex.Message}", Color.Red);
            }
        });

        commandListView.OpenSelectedItem += async args =>
        {
            var cmd = (Command)args.Value;

            if (cmd.Key == "exit")
            {
                Application.RequestStop();
                return;
            }

            if (_serviceBus == null)
            {
                Log("Error: Bus not initialized.", Color.Red);
                return;
            }

            Log($"Action: Executing {cmd.Key}...", cmd.Color);

            try
            {
                switch (cmd.Key)
                {
                    case "deferred":
                    {
                        await _serviceBus.SendAsync(new DeferredMessage(), b => b.Defer(DateTime.Now.AddSeconds(5)));
                        break;
                    }

                    case "email":
                    {
                        await _serviceBus.SendAsync(new EmailMessage());
                        break;
                    }

                    case "request":
                    {
                        await _serviceBus.SendAsync(new RequestMessage());
                        break;
                    }

                    case "publish":
                    {
                        await _serviceBus.SendAsync(new PublishMessage());
                        break;
                    }

                    case "stream":
                    {
                        for (var i = 1; i < 51; i++)
                        {
                            await _serviceBus.SendAsync(new StreamMessage { Index = i });
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Send Error: {ex.Message}", Color.Red);
            }
        };

        Application.Run();
        Application.Shutdown();

        if (_serviceBus != null)
        {
            Console.WriteLine("Closing Service Bus connections...");
            _serviceBus.Dispose();
        }

        Console.ResetColor();
        Console.Clear();
        Console.WriteLine("------------------------------------------");
        Console.WriteLine("Client has shut down successfully.");
        Console.WriteLine("------------------------------------------");

        Environment.Exit(0);
    }

    private class Command
    {
        public Color Color { get; init; }
        public string Description { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;

        public override string ToString()
        {
            return Description;
        }
    }

    private record LogEntry(string Message, Color Foreground)
    {
        public override string ToString()
        {
            return Message;
        }
    }
}