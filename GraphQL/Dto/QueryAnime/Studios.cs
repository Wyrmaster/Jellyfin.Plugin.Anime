using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Studios(
  [property: JsonPropertyName("edges")] IReadOnlyList<StudioEdge> Edges
);