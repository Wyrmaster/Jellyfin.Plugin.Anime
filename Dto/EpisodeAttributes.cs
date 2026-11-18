using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Anime.Dto;

public class EpisodeAttributes
{
  [JsonPropertyName("canonicalTitle")]
  public string CanonicalTitle { get; set; }
 
  [JsonPropertyName("synopsis")]
  public string? Description { get; set; }
  
  [JsonPropertyName("number")]
  public int? Number { get; set; }

  [JsonPropertyName("airdate")]
  public string? AirDate { get; set; }
}