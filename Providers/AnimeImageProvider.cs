using Jellyfin.Plugin.Anime.GraphQL;
using Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using Season = MediaBrowser.Controller.Entities.TV.Season;

namespace Jellyfin.Plugin.Anime.Providers;

/// <summary>
///   Provider for resolving remote images from AniList
/// </summary>
public class AnimeImageProvider: IRemoteImageProvider
{
  #region Field

  private readonly ILogger<AnimeImageProvider>? _logger;
  private readonly HttpClient _httpClient;

  #endregion

  #region Constructor

  public AnimeImageProvider(IHttpClientFactory httpClientFactory, ILogger<AnimeImageProvider>? logger = null)
  {
    this._logger = logger;
    this._httpClient =  httpClientFactory.CreateClient(nameof(AnimeImageProvider));
  }

  #endregion
  
  #region RemoteImageProvider

  /// <inheritdoc cref="IRemoteImageProvider.Name"/>
  public string Name => ProviderNames.AniListProvider;

  /// <inheritdoc cref="IRemoteImageProvider.Supports"/>
  public bool Supports(BaseItem item) => item is Video or Series or Season;

  /// <inheritdoc cref="IRemoteImageProvider.GetSupportedImages"/>
  public IEnumerable<ImageType> GetSupportedImages(BaseItem item) =>
  [
    ImageType.Primary
  ];

  /// <inheritdoc cref="IRemoteImageProvider.GetImages"/>
  public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
  {
    if (!item.ProviderIds.ContainsKey(ProviderNames.AniListProvider)
        || !long.TryParse(item.ProviderIds[ProviderNames.AniListProvider], out long id))
    {
      return [];
    }

    try
    {
      Media? anime = await (AniListProvider.Instance?.GetAnimeDataAsync(id, cancellationToken) ?? Task.FromResult<Media?>(null));

      return
      [
        new()
        {
          Url = anime?.CoverImage?.ExtraLarge ?? anime?.CoverImage?.Large ?? anime?.CoverImage?.Medium,
          Type = ImageType.Primary,
          ProviderName = ProviderNames.AniListProvider,
        }
      ];
    }
    catch (Exception e)
    {
      this._logger?.LogError(e,$"Unable to resolve images for file: {item.Path}");
      return [];
    }
  }

  /// <inheritdoc cref="IRemoteImageProvider.GetImageResponse"/>
  public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    => this._httpClient.GetAsync(url, cancellationToken);

  #endregion
}