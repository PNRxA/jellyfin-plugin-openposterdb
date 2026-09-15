using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.OpenPosterDB.Providers
{
    /// <summary>Supplies season posters from a season-enabled OpenPosterDB server.</summary>
    public class OpenPosterDbSeasonImageProvider : OpenPosterDbImageProvider
    {
        public OpenPosterDbSeasonImageProvider(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        public override bool Supports(BaseItem item) => item is Season;

        public override IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            if (Plugin.Instance?.Configuration.EnablePosters == true)
            {
                yield return ImageType.Primary;
            }
        }

        /// <inheritdoc />
        protected override string KindFor(ImageType type) =>
            type == ImageType.Primary ? "season" : base.KindFor(type);

        protected override OpdbId? ResolveId(BaseItem item)
        {
            if (item is not Season season || season.IndexNumber is not int number || number < 0
                || season.Series is not Series series)
            {
                return null;
            }

            var id = MovieOrSeriesId(series, isSeries: true);
            if (id is null)
            {
                return null;
            }

            var seriesId = id.Value.IdType == "tmdb"
                ? id.Value.IdValueBase.Substring("series-".Length)
                : id.Value.IdValueBase;
            return new OpdbId(id.Value.IdType,
                string.Format(CultureInfo.InvariantCulture, "season-{0}-S{1}", seriesId, number));
        }
    }
}
