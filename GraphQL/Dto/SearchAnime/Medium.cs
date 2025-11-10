using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.GraphQL.Dto.SearchAnime;

public record Medium(
  [property: JsonPropertyName("id")] long? Id,
  [property: JsonPropertyName("title")] Title Title,
  [property: JsonPropertyName("coverImage")] CoverImage CoverImage,
  [property: JsonPropertyName("startDate")] Date StartDate
);