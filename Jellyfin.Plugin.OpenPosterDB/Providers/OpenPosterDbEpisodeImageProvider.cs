using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.OpenPosterDB.Providers
{
    /// <summary>
    /// Supplies OpenPosterDB per-episode still images (with episode-level rating badges)
    /// for TV episodes. Built from the parent series' external id plus the season/episode
    /// numbers, e.g. <c>imdb/episode-default/episode-tt0903747-S1E1.jpg</c>.
    /// </summary>
    public class OpenPosterDbEpisodeImageProvider : OpenPosterDbImageProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenPosterDbEpisodeImageProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        public OpenPosterDbEpisodeImageProvider(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        /// <inheritdoc />
        public override bool Supports(BaseItem item) => item is Episode;

        /// <inheritdoc />
        public override IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            var config = Plugin.Instance?.Configuration;
            if (config is not null && config.EnableEpisodes)
            {
                // The episode's main image in Jellyfin is its Primary (the still).
                yield return ImageType.Primary;
            }
        }

        /// <inheritdoc />
        protected override string KindFor(ImageType type) =>
            type == ImageType.Primary ? "episode" : base.KindFor(type);

        /// <inheritdoc />
        protected override OpdbId? ResolveId(BaseItem item)
        {
            if (item is not Episode episode)
            {
                return null;
            }

            var season = episode.ParentIndexNumber;
            var number = episode.IndexNumber;
            if (season is null || number is null)
            {
                return null;
            }

            var series = episode.Series;
            if (series is null)
            {
                return null;
            }

            var sxe = string.Format(CultureInfo.InvariantCulture, "S{0}E{1}", season.Value, number.Value);

            var imdb = series.GetProviderId(MetadataProvider.Imdb);
            if (!string.IsNullOrEmpty(imdb))
            {
                return new OpdbId("imdb", FormatEpisode(imdb, sxe));
            }

            var tmdb = series.GetProviderId(MetadataProvider.Tmdb);
            if (!string.IsNullOrEmpty(tmdb))
            {
                return new OpdbId("tmdb", FormatEpisode(tmdb, sxe));
            }

            var tvdb = series.GetProviderId(MetadataProvider.Tvdb);
            if (!string.IsNullOrEmpty(tvdb))
            {
                return new OpdbId("tvdb", FormatEpisode(tvdb, sxe));
            }

            return null;
        }

        private static string FormatEpisode(string seriesId, string sxe) =>
            string.Format(CultureInfo.InvariantCulture, "episode-{0}-{1}", seriesId, sxe);
    }
}
