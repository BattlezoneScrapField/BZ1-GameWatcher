using BZAPI.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace BZAPI.Steam
{
    public interface ISteamAvatarProvider
    {
        /// <summary>
        /// Resolves a player's full-size avatar URL, or null if it cannot be determined.
        /// </summary>
        Task<string?> GetAvatarUrlAsync(ulong steamId, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Looks up Steam avatars, caching results in memory.
    /// </summary>
    /// <remarks>
    /// Lobby updates arrive continuously and each one lists every connected player, so querying
    /// Steam per player per message burns through the Web API quota (100k calls/day) and stalls
    /// message processing behind network latency. Both successes and failures are cached so a
    /// steady-state lounge issues almost no Steam traffic.
    /// </remarks>
    public sealed class SteamAvatarProvider : ISteamAvatarProvider
    {
        private readonly IMemoryCache _cache;
        private readonly SteamOptions _options;
        private readonly ILogger<SteamAvatarProvider> _logger;
        private readonly SteamUser? _steamUser;

        public SteamAvatarProvider(
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory,
            IOptions<SteamOptions> options,
            ILogger<SteamAvatarProvider> logger)
        {
            _cache = cache;
            _options = options.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _logger.LogWarning(
                    "No Steam API key configured ({Section}:{Key}); player avatars will be unavailable.",
                    SteamOptions.SectionName,
                    nameof(SteamOptions.ApiKey));

                return;
            }

            var factory = new SteamWebInterfaceFactory(_options.ApiKey);
            _steamUser = factory.CreateSteamWebInterface<SteamUser>(httpClientFactory.CreateClient(nameof(SteamAvatarProvider)));
        }

        public async Task<string?> GetAvatarUrlAsync(ulong steamId, CancellationToken cancellationToken)
        {
            if (_steamUser is null)
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var cacheKey = $"steam:avatar:{steamId}";

            if (_cache.TryGetValue(cacheKey, out string? cached))
            {
                return cached;
            }

            string? avatarUrl = null;

            try
            {
                var summary = await _steamUser.GetPlayerSummaryAsync(steamId);
                avatarUrl = summary?.Data?.AvatarFullUrl;
            }
            catch (Exception ex)
            {
                // A failed lookup must never take down the watcher; the player simply renders
                // without an avatar until the negative cache entry expires.
                _logger.LogWarning(ex, "Failed to fetch Steam avatar for {SteamId}.", steamId);
            }

            _cache.Set(
                cacheKey,
                avatarUrl,
                avatarUrl is null ? _options.AvatarFailureCacheDuration : _options.AvatarCacheDuration);

            return avatarUrl;
        }
    }
}
