using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Tag(
  [property: JsonPropertyName("id")] int? Id,
  [property: JsonPropertyName("name")] string Name,
  [property: JsonPropertyName("category")] string Category,
  [property: JsonPropertyName("isMediaSpoiler")] bool? IsMediaSpoiler
);