using System.Collections.ObjectModel;
using Messages.v1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.Kafka;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace Client;

internal class Program
{
    private static readonly ObservableCollection<LogEntry> LogEntries = [];

    private static IApplication _app = null!;
    private static ListView _outputListView = null!;
    private static IBus? _bus;
    private static IBusControl? _busControl;

    private static void Log(string message, Color color)
    {
        _app.Invoke(() =>
        {
            LogEntries.Add(new($"[{DateTime.Now:HH:mm:ss}] {message}", color));

            _outputListView.SelectedItem = LogEntries.Count - 1;
            _outputListView.EnsureSelectedItemVisible();
        });
    }

    private static void Main()
    {
        _app = Application.Create().Init();

        var defaultScheme = new Scheme
        {
            Normal = new Attribute(Color.White, Color.Black),
            Focus = new Attribute(Color.Black, Color.Gray),
            HotNormal = new Attribute(Color.BrightCyan, Color.Black),
            HotFocus = new Attribute(Color.BrightCyan, Color.Gray)
        };

        var top = new Window
        {
            Title = "Shuttle.Hopper Client",
            BorderStyle = LineStyle.None
        };

        top.SetScheme(defaultScheme);

        var promptWin = new FrameView
        {
            Title = "Message Prompts",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(40)
        };

        var outputWin = new FrameView
        {
            Title = "System Output (Press Ctrl+Q to Exit)",
            X = 0,
            Y = Pos.Bottom(promptWin),
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var commands = new ObservableCollection<Command>
        {
            new() { Key = "deferred", Description = "Send a deferred message (waits 5 seconds)", Color = Color.Yellow },
            new() { Key = "email", Description = "Send simulated e-mail processing (demonstrates dependency injection)", Color = Color.Gray },
            new() { Key = "request", Description = "Send request message (will receive response)", Color = Color.Green },
            new() { Key = "publish", Description = "Send publish message (the published message will be handled by the subscriber)", Color = Color.BrightYellow },
            new() { Key = "stream", Description = "Produce stream messages", Color = Color.BrightGreen },
            new() { Key = "priority", Description = "Send priority message (handled by the server's additional 'priority' inbox)", Color = Color.BrightMagenta },
            new() { Key = "exit", Description = "(exit)", Color = Color.Magenta }
        };

        var commandListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            CanFocus = true
        };

        commandListView.SetSource(commands);
        commandListView.SelectedItem = 0;

        commandListView.RowRender += (_, args) =>
        {
            if (commandListView.SelectedItem == args.Row)
            {
                return;
            }

            args.RowAttribute = new Attribute(commands[args.Row].Color, Color.Black);
        };

        _outputListView = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            CanFocus = false
        };

        _outputListView.SetSource(LogEntries);

        _outputListView.RowRender += (_, args) =>
        {
            args.RowAttribute = new Attribute(LogEntries[args.Row].Foreground, Color.Black);
        };

        promptWin.Add(commandListView);
        outputWin.Add(_outputListView);
        top.Add(promptWin, outputWin);

        top.KeyDown += (_, key) =>
        {
            if (key != Key.Q.WithCtrl)
            {
                return;
            }

            key.Handled = true;
            _app.RequestStop();
        };

        Task.Run(async () =>
        {
            try
            {
                Log("Initializing services...", Color.Cyan);
                var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                var provider = new ServiceCollection()
                    .AddSingleton<IConfiguration>(configuration)
                    .AddHopper(options =>
                    {
                        configuration.GetSection(HopperOptions.SectionName).Bind(options);
                    })
                    .UseAzureStorageQueues(builder =>
                    {
                        builder.Configure("hopper-samples", options =>
                        {
                            options.ConnectionString = "UseDevelopmentStorage=true;";
                        });
                    })
                    .UseKafka(builder =>
                    {
                        builder.Configure("local", options =>

                        {
                            options.BootstrapServers = "localhost:9092";
                            options.EnableAutoCommit = true;
                            options.EnableAutoOffsetStore = true;
                            options.NumPartitions = 1;
                            options.UseCancellationToken = false;
                            options.ConsumeTimeout = TimeSpan.FromMilliseconds(25);
                        });
                    })
                    .AddMessageHandler((ResponseMessage message) =>
                    {
                        Log($"[RECV] Response ID: {message.Id}", Color.BrightGreen);
                        return Task.CompletedTask;
                    })
                    .Services
                    .BuildServiceProvider();

                _bus = provider.GetRequiredService<IBus>();
                _busControl = await provider.GetRequiredService<IBusControl>().StartAsync();

                Log("Service Bus Started. Select a command above.", Color.BrightCyan);
            }
            catch (Exception ex)
            {
                Log($"STARTUP ERROR: {ex.Message}", Color.Red);
            }
        });

        commandListView.Accepting += async (_, args) =>
        {
            if (commandListView.SelectedItem is not { } selectedItem)
            {
                return;
            }

            args.Handled = true;

            var cmd = commands[selectedItem];

            if (cmd.Key == "exit")
            {
                _app.RequestStop();
                return;
            }

            if (_busControl == null || _bus == null)
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
                        await _bus.SendAsync(new DeferredMessage(), b => b.DeferUntil(DateTime.Now.AddSeconds(5)));
                        break;
                    }

                    case "email":
                    {
                        await _bus.SendAsync(new EmailMessage());
                        break;
                    }

                    case "request":
                    {
                        await _bus.SendAsync(new RequestMessage());
                        break;
                    }

                    case "publish":
                    {
                        await _bus.SendAsync(new PublishMessage());
                        break;
                    }

                    case "priority":
                    {
                        await _bus.SendAsync(new PriorityMessage());
                        break;
                    }

                    case "stream":
                    {
                        for (var i = 1; i < 51; i++)
                        {
                            await _bus.SendAsync(new StreamMessage { Index = i });
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

        _app.Run(top);
        top.Dispose();
        _app.Dispose();

        if (_busControl != null)
        {
            Console.WriteLine("Closing Service Bus connections...");
            _busControl.Dispose();
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