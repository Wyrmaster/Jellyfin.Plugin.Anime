using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Anime.Common;

/// <summary>
///   Allows one request per <paramref name="minInterval" /> and pauses all requests while the API blocks us.
///   Requests that would have to wait longer than <paramref name="maxWait" /> fail immediately, so nothing
///   hammers a blocked API and nothing hangs for minutes.
/// </summary>
public class RequestThrottle(TimeSpan minInterval, TimeSpan maxWait, ILogger? logger = null) : IRequestThrottle
{
  #region Constants

  private const int MaxAttempts = 3;
  private static readonly TimeSpan MaxCooldown = TimeSpan.FromMinutes(10);

  #endregion

  #region Fields

  private readonly object _lock = new();
  private DateTimeOffset _nextSlot = DateTimeOffset.MinValue;

  #endregion

  #region IRequestThrottle

  public async Task<T> ExecuteAsync<T>(Func<Task<T>> send, CancellationToken token = default)
  {
    for (int attempt = 1; ; attempt++)
    {
      await this.WaitForSlotAsync(token);
      try
      {
        return await send();
      }
      catch (RateLimitedException e)
      {
        this.Block(e.RetryAfter);
        if (attempt >= MaxAttempts)
        {
          throw;
        }
      }
    }
  }

  #endregion

  #region Private Methods

  private async Task WaitForSlotAsync(CancellationToken token)
  {
    while (true)
    {
      TimeSpan delay = this.TryTakeSlot();
      if (delay <= TimeSpan.Zero)
      {
        return;
      }

      if (delay > maxWait)
      {
        throw new RateLimitedException(delay);
      }

      await Task.Delay(delay, token);
    }
  }

  private TimeSpan TryTakeSlot()
  {
    lock (this._lock)
    {
      DateTimeOffset now = DateTimeOffset.UtcNow;
      if (now < this._nextSlot)
      {
        return this._nextSlot - now;
      }

      this._nextSlot = now + minInterval;
      return TimeSpan.Zero;
    }
  }

  private void Block(TimeSpan duration)
  {
    duration = duration > MaxCooldown ? MaxCooldown : duration;
    lock (this._lock)
    {
      DateTimeOffset until = DateTimeOffset.UtcNow + duration;
      this._nextSlot = until > this._nextSlot ? until : this._nextSlot;
    }

    logger?.LogWarning("API is rate limiting requests, pausing for {Seconds:F0}s", duration.TotalSeconds);
  }

  #endregion
}
