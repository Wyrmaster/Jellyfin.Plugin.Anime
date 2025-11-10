using Jellyfin.Plugin.Anime.Common;
using Jellyfin.Plugin.Anime.Extensions;
using Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Anime.Providers;

/// <summary>
///   Provider to resolve Anime Series from MyAnimeList
/// </summary>
public class AnimeSeriesProvider: AbstractBaseAnimeProvider<Series, SeriesInfo>
{
  #region Constructor
  
  public AnimeSeriesProvider(IHttpClientFactory httpClientFactory, ILogger<AbstractBaseAnimeProvider<Series, SeriesInfo>> logger) : base(httpClientFactory, logger)
  {
  }

  #endregion

  #region Overrides

  /// <inheritdoc/>
  public override async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
  {
    Media? anime = await this.ResolveAnime(info.Path, cancellationToken);

    if (anime == null)
    {
      return new NoMetaDataFound<Series>();
    }
    
    Series series = anime.ToFolder<Series>();

    series.TrySetProviderId(ProviderNames.AniListProvider, info.GetProviderId(ProviderNames.AniListProvider));
    
    return new MetadataResult<Series>
    {
      HasMetadata = true,
      Item = series,
      People = await anime.ToPersonInfos(cancellationToken).ToListAsync(cancellationToken),
      Provider = ProviderNames.AniListProvider,
    };
  }

  #endregion
}