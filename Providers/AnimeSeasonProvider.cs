using Jellyfin.Plugin.Anime.Common;
using Jellyfin.Plugin.Anime.Extensions;
using Jellyfin.Plugin.Anime.GraphQL;
using Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;
using Season = MediaBrowser.Controller.Entities.TV.Season;

namespace Jellyfin.Plugin.Anime.Providers;

/// <summary>
///   Implementation for Anime Seasons
/// </summary>
public class AnimeSeasonProvider: AbstractBaseAnimeProvider<Season, SeasonInfo>
{
  #region Constructor

  public AnimeSeasonProvider(IHttpClientFactory httpClientFactory, ILogger<AbstractBaseAnimeProvider<Season, SeasonInfo>> logger)
    : base(httpClientFactory, logger)
  {
  }

  #endregion

  #region Overrides

  /// <inheritdoc/>
  public override async Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
  {
    Media? anime;

    try
    {
      anime = await 
      (
        AniListProvider.Instance?.GetAnimeDataAsync
        (
          long.Parse(info.SeriesProviderIds[ProviderNames.AniListProvider]),
          cancellationToken
        )
        ?? Task.FromResult<Media?>(null)
      );
    }
    catch (Exception e)
    {
      this.Logger?.LogError(e, $"Error resolving anime");
      anime = null;
    }

    if (anime == null)
    {
      return new NoMetaDataFound<Season>();
    }

    Season season = anime.ToFolder<Season>();
    
    // can be set to one as every anime will have one season and anime "seasons" are all treated as
    // their own shows therefore have their own Ids
    season.IndexNumber = 1;

    return new()
    {
      HasMetadata = true,
      Item = season,
      Provider = ProviderNames.AniListProvider,
      People = await anime.ToPersonInfos(cancellationToken).ToListAsync(cancellationToken),
    };
  }
  
  #endregion
}