using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.Dto;

public class EpisodeData
{
  [JsonPropertyName("id")]
  public string Id { get; set; }
 
  [JsonPropertyName("type")]
  public string Type { get; set; }
 
  [JsonPropertyName("attributes")]
  public EpisodeAttributes Attributes { get; set; }
}