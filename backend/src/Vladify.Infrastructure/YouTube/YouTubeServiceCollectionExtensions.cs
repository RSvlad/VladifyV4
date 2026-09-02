using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vladify.Application.Tracks;

namespace Vladify.Infrastructure.YouTube;

public static class YouTubeServiceCollectionExtensions
{
    public static IServiceCollection AddYouTubeIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<YouTubeOptions>(configuration.GetSection(YouTubeOptions.SectionName));
        services.AddScoped<IYouTubeTrackSearcher, YtDlpTrackSearcher>();

        return services;
    }
}
