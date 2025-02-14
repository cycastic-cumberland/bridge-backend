using Bridge.Web.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace Bridge.Web;

[Command("Bridge.Web")]
[Subcommand(typeof(MigrateCommand))]
[Subcommand(typeof(CleanUpCommand))]
[Subcommand(typeof(CleanUpWorkerCommand))]
public class Program
{
    private static WebApplication? app;

    public static async Task<int> Main(string[] args)
    {
        _ = nameof(OnExecuteAsync);
        var builder = WebApplication.CreateBuilder(args);
        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services, builder.Environment);
        app = builder.Build();
        startup.Configure(app, app.Environment);

        // Command line processing.
        var commandLineApplication = new CommandLineApplication<Program>();
        using var scope = app.Services.CreateScope();
        commandLineApplication
            .Conventions
            .UseConstructorInjection(scope.ServiceProvider)
            .UseDefaultConventions();
        return await commandLineApplication.ExecuteAsync(args);
    }
    
    public async Task<int> OnExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));
        await app.StartAsync();
        await app.InitAsync();
        await app.WaitForShutdownAsync();
        return 0;
    }
}