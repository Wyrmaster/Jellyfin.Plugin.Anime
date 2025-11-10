using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Name(
  [property: JsonPropertyName("first")] string First,
  [property: JsonPropertyName("last")] string Last,
  [property: JsonPropertyName("full")] string Full,
  [property: JsonPropertyName("native")] string Native
);