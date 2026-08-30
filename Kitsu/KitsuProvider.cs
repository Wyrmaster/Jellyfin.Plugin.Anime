using System.Text.Json;
using Jellyfin.Plugin.Anime.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Anime.Kitsu;

/// <summary>
///   Provider to resolve episode information for animes from kitsu
/// </summary>
public class KitsuProvider
{
  #region Constants

  private const string KitsuApiUrl = @"https://kitsu.io/api/edge/episodes?filter[mediaId]={0}&filter[number]={1}";

  #endregion

  #region Fields
  
  private SemaphoreSlim _semaphoreSlim = new(2);

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
  ///   Download episode data from kitsu
  /// </summary>
  /// <param name="client">http client that performs the request</param>
  /// <param name="mediaId">id of the anime in the kitsu eco system</param>
  /// <param name="episodeNumber">episode to resolve</param>
  /// <param name="logger"></param>
  /// <param name="token"></param>
  /// <returns></returns>
  public async Task<EpisodeData?> GetEpisodeData(HttpClient client, long mediaId, int episodeNumber, ILogger? logger, CancellationToken token = default)
  {
    
    await this._semaphoreSlim.WaitAsync(token);
    
    EpisodeData? episodeData = null;

    try
    {
      string content = await client.GetStringAsync(string.Format(KitsuApiUrl, mediaId, episodeNumber), token);
      
      KitsuEpisodeListResponse? response = JsonSerializer.Deserialize<KitsuEpisodeListResponse>(content);
      if (response == null || !response.Data.Any())
      {
        return null;
      }
      
      episodeData = response.Data.First();
    }
    catch (Exception e)
    {
      logger?.LogError(e.Message, e);
    }
    finally
    {
      this._semaphoreSlim.Release();
    }

    return episodeData;
  }

  #endregion
}