using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vladify.Application.Playlists;

namespace Vladify.Infrastructure.Spotify;

public static class SpotifyServiceCollectionExtensions
{
    public static IServiceCollection AddSpotifyIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SpotifyOptions>(configuration.GetSection(SpotifyOptions.SectionName));

        services.AddHttpClient<SpotifyTokenProvider>();
        services.AddHttpClient<SpotifyPlaylistReader>();

        services.AddSingleton<SpotifyTokenProvider>();
        services.AddScoped<ISpotifyPlaylistReader, SpotifyPlaylistReader>();

        return services;
    }
}
