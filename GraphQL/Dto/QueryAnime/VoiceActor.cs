using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record VoiceActor(
  [property: JsonPropertyName("id")] int? Id,
  [property: JsonPropertyName("name")] Name Name,
  [property: JsonPropertyName("image")] Image Image,
  [property: JsonPropertyName("language")] string Language
);