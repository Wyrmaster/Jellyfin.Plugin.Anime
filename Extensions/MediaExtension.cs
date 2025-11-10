using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;
using Jellyfin.Plugin.Anime.Providers;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.Anime.Extensions;

/// <summary>
///   Extension Methods for Media objects
/// </summary>
public static  class MediaExtension
{
  #region Extention Methods

  /// <summary>
  ///   Converts an Animeobject to a TVideo
  /// </summary>
  /// <param name="anime"></param>
  /// <typeparam name="TVideo"></typeparam>
  /// <returns></returns>
  public static TVideo ToVideo<TVideo>(this Media anime) where TVideo : Video, new() =>
    new()
    {
      Name = anime.Title.English,
      OriginalTitle = anime.Title.Romaji,
      Overview = anime.Description,
      ProductionYear = anime.StartDate.Year,
      PremiereDate = new DateTime(anime.StartDate.Year ?? 1970, anime.StartDate.Month ?? 1, anime.StartDate.Day ?? 1, 0, 0, 0),
      EndDate = new DateTime(anime.EndDate.Year ?? 1970, anime.EndDate.Month ?? 1, anime.EndDate.Day ?? 1, 0, 0, 0),
      CommunityRating = (float) (anime.MeanScore ?? anime.AverageScore ?? 0.0d),
      RunTimeTicks = TimeSpan.FromMinutes(anime.Duration ?? 0).Ticks,
      Genres = anime.Genres.ToArray(),
      Studios = anime.Studios.Edges.Select(entry => entry.Node.Name).ToArray(),
      ProviderIds = new Dictionary<string, string>{{ProviderNames.AniListProvider, anime.Id?.ToString() ?? "0"}}
    };
  
  /// <summary>
  ///   convert an anime to a folder item
  /// </summary>
  /// <param name="anime"></param>
  /// <typeparam name="TItem"></typeparam>
  /// <returns></returns>
  public static TItem ToFolder<TItem>(this Media anime) where TItem : Folder, new() =>
    new()
    {
      Name = anime.Title.English,
      OriginalTitle = anime.Title.Romaji,
      Overview = anime.Description,
      ProductionYear = anime.StartDate.Year,
      PremiereDate = new DateTime(anime.StartDate.Year ?? 1970, anime.StartDate.Month ?? 1, anime.StartDate.Day ?? 1, 0, 0, 0),
      EndDate = new DateTime(anime.EndDate.Year ?? 1970, anime.EndDate.Month ?? 1, anime.EndDate.Day ?? 1, 0, 0, 0),
      CommunityRating = (float) (anime.MeanScore ?? anime.AverageScore ?? 0.0d),
      RunTimeTicks = TimeSpan.FromMinutes(anime.Duration ?? 0).Ticks,
      Genres = anime.Genres.ToArray(),
      Studios = anime.Studios.Edges.Select(entry => entry.Node.Name).ToArray(),
      ProviderIds = new Dictionary<string, string>{{ProviderNames.AniListProvider, anime.Id?.ToString() ?? "0"}}
    };

  /// <summary>
  ///   Read the characters of an anime 
  /// </summary>
  /// <param name="anime"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
  public static async IAsyncEnumerable<PersonInfo> ToPersonInfos(this Media? anime, [EnumeratorCancellation] CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
  {
    if (anime == null)
    {
      yield break;
    }
    
    foreach (Edge charactersEdge in anime.Characters.Edges)
    {
      VoiceActor? voiceActor = charactersEdge.VoiceActors.FirstOrDefault(entry => entry.Language == "Japanese");
      if (voiceActor == null)
      {
        continue;
      }

      yield return new()
      {
        Name = $"{voiceActor.Name.Full} {(string.IsNullOrEmpty(voiceActor.Name.Native)?  string.Empty : $"({voiceActor.Name.Native})")}",
        Role = charactersEdge.Node.Name.Full,
        Type = PersonKind.Actor,
        ImageUrl = voiceActor.Image.Large ?? voiceActor.Image.Medium,
        ProviderIds = { [ProviderNames.AniListProvider] = anime.Id?.ToString() ?? "0" }
      };
    }
  }
  
  #endregion
}