namespace BZAPI.Configuration
{
    /// <summary>
    /// Settings for the Steam Web API.
    /// </summary>
    public sealed class SteamOptions
    {
        public const string SectionName = "Steam";

        /// <summary>
        /// Steam Web API key, obtained from https://steamcommunity.com/dev/apikey.
        /// Supplied via configuration: the <c>Steam__ApiKey</c> environment variable or user
        /// secrets. When empty, avatar lookups are skipped.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// How long a successfully resolved avatar URL is cached before Steam is queried again.
        /// </summary>
        public TimeSpan AvatarCacheDuration { get; set; } = TimeSpan.FromHours(6);

        /// <summary>
        /// How long a failed lookup is cached, to avoid hammering Steam for accounts that
        /// consistently fail to resolve.
        /// </summary>
        public TimeSpan AvatarFailureCacheDuration { get; set; } = TimeSpan.FromMinutes(5);
    }
}
