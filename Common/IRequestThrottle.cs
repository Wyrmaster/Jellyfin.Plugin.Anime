namespace Jellyfin.Plugin.Anime.Common;

/// <summary>
///   Spaces out requests to an API and stops sending while the API blocks us
/// </summary>
public interface IRequestThrottle
{
  /// <summary>
  ///   Waits for a free request slot, sends the request and retries it after the cooldown when the API rate limits us
  /// </summary>
  /// <exception cref="RateLimitedException">The API keeps blocking us or the cooldown is too long to wait for</exception>
  Task<T> ExecuteAsync<T>(Func<Task<T>> send, CancellationToken token = default);
}
