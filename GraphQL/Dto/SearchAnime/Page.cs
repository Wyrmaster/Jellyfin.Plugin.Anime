using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.SearchAnime;

public record Page(
  [property: JsonPropertyName("media")] IReadOnlyList<Medium> Media
);