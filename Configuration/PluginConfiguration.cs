using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Anime.Configuration;

/// <summary>
///   Configuration for this Jellyfin Plugin
/// </summary>
public class PluginConfiguration: BasePluginConfiguration
{
  #region Constructor

  public PluginConfiguration()
  {
    
  }

  #endregion
  
  #region Properties

  /// <summary>
  ///   Endpoint for the AniList GraphQl
  /// </summary>
  public string AniListEndpoint { get; set; } = "https://graphql.anilist.co/";

  /// <summary>
  ///   Endpoint for the Jikan Endpoint
  /// </summary>
  public string JikanEndpoint { get; set; }

  #endregion
}