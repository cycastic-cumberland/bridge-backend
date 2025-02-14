using Bridge.Core;
using Bridge.Infrastructure;
using Bridge.Infrastructure.Data;
using Bridge.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bridge.Web;

public class Startup
{
    private const string CorsPolicy = nameof(CorsPolicy);
    
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment environment)
    {
        var settings = configuration.GetSection(nameof(AppSettings));
        services.Configure<AppSettings>(settings);
        services.Configure<RoomConfigurations>(settings.GetSection(nameof(AppSettings.RoomConfigurations)));
        services.Configure<ItemConfigurations>(settings.GetSection(nameof(AppSettings.ItemConfigurations)));
        services.Configure<S3Settings>(settings.GetSection(nameof(AppSettings.S3Settings)));

        string connectionString = configuration.GetConnectionString("DefaultConnection") ??
                                  throw new InvalidOperationException("No connection string was supplied");
        services.AddDbContext<PostgresAppDbContext>(options
            => options
                .UseNpgsql(connectionString, b
                    => b.MigrationsAssembly(DataAccessRoot.AssemblyName)));

        services.AddScoped<IAppDbContext, PostgresAppDbContext>();
        services.TryAddScoped<DbContext, PostgresAppDbContext>();

        services.AddScoped<IStorageService, S3StorageService>();
        services.AddSingleton<IUrlGenerator, UrlGenerator>();
        services.AddScoped<QrService>();
        services.AddScoped<RoomService>();
        services.AddScoped<ItemService>();

        var cors = settings.Get<AppSettings>()?.AllowedOrigins;
        if (!string.IsNullOrEmpty(cors))
        {
            var origins = cors.Split(";");
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy, policy =>
                {
                    policy.WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        services.AddAsyncInitialization();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddControllers(cfg =>
        {
            cfg.Filters.Add<ApiExceptionFilter>();
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();
        app.UseCors(CorsPolicy);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}