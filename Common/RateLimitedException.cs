using System.Net;
using System.Net.Http.Headers;

namespace Jellyfin.Plugin.Anime.Common;

/// <summary>
///   Thrown when an API signals that we are sending too many requests (or refuses us because of it)
/// </summary>
public class RateLimitedException(TimeSpan? retryAfter = null, Exception? inner = null)
  : Exception("The remote API is rate limiting requests", inner)
{
  #region Constants

  /// <summary>
  ///   Cooldown used when the API does not tell us how long to wait
  /// </summary>
  public static readonly TimeSpan DefaultCooldown = TimeSpan.FromSeconds(60);

  #endregion

  #region Properties

  /// <summary>
  ///   How long no further request should be sent
  /// </summary>
  public TimeSpan RetryAfter { get; } = retryAfter ?? DefaultCooldown;

  #endregion

  #region Static Methods

  /// <summary>
  ///   AniList and Kitsu answer with 429, but also with 401/402/403 once a client is blocked
  /// </summary>
  public static bool IsBlockingStatus(HttpStatusCode status) =>
    status is HttpStatusCode.TooManyRequests
      or HttpStatusCode.Unauthorized
      or HttpStatusCode.PaymentRequired
      or HttpStatusCode.Forbidden;

  /// <summary>
  ///   Reads the Retry-After header (seconds or date)
  /// </summary>
  public static TimeSpan? ParseRetryAfter(HttpHeaders? headers)
  {
    if (headers == null || !headers.TryGetValues("Retry-After", out IEnumerable<string>? values))
    {
      return null;
    }

    string? value = values.FirstOrDefault();
    if (int.TryParse(value, out int seconds))
    {
      return TimeSpan.FromSeconds(seconds);
    }

    return DateTimeOffset.TryParse(value, out DateTimeOffset date)
      ? date - DateTimeOffset.UtcNow
      : null;
  }

  #endregion
}
