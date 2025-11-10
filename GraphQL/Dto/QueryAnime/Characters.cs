using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Characters(
  [property: JsonPropertyName("edges")] IReadOnlyList<Edge> Edges
);