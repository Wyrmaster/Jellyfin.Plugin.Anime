using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Image(
  [property: JsonPropertyName("medium")] string? Medium,
  [property: JsonPropertyName("large")] string? Large
);