using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Anime.Providers;

/// <summary>
///   Provider for Anime Movies
/// </summary>
/// <param name="httpClientFactory"></param>
/// <param name="logger"></param>
public class AnimeMovieProvider(
  IHttpClientFactory httpClientFactory,
  ILogger<AbstractBaseAnimeProvider<Movie, MovieInfo>>? logger = null)
  : AbstractAnimeProvider<Movie, MovieInfo>(httpClientFactory, logger);