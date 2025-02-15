using Bridge.Core;
using McMaster.Extensions.CommandLineUtils;

namespace Bridge.Web.Commands;

[HelpOption]
[Command("cleanup-worker", "Run this program as a worker that occasionally clean up expired records.")]
public class CleanUpWorkerCommand
{
    private readonly ILogger<CleanUpWorkerCommand> _logger;
    private readonly CleanUpCommand _cleanUpCommand;

    [Option("--interval", ShortName = "-i", Description = "Clean up interval. Default to 10 minutes.")]
    public uint Interval { get; private set; } = 10;

    public CleanUpWorkerCommand(IEnumerable<IEphemeralCleaner> ephemeralCleaners,
        ILogger<CleanUpWorkerCommand> logger)
    {
        _ = nameof(OnExecuteAsync);
        _logger = logger;
        _cleanUpCommand = new(ephemeralCleaners, logger);
    }

    public async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await _cleanUpCommand.OnExecuteAsync(cancellationToken);
                var interval = TimeSpan.FromMinutes(Interval);
                await Task.Delay(interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured during cleanup iteration");
            }
        }
    }
}