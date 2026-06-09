using System.Collections.Generic;
using System.Net.Http;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.OpenPosterDB.Providers
{
    /// <summary>
    /// Supplies OpenPosterDB poster, backdrop and logo images for movies.
    /// </summary>
    public class OpenPosterDbMovieImageProvider : OpenPosterDbImageProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenPosterDbMovieImageProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        public OpenPosterDbMovieImageProvider(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        /// <inheritdoc />
        public override bool Supports(BaseItem item) => item is Movie;

        /// <inheritdoc />
        public override IEnumerable<ImageType> GetSupportedImages(BaseItem item) => EnabledArtImages();

        /// <inheritdoc />
        protected override OpdbId? ResolveId(BaseItem item) => MovieOrSeriesId(item, isSeries: false);
    }
}
