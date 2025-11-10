using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record CoverImage(
  [property: JsonPropertyName("medium")] string? Medium,
  [property: JsonPropertyName("large")] string? Large,
  [property: JsonPropertyName("extraLarge")] string? ExtraLarge
);