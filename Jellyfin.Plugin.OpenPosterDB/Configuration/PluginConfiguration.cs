using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.OpenPosterDB.Configuration
{
    /// <summary>
    /// Plugin configuration for OpenPosterDB.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            BaseUrl = "https://openposterdb.com";
            ApiKey = string.Empty;
            EnablePosters = true;
            EnableBackdrops = true;
            EnableLogos = true;
            EnableEpisodes = true;
            ExtraQuery = string.Empty;
        }

        /// <summary>
        /// Gets or sets the base URL of the OpenPosterDB instance
        /// (e.g. <c>https://openposterdb.com</c> or <c>http://localhost:3000</c>).
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the OpenPosterDB API key (64-character hex string).
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether poster (Primary) images are offered.
        /// </summary>
        public bool EnablePosters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether backdrop images are offered.
        /// </summary>
        public bool EnableBackdrops { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logo images are offered.
        /// </summary>
        public bool EnableLogos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether episode still images are offered.
        /// </summary>
        public bool EnableEpisodes { get; set; }

        /// <summary>
        /// Gets or sets extra query parameters appended verbatim to every image
        /// request, e.g. <c>lang=de&amp;imageSize=large&amp;badge_size=l</c>.
        /// Leading <c>?</c> / <c>&amp;</c> are stripped automatically.
        /// </summary>
        public string ExtraQuery { get; set; }
    }
}
