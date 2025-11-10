using Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;
using Microsoft.Extensions.Caching.Memory;
using AnimeMedia = Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime.Media;
using QueryMedia = Jellyfin.Plugin.Anime.GraphQL.Dto.SearchAnime.Medium;

namespace Jellyfin.Plugin.Anime.GraphQL;

public class AniListProvider: IDisposable, IAsyncDisposable
{
  #region Fields

  private readonly GraphQlClient _client;
  private readonly IMemoryCache _cache;

  #endregion
  
  #region Constructor

  private AniListProvider(string endpoint)
  {
    this._client = new GraphQlClient(endpoint);
    this._cache = new MemoryCache(new MemoryCacheOptions());
  }

  #endregion

  #region Properties

  /// <summary>
  ///  Static Reference of the Provider
  /// </summary>
  public static AniListProvider? Instance { get; private set; }

  #endregion

  #region Public Methods

  /// <summary>
  ///   Reinitialze the Anilist Provider
  /// </summary>
  /// <param name="endpoint"></param>
  public static void Initialize(string endpoint)
  {
    Instance?.Dispose();
    Instance = new AniListProvider(endpoint);
  }

  /// <summary>
  ///   Get the Anime Metadata from an ID
  /// </summary>
  /// <param name="id"></param>
  /// <param name="token"></param>
  /// <returns></returns>
  public async Task<AnimeMedia?> GetAnimeDataAsync(long id, CancellationToken token = default)
  {
    if (!this._cache.TryGetValue(id, out Media? anime))
    {
      anime = (await this._client.LoadAnimeAsync(id, token))?.Media;
      if (anime == null)
      {
        return null;
      }
      
      this._cache.Set(id, anime, TimeSpan.FromMinutes(30));
    }

    return anime;
  }

  /// <summary>
  ///   Searches for an anime with a specific name
  /// </summary>
  /// <param name="query">serach query</param>
  /// <param name="token"></param>
  /// <returns></returns>
  public async IAsyncEnumerable<QueryMedia> SearchForAnimeAsync(string query, CancellationToken token = default)
  {
    foreach (var pageMedium in (await this._client.QueryAnimeAsync(query, token))?.Page?.Media ?? [])
    {
      yield return pageMedium;
    }
  }

  #endregion

  #region Disposable
  public void Dispose()
  {
    this._client.Dispose();
  }

  public async ValueTask DisposeAsync()
  {
    await this._client.DisposeAsync();
  }
  
  #endregion
}