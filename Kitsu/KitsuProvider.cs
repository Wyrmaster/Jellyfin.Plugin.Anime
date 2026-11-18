using System.Text.Json;
using Jellyfin.Plugin.Anime.Common;
using Jellyfin.Plugin.Anime.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Anime.Kitsu;

/// <summary>
///   Provider to resolve episode information for animes from kitsu
/// </summary>
public class KitsuProvider
{
  #region Constants

  private const int PageSize = 20;
  private const string KitsuApiUrl = @"https://kitsu.io/api/edge/episodes?filter[mediaId]={0}&sort=number&page[limit]=20&page[offset]={1}";
  private static readonly TimeSpan PageCacheDuration = TimeSpan.FromHours(1);

  #endregion

  #region Fields

  private readonly IRequestThrottle _throttle = new RequestThrottle(TimeSpan.FromMilliseconds(750), TimeSpan.FromSeconds(90));
  private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

  #endregion

  #region Constructor

  private KitsuProvider(){}

  #endregion

  #region Instance

  private static KitsuProvider? _instance;

  /// <summary>
  ///   Static access instance
  /// </summary>
  public static KitsuProvider Instance => _instance ??= new KitsuProvider();

  #endregion

  #region Public Methods

  /// <summary>
  ///   Download episode data from kitsu. Kitsu lists 20 episodes per page; every page is requested once
  ///   and cached, so a whole series costs a few requests instead of one per episode.
  /// </summary>
  /// <param name="client">http client that performs the request</param>
  /// <param name="mediaId">id of the anime in the kitsu eco system</param>
  /// <param name="episodeNumber">episode to resolve</param>
  /// <param name="logger"></param>
  /// <param name="token"></param>
  /// <returns></returns>
  public async Task<EpisodeData?> GetEpisodeData(HttpClient client, long mediaId, int episodeNumber, ILogger? logger, CancellationToken token = default)
  {
    if (episodeNumber < 1)
    {
      return null;
    }

    int offset = (episodeNumber - 1) / PageSize * PageSize;
    try
    {
      List<EpisodeData> page = await this.GetPageAsync(client, mediaId, offset).WaitAsync(token);
      return page.FirstOrDefault(entry => entry.Attributes.Number == episodeNumber);
    }
    catch (RateLimitedException e)
    {
      logger?.LogWarning("Kitsu is rate limiting requests, skipping episode {Episode} of {MediaId} for now: {Message}", episodeNumber, mediaId, e.Message);
    }
    catch (Exception e) when (e is not OperationCanceledException)
    {
      logger?.LogError(e, "Could not load episode {Episode} of {MediaId} from Kitsu", episodeNumber, mediaId);
    }

    return null;
  }

  #endregion

  #region Private Methods

  /// <summary>
  ///   The page task is cached, so parallel callers share one request. A failed request is not kept.
  /// </summary>
  private Task<List<EpisodeData>> GetPageAsync(HttpClient client, long mediaId, int offset)
  {
    string key = $"{mediaId}:{offset}";
    if (this._cache.TryGetValue(key, out Task<List<EpisodeData>>? cached) && cached != null)
    {
      return cached;
    }

    Task<List<EpisodeData>> page = this._throttle.ExecuteAsync(() => FetchPageAsync(client, mediaId, offset));
    this._cache.Set(key, page, PageCacheDuration);
    page.ContinueWith(
      _ => this._cache.Remove(key),
      CancellationToken.None,
      TaskContinuationOptions.OnlyOnFaulted,
      TaskScheduler.Default);

    return page;
  }

  private static async Task<List<EpisodeData>> FetchPageAsync(HttpClient client, long mediaId, int offset)
  {
    using HttpRequestMessage request = new(HttpMethod.Get, string.Format(KitsuApiUrl, mediaId, offset));
    request.Headers.Accept.ParseAdd("application/vnd.api+json");
    request.Headers.UserAgent.ParseAdd("Jellyfin.Plugin.Anime");

    using HttpResponseMessage response = await client.SendAsync(request);
    if (RateLimitedException.IsBlockingStatus(response.StatusCode))
    {
      throw new RateLimitedException(RateLimitedException.ParseRetryAfter(response.Headers));
    }

    response.EnsureSuccessStatusCode();
    string content = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<KitsuEpisodeListResponse>(content)?.Data ?? [];
  }

  #endregion
}
