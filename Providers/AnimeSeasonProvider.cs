using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Anime.Providers;

/// <summary>
///   Provider for anime seasons. Since most sites don't handle episodes by season but by series we can set this always to 1
/// </summary>
/// <param name="httpClientFactory"></param>
public class AnimeSeasonProvider(IHttpClientFactory httpClientFactory): IRemoteMetadataProvider<Season, SeasonInfo>
{
  #region RemoteMetadataProvider
  
  public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
  {
    return Task.FromResult(new MetadataResult<Season>()
    {
      Item = new Season
      {
        IndexNumber = 1,
      }
    });
  }

  public string Name => ProviderNames.AniListProvider;
  
  
  public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    => httpClientFactory.CreateClient().GetAsync(url, cancellationToken);

  #endregion
  
}