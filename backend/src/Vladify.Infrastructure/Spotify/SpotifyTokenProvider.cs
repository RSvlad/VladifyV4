using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Vladify.Infrastructure.Spotify;

/// <summary>
/// Obtains and caches a Spotify access token via the Client Credentials flow.
/// Stateless server, single-user app: an in-memory cache (no persistence) is enough,
/// since a token is only ever needed for the lifetime of a running server process.
/// </summary>
public sealed class SpotifyTokenProvider
{
    private const string TokenEndpoint = "https://accounts.spotify.com/api/token";

    private readonly HttpClient _httpClient;
    private readonly SpotifyOptions _options;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public SpotifyTokenProvider(HttpClient httpClient, IOptions<SpotifyOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        // Refresh 60s before actual expiry to avoid using a token that expires mid-request.
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _expiresAt - TimeSpan.FromSeconds(60))
        {
            return _cachedToken;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _expiresAt - TimeSpan.FromSeconds(60))
            {
                return _cachedToken;
            }

            var (token, expiresInSeconds) = await RequestNewTokenAsync(cancellationToken);
            _cachedToken = token;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
            return token;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<(string Token, int ExpiresInSeconds)> RequestNewTokenAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint);

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<TokenResponse>(stream, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Spotify token response was empty.");

        return (payload.AccessToken, payload.ExpiresIn);
    }

    private sealed record TokenResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string AccessToken,
        [property: System.Text.Json.Serialization.JsonPropertyName("expires_in")] int ExpiresIn);
}
