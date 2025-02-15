using Bridge.Core;
using McMaster.Extensions.CommandLineUtils;

namespace Bridge.Web.Commands;

[HelpOption]
[Command("cleanup", "Clean up expired records.")]
public class CleanUpCommand
{
    private readonly IEnumerable<IEphemeralCleaner> _ephemeralCleaners;
    private readonly ILogger _logger;

    public CleanUpCommand(IEnumerable<IEphemeralCleaner> ephemeralCleaners,
        ILogger<CleanUpCommand> logger)
        : this(ephemeralCleaners, (ILogger)logger)
    {
    }
    
    internal CleanUpCommand(IEnumerable<IEphemeralCleaner> ephemeralCleaners,
        ILogger logger)
    {
        _ = nameof(OnExecuteAsync);
        _ephemeralCleaners = ephemeralCleaners;
        _logger = logger;
    }

    public async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clean up started");

        foreach (var cleaner in _ephemeralCleaners)
        {
            _logger.LogInformation("Cleaning: {Type}", cleaner.EphemeralType.Name);
            await cleaner.CleanUpAsync(cancellationToken);
            _logger.LogInformation("Finished cleaning: {Type}", cleaner.EphemeralType.Name);
        }
        
        _logger.LogInformation("Clean up completed");
    }
}