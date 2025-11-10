using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Media(
  [property: JsonPropertyName("id")] int? Id,
  [property: JsonPropertyName("title")] Title Title,
  [property: JsonPropertyName("startDate")] Date StartDate,
  [property: JsonPropertyName("endDate")] Date EndDate,
  [property: JsonPropertyName("coverImage")] CoverImage CoverImage,
  [property: JsonPropertyName("bannerImage")] string BannerImage,
  [property: JsonPropertyName("format")] string Format,
  [property: JsonPropertyName("type")] string Type,
  [property: JsonPropertyName("status")] string Status,
  [property: JsonPropertyName("episodes")] int? Episodes,
  [property: JsonPropertyName("chapters")] object Chapters,
  [property: JsonPropertyName("volumes")] object Volumes,
  [property: JsonPropertyName("season")] string Season,
  [property: JsonPropertyName("seasonYear")] int? SeasonYear,
  [property: JsonPropertyName("description")] string Description,
  [property: JsonPropertyName("averageScore")] double? AverageScore,
  [property: JsonPropertyName("meanScore")] double? MeanScore,
  [property: JsonPropertyName("genres")] IReadOnlyList<string> Genres,
  [property: JsonPropertyName("synonyms")] IReadOnlyList<string> Synonyms,
  [property: JsonPropertyName("duration")] int? Duration,
  [property: JsonPropertyName("tags")] IReadOnlyList<Tag> Tags,
  [property: JsonPropertyName("nextAiringEpisode")] object NextAiringEpisode,
  [property: JsonPropertyName("studios")] Studios Studios,
  [property: JsonPropertyName("characters")] Characters Characters
);