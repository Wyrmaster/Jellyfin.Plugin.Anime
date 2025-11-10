using Jellyfin.Plugin.Anime.Configuration;
using Jellyfin.Plugin.Anime.GraphQL;
using Jellyfin.Plugin.Anime.MyAnimeList;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Anime;

/// <summary>
///   Plugin entry point 
/// </summary>
public class Plugin
  : BasePlugin<PluginConfiguration>,
    IHasWebPages
{
  #region Constructor
  
  public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
  {
    AniListProvider.Initialize(this.Configuration.AniListEndpoint);
    JikanProvider.Initialize(this.Configuration.JikanEndpoint);
    this.ConfigurationChanged += this.HandleConfigurationChanged;
  }

  #endregion

  #region Override
  
  /// <inheritdoc/>
  public override string Name => "AnimeProvider";

  /// <inheritdoc/>
  public override Guid Id => Guid.Parse("597c0d6e-2e91-402d-be34-54b67c4a5da2");

  /// <inheritdoc/>
  public override string Description => "Anime plugin for Jellyfin. Resolving the metadata for Animes using the Anilist API";
  
  #endregion
  
  #region HasWebPages
  
  public IEnumerable<PluginPageInfo> GetPages() =>
  [
    new()
    {
      Name = Name,
      EmbeddedResourcePath = "Jellyfin.Plugin.Anime.Configuration.config.html",
      DisplayName = "Anime Provider",
    }
  ];
  
  #endregion

  #region Events

  /// <summary>
  ///   Updates the Instance of the AnilistProvider
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  /// <exception cref="NotImplementedException"></exception>
  private void HandleConfigurationChanged(object? sender, BasePluginConfiguration e)
  {
    AniListProvider.Initialize(this.Configuration.AniListEndpoint);
    JikanProvider.Initialize(this.Configuration.JikanEndpoint);
  }

  #endregion
}