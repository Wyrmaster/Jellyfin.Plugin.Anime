using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Title(
  [property: JsonPropertyName("romaji")] string Romaji,
  [property: JsonPropertyName("english")] string English,
  [property: JsonPropertyName("native")] string Native,
  [property: JsonPropertyName("userPreferred")] string UserPreferred
);