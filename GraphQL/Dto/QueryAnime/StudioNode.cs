using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record StudioNode(
  [property: JsonPropertyName("id")] int? Id,
  [property: JsonPropertyName("name")] string Name,
  [property: JsonPropertyName("isAnimationStudio")] bool? IsAnimationStudio
);