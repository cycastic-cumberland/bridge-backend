using Bridge.Core;
using McMaster.Extensions.CommandLineUtils;

namespace Bridge.Web.Commands;

[HelpOption]
[Command("cleanup", "Clean up expired records.")]
public class CleanUpCommand
{
    private readonly RoomService _roomService;
    private readonly ItemService _itemService;
    private readonly ILogger _logger;

    public CleanUpCommand(RoomService roomService,
        ItemService itemService,
        ILogger<CleanUpCommand> logger)
        : this(roomService, itemService, (ILogger)logger)
    {
    }
    
    internal CleanUpCommand(RoomService roomService,
        ItemService itemService,
        ILogger logger)
    {
        _ = nameof(OnExecuteAsync);
        _roomService = roomService;
        _itemService = itemService;
        _logger = logger;
    }

    public async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clean up started");
        _logger.LogInformation("Cleaning expired rooms");
        await _roomService.CleanRoomsAsync(cancellationToken);
        _logger.LogInformation("Expired rooms removed.");
        _logger.LogInformation("Cleaning expired items");
        await _itemService.CleanItemsAsync(cancellationToken);
        _logger.LogInformation("Expired items removed.");
        _logger.LogInformation("Clean up completed");
    }
}