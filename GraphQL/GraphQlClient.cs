using System.Net;
using GraphQL;
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
  private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

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

  public GraphQlClient(string url, ILogger? logger =  null)
  {
    this._logger = logger;
    this._client = new GraphQLHttpClient(url, new SystemTextJsonSerializer());
  }

  #endregion

  #region Public Methods

  public async Task<AnimeRoot?> LoadAnimeAsync(long id, CancellationToken token = default)
  {
    GraphQLResponse<AnimeRoot?> response = await this._client.SendQueryAsync<AnimeRoot?>(GetAnimeGraphqlQuery, new Dictionary<string, string> { { "id", id.ToString() } }, cancellationToken: token);

    if (response is GraphQLHttpResponse<AnimeRoot?> httpResponse && httpResponse.StatusCode != HttpStatusCode.OK)
    {
      this._logger?.LogError(string.Join(Environment.NewLine,response.Errors?.Select(entry => entry.Message) ?? ["An error occured"]));
      return null;
    }

    return response.Data;
  }

  public async Task<QueryRoot?> QueryAnimeAsync(string query, CancellationToken token = default)
  {
    await this._semaphoreSlim.WaitAsync(token);
    
    if (token.IsCancellationRequested)
    {
      return null;
    }
    GraphQLResponse<QueryRoot?> response = await this._client.SendQueryAsync<QueryRoot?>(SearchAnimeGraphqlQuery, new Dictionary<string, string> { { "query", query } }, cancellationToken: token);
    
    this._semaphoreSlim.Release();
    
    if (response is GraphQLHttpResponse<QueryRoot?> httpResponse && httpResponse.StatusCode != HttpStatusCode.OK)
    {
      this._logger?.LogError(string.Join(Environment.NewLine,response.Errors?.Select(entry => entry.Message) ?? ["An error occured"]));
      return null;
    }

    return response.Data;
  }

  #endregion

  #region Disposable

  public void Dispose()
  {
    this.DisposeAsync().AsTask().Wait();
  }

  public async ValueTask DisposeAsync()
  {
    await CastAndDispose(this._client);
    await CastAndDispose(this._semaphoreSlim);

    return;

    static async ValueTask CastAndDispose(IDisposable resource)
    {
      if (resource is IAsyncDisposable resourceAsyncDisposable)
      {
        await resourceAsyncDisposable.DisposeAsync();
      }
      else
      {
        resource.Dispose();
      }
    }
  }
  
  #endregion
}