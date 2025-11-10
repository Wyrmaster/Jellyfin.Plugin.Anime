using JikanDotNet;

namespace Jellyfin.Plugin.Anime.MyAnimeList;

/// <summary>
///   Connector to the MAL
/// </summary>
public class JikanProvider
{
  #region Fields

  private readonly Jikan _jikan;
  private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

  #endregion
  
  #region Constructor

  public JikanProvider(string endpoint)
  {
    this._jikan = string.IsNullOrEmpty(endpoint)
      ? new()
      : new(new(), new HttpClient()
      {
        BaseAddress = new Uri(endpoint)
      });
  }

  #endregion
  
  #region Instance

  public static JikanProvider? Instance { get; private set; }

  #endregion

  #region Public Methods

  /// <summary>
  ///   Reinitialize the JikanProvider
  /// </summary>
  /// <param name="endpoint"></param>
  public static void Initialize(string endpoint)
  {
    Instance = new JikanProvider(endpoint);
  }

  /// <summary>
  ///   Get the metadata of an anime episode
  /// </summary>
  /// <param name="malId">id of the anime in myanimelist</param>
  /// <param name="idx">episode index</param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task<AnimeEpisode?> GetAnimeEpisodeAsync(long? malId, int idx, CancellationToken cancellationToken = default)
  {
    await this._semaphoreSlim.WaitAsync(cancellationToken);
    
    var result = malId == null
      ? null
      : (await this._jikan.GetAnimeEpisodeAsync(malId.Value, idx, cancellationToken)).Data;

    this._semaphoreSlim.Release();
    
    return result;
  }

  #endregion
}