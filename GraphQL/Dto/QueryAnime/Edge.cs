using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Edge(
  [property: JsonPropertyName("node")] Node Node,
  [property: JsonPropertyName("isMain")] bool? IsMain,
  [property: JsonPropertyName("role")] string Role,
  [property: JsonPropertyName("voiceActors")] IReadOnlyList<VoiceActor> VoiceActors
);