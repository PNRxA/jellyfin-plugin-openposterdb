using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.OpenPosterDB.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.OpenPosterDB.Providers
{
    /// <summary>
    /// Base class for the OpenPosterDB remote image providers. Builds OpenPosterDB
    /// image URLs from an item's external ids and lets Jellyfin download them.
    /// </summary>
    public abstract class OpenPosterDbImageProvider : IRemoteImageProvider, IHasOrder
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenPosterDbImageProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        protected OpenPosterDbImageProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc />
        public string Name => "OpenPosterDB";

        /// <inheritdoc />
        public int Order => 0;

        /// <inheritdoc />
        public abstract bool Supports(BaseItem item);

        /// <inheritdoc />
        public abstract IEnumerable<ImageType> GetSupportedImages(BaseItem item);

        /// <summary>
        /// Resolves the OpenPosterDB id for the given item, or <c>null</c> when no usable external id exists.
        /// </summary>
        /// <param name="item">The library item.</param>
        /// <returns>The resolved <see cref="OpdbId"/>, or <c>null</c>.</returns>
        protected abstract OpdbId? ResolveId(BaseItem item);

        /// <inheritdoc />
        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var results = new List<RemoteImageInfo>();
            var config = Plugin.Instance?.Configuration;
            if (config is null
                || string.IsNullOrWhiteSpace(config.ApiKey)
                || string.IsNullOrWhiteSpace(config.BaseUrl))
            {
                return Task.FromResult<IEnumerable<RemoteImageInfo>>(results);
            }

            var id = ResolveId(item);
            if (id is null)
            {
                return Task.FromResult<IEnumerable<RemoteImageInfo>>(results);
            }

            foreach (var type in GetSupportedImages(item))
            {
                var url = BuildUrl(config, id.Value, type);
                results.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Url = url,
                    ThumbnailUrl = url,
                    Type = type,
                });
            }

            return Task.FromResult<IEnumerable<RemoteImageInfo>>(results);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient(NamedClient.Default);
            return httpClient.GetAsync(new Uri(url), cancellationToken);
        }

        /// <summary>
        /// Maps a Jellyfin <see cref="ImageType"/> to the OpenPosterDB path segment
        /// (<c>poster</c>, <c>backdrop</c> or <c>logo</c>). The episode provider overrides
        /// this to map <see cref="ImageType.Primary"/> to <c>episode</c>.
        /// </summary>
        /// <param name="type">The image type.</param>
        /// <returns>The OpenPosterDB image-kind path segment.</returns>
        protected virtual string KindFor(ImageType type) => type switch
        {
            ImageType.Primary => "poster",
            ImageType.Backdrop => "backdrop",
            ImageType.Logo => "logo",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported image type for OpenPosterDB"),
        };

        /// <summary>
        /// Yields the art image types (Primary/Backdrop/Logo) enabled in the plugin
        /// configuration. Used by the movie and series providers.
        /// </summary>
        /// <returns>The enabled art image types.</returns>
        protected static IEnumerable<ImageType> EnabledArtImages()
        {
            var config = Plugin.Instance?.Configuration;
            if (config is null)
            {
                yield break;
            }

            if (config.EnablePosters)
            {
                yield return ImageType.Primary;
            }

            if (config.EnableBackdrops)
            {
                yield return ImageType.Backdrop;
            }

            if (config.EnableLogos)
            {
                yield return ImageType.Logo;
            }
        }

        /// <summary>
        /// Resolves the OpenPosterDB id for a movie or series from its IMDb/TMDB/TVDB ids,
        /// preferring IMDb (the most reliably populated and cross-resolvable id).
        /// </summary>
        /// <param name="item">The movie or series item.</param>
        /// <param name="isSeries"><c>true</c> for a series (uses the <c>series-</c> TMDB prefix), <c>false</c> for a movie.</param>
        /// <returns>The resolved <see cref="OpdbId"/>, or <c>null</c>.</returns>
        protected static OpdbId? MovieOrSeriesId(BaseItem item, bool isSeries)
        {
            var imdb = item.GetProviderId(MetadataProvider.Imdb);
            if (!string.IsNullOrEmpty(imdb))
            {
                return new OpdbId("imdb", imdb);
            }

            var tmdb = item.GetProviderId(MetadataProvider.Tmdb);
            if (!string.IsNullOrEmpty(tmdb))
            {
                var prefix = isSeries ? "series-" : "movie-";
                return new OpdbId("tmdb", prefix + tmdb);
            }

            var tvdb = item.GetProviderId(MetadataProvider.Tvdb);
            if (!string.IsNullOrEmpty(tvdb))
            {
                return new OpdbId("tvdb", tvdb);
            }

            return null;
        }

        private string BuildUrl(PluginConfiguration config, OpdbId id, ImageType type)
        {
            var baseUrl = config.BaseUrl.TrimEnd('/');
            var kind = KindFor(type);
            var ext = type == ImageType.Logo ? "png" : "jpg";

            var url = string.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1}/{2}/{3}-default/{4}.{5}",
                baseUrl,
                config.ApiKey,
                id.IdType,
                kind,
                id.IdValueBase,
                ext);

            var extra = config.ExtraQuery?.Trim().TrimStart('?', '&');
            if (!string.IsNullOrEmpty(extra))
            {
                url += "?" + extra;
            }

            return url;
        }
    }
}
