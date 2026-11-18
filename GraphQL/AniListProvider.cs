using System.Runtime.CompilerServices;
using Jellyfin.Plugin.Anime.Common;
using Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;
using Microsoft.Extensions.Caching.Memory;
using AnimeMedia = Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime.Media;
using QueryMedia = Jellyfin.Plugin.Anime.GraphQL.Dto.SearchAnime.Medium;

namespace Jellyfin.Plugin.Anime.GraphQL;

public class AniListProvider: IDisposable, IAsyncDisposable
{
  #region Fields

  private static readonly TimeSpan AnimeCacheDuration = TimeSpan.FromMinutes(30);
  private static readonly TimeSpan NotFoundCacheDuration = TimeSpan.FromMinutes(5);
  private static readonly TimeSpan SearchCacheDuration = TimeSpan.FromMinutes(10);

  /// <summary>
  ///   Shared between re-initializations so a cooldown survives a configuration change.
  ///   AniList allows about 90 requests per minute (30 while degraded), so stay well below that.
  /// </summary>
  private static readonly IRequestThrottle Throttle = new RequestThrottle(TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(90));

  private readonly GraphQlClient _client;
  private readonly IMemoryCache _cache;

  #endregion
  
  #region Constructor

  private AniListProvider(string endpoint)
  {
    this._client = new GraphQlClient(endpoint, Throttle);
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
    if (this._cache.TryGetValue(id, out Media? anime))
    {
      return anime;
    }

    anime = (await this._client.LoadAnimeAsync(id, token))?.Media;
    this._cache.Set(id, anime, anime == null ? NotFoundCacheDuration : AnimeCacheDuration);

    return anime;
  }

  /// <summary>
  ///   Searches for an anime with a specific name
  /// </summary>
  /// <param name="query">serach query</param>
  /// <param name="token"></param>
  /// <returns></returns>
  public async IAsyncEnumerable<QueryMedia> SearchForAnimeAsync(string query, [EnumeratorCancellation] CancellationToken token = default)
  {
    string key = $"search:{query.ToLowerInvariant()}";
    if (!this._cache.TryGetValue(key, out List<QueryMedia>? results))
    {
      results = (await this._client.QueryAnimeAsync(query, token))?.Page?.Media?.ToList() ?? [];
      this._cache.Set(key, results, SearchCacheDuration);
    }

    foreach (QueryMedia medium in results!)
    {
      yield return medium;
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