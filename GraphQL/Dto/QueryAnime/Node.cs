using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Node(
  [property: JsonPropertyName("id")] int? Id,
  [property: JsonPropertyName("name")] Name Name,
  [property: JsonPropertyName("isAnimationStudio")] bool? IsAnimationStudio,
  [property: JsonPropertyName("image")] Image Image
);