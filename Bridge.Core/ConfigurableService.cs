using Bridge.Infrastructure;
using Microsoft.Extensions.Options;

namespace Bridge.Core;

public abstract class ConfigurableService<TConfig> where TConfig : class
{
    private readonly IOptions<TConfig> _config;

    protected IAppDbContext DbContext { get; }

    public TConfig Configurations => _config.Value;
    
    protected ConfigurableService(IAppDbContext dbContext, IOptions<TConfig> configurations)
    {
        DbContext = dbContext;
        _config = configurations;
    }
}