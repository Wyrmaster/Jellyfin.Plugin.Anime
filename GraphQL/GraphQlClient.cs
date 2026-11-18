using GraphQL;
using Jellyfin.Plugin.Anime.Common;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Logging;

using AnimeRoot = Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime.Root;
using QueryRoot = Jellyfin.Plugin.Anime.GraphQL.Dto.SearchAnime.Root;

namespace Jellyfin.Plugin.Anime.GraphQL;

public class GraphQlClient : IDisposable, IAsyncDisposable
{
  #region Fields

  private readonly ILogger? _logger;
  private readonly GraphQLHttpClient _client;
  private readonly IRequestThrottle _throttle;

  private const string GetAnimeGraphqlQuery =
    """
        query($id: Int!) {
          Media(id: $id, type: ANIME) {
            id
            title {
              romaji
              english
              native
              userPreferred
            }
            startDate {
              year
              month
              day
            }
            endDate {
              year
              month
              day
            }
            coverImage {
              medium
              large
              extraLarge
            }
            bannerImage
            format
            type
            status
            episodes
            chapters
            volumes
            season
            seasonYear
            description
            averageScore
            meanScore
            genres
            synonyms
            duration
            tags {
              id
              name
              category
              isMediaSpoiler
            }
            nextAiringEpisode {
              airingAt
              timeUntilAiring
              episode
            }

            studios {
              edges {
                node {
                  id
                  name
                  isAnimationStudio
                }
                isMain
              }
            }
            characters(sort: [ROLE, FAVOURITES_DESC]) {
              edges {
                node {
                  id
                  name {
                    first
                    last
                    full
                  }
                  image {
                    medium
                    large
                  }
                }
                role
                voiceActors {
                  id
                  name {
                    first
                    last
                    full
                    native
                  }
                  image {
                    medium
                    large
                  }
                  language: languageV2
                }
              }
            }
          }
        }
    """;

  private const string SearchAnimeGraphqlQuery =
    """
        query ($query: String) {
          Page {
            media(search: $query, type: ANIME) {
              id
              title {
                romaji
                english
                native
              }
              coverImage {
                medium
                large
                extraLarge
              }
              startDate {
                year
                month
                day
              }
            }
          }
        }
    """;

  #endregion

  #region Constructor

  public GraphQlClient(string url, IRequestThrottle throttle, ILogger? logger = null)
  {
    this._logger = logger;
    this._throttle = throttle;
    this._client = new GraphQLHttpClient(url, new SystemTextJsonSerializer());
  }

  #endregion

  #region Public Methods

  public Task<AnimeRoot?> LoadAnimeAsync(long id, CancellationToken token = default) =>
    this.SendAsync<AnimeRoot>(GetAnimeGraphqlQuery, new { id }, token);

  public Task<QueryRoot?> QueryAnimeAsync(string query, CancellationToken token = default) =>
    this.SendAsync<QueryRoot>(SearchAnimeGraphqlQuery, new { query }, token);

  #endregion

  #region Private Methods

  private Task<T?> SendAsync<T>(string query, object variables, CancellationToken token) where T : class =>
    this._throttle.ExecuteAsync(() => this.SendOnceAsync<T>(query, variables, token), token);

  private async Task<T?> SendOnceAsync<T>(string query, object variables, CancellationToken token) where T : class
  {
    GraphQLResponse<T> response;
    try
    {
      response = await this._client.SendQueryAsync<T>(query, variables, cancellationToken: token);
    }
    catch (GraphQLHttpRequestException e) when (RateLimitedException.IsBlockingStatus(e.StatusCode))
    {
      throw new RateLimitedException(RateLimitedException.ParseRetryAfter(e.ResponseHeaders), e);
    }

    if (response is GraphQLHttpResponse<T> httpResponse && RateLimitedException.IsBlockingStatus(httpResponse.StatusCode))
    {
      throw new RateLimitedException(RateLimitedException.ParseRetryAfter(httpResponse.ResponseHeaders));
    }

    if (response.Errors is { Length: > 0 })
    {
      this._logger?.LogError("AniList returned errors: {Errors}", string.Join(Environment.NewLine, response.Errors.Select(entry => entry.Message)));
    }

    return response.Data;
  }

  #endregion

  #region Disposable

  public void Dispose()
  {
    this._client.Dispose();
  }

  public ValueTask DisposeAsync()
  {
    this._client.Dispose();
    return ValueTask.CompletedTask;
  }

  #endregion
}
