using AfterStorm.Application.Interfaces;
using AfterStorm.Infrastructure.Persistence;
using AfterStorm.Infrastructure.Replay;
using AfterStorm.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AfterStorm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' was not configured.");

        services.AddDbContext<AfterStormDbContext>(options =>
            options.UseNpgsql(connectionString));

        var replayPath = configuration["ReplayStorage:Path"] ?? "replays";

        services.Configure<HotsParserOptions>(configuration.GetSection("HotsParser"));
        services.AddSingleton<IReplayStorage>(_ => new LocalReplayStorage(replayPath));
        services.AddScoped<IReplayParser, StormReplayParser>();
        services.AddScoped<IMatchRepository, MatchRepository>();

        return services;
    }
}
