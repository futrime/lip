using Lip.Core;
using Spectre.Console;
using System.Collections.Concurrent;

namespace Lip.CLI;

class UserInteraction : IUserInteraction
{
    private readonly ConcurrentDictionary<string, (float, string)> _progressUpdates = new();

    public async Task<bool> Confirm(string format, params object[] args)
    {
        return await AnsiConsole.ConfirmAsync(string.Format(format, args).EscapeMarkup());
    }

    public async Task<string> PromptForInput(string defaultValue, string format, params object[] args)
    {
        return await AnsiConsole.AskAsync(string.Format(format, args).EscapeMarkup(), defaultValue: defaultValue);
    }

    public async Task<string> PromptForSelection(IEnumerable<string> options, string format, params object[] args)
    {
        return await AnsiConsole.PromptAsync(new SelectionPrompt<string>()
            .Title(string.Format(format, args).EscapeMarkup())
            .AddChoices(options)
        );
    }

    public async Task RunProgressService()
    {
        while (true)
        {
            await Task.Delay(1);

            if (_progressUpdates.IsEmpty)
            {
                continue;
            }

            await AnsiConsole.Progress()
                .Columns([
                    new TaskDescriptionColumn()
                    {
                        Alignment = Justify.Left,
                    },
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn(),
                ])
                .StartAsync(async ctx =>
                {
                    Dictionary<string, ProgressTask> tasks = [];

                    do
                    {
                        foreach (var (id, (progress, description)) in _progressUpdates)
                        {
                            if (!tasks.TryGetValue(id, out ProgressTask? value))
                            {
                                value = ctx.AddTask(description);
                                tasks[id] = value;
                            }

                            value.Description = description;
                            value.Value = progress;
                        }

                        // Here some updates may be ignored, but we don't know how to handle it.

                        _progressUpdates.Clear();

                        await Task.Delay(1);
                    } while (!ctx.IsFinished);
                });
        }
    }

    public async Task UpdateProgress(string id, float progress, string format, params object[] args)
    {
        await Task.CompletedTask; // Suppress warning.

        _progressUpdates[id] = new(progress * 100, string.Format(format, args).EscapeMarkup());
    }
}