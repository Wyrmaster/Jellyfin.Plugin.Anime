using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Date(
  [property: JsonPropertyName("year")] int? Year,
  [property: JsonPropertyName("month")] int? Month,
  [property: JsonPropertyName("day")] int? Day
);