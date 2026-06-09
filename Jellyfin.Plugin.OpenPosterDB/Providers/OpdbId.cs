namespace Jellyfin.Plugin.OpenPosterDB.Providers
{
    /// <summary>
    /// An OpenPosterDB identifier: the id type segment and the id value (without file extension).
    /// </summary>
    /// <param name="IdType">The OpenPosterDB id type segment: <c>imdb</c>, <c>tmdb</c> or <c>tvdb</c>.</param>
    /// <param name="IdValueBase">
    /// The id value without a file extension, e.g. <c>tt0903747</c>, <c>movie-550</c>,
    /// <c>series-1396</c> or <c>episode-1396-S1E1</c>.
    /// </param>
    public readonly record struct OpdbId(string IdType, string IdValueBase);
}
