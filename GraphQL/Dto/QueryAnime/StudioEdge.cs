using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record StudioEdge(
  [property: JsonPropertyName("node")] StudioNode Node,
  [property: JsonPropertyName("isMain")] bool? IsMain,
  [property: JsonPropertyName("role")] string Role,
  [property: JsonPropertyName("voiceActors")] IReadOnlyList<VoiceActor> VoiceActors
);