using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Web.Commands;

[HelpOption]
[Command("migrate", "Apply all available migration to database.")]
public class MigrateCommand
{
    private readonly DbContext _dbContext;

    public MigrateCommand(DbContext dbContext)
    {
        _ = nameof(OnExecuteAsync);
        _dbContext = dbContext;
    }

    public Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Database.MigrateAsync(cancellationToken);
    }
}