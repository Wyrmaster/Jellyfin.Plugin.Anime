using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.Dto;

// Top-level response wrapper for a list of episodes
// e.g. GET /anime/{id}/episodes
public class KitsuEpisodeListResponse
{
  [JsonPropertyName("data")]
  public List<EpisodeData> Data { get; set; }
}