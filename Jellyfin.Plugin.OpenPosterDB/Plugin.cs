using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.OpenPosterDB.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.OpenPosterDB
{
    /// <summary>
    /// The OpenPosterDB plugin. Registers remote image providers that fetch
    /// rating-badge artwork from a self-hosted OpenPosterDB instance.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static Plugin? Instance { get; private set; }

        /// <inheritdoc />
        public override string Name => "OpenPosterDB";

        /// <inheritdoc />
        public override string Description =>
            "Fetches posters, backdrops, logos and episode stills (with rating badges) " +
            "from a self-hosted OpenPosterDB instance, by each item's IMDb / TMDB / TVDB id.";

        /// <inheritdoc />
        public override Guid Id => Guid.Parse("f3a7c2e1-9b4d-4e6a-8c1f-2d5b7e9a0c34");

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}.Configuration.configPage.html",
                        GetType().Namespace),
                },
            };
        }
    }
}
