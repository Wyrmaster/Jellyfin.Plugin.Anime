using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;

public record Root(
  [property: JsonPropertyName("Media")] Media Media
);