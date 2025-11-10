using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.SearchAnime;

public record Root(
  [property: JsonPropertyName("Page")] Page Page
);
