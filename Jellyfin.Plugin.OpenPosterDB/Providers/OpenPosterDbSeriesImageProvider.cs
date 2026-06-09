using System.Collections.Generic;
using System.Net.Http;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.OpenPosterDB.Providers
{
    /// <summary>
    /// Supplies OpenPosterDB poster, backdrop and logo images for TV series.
    /// </summary>
    public class OpenPosterDbSeriesImageProvider : OpenPosterDbImageProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenPosterDbSeriesImageProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        public OpenPosterDbSeriesImageProvider(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        /// <inheritdoc />
        public override bool Supports(BaseItem item) => item is Series;

        /// <inheritdoc />
        public override IEnumerable<ImageType> GetSupportedImages(BaseItem item) => EnabledArtImages();

        /// <inheritdoc />
        protected override OpdbId? ResolveId(BaseItem item) => MovieOrSeriesId(item, isSeries: true);
    }
}
