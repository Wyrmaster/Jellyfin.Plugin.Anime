using System.Text.RegularExpressions;
using Jellyfin.Plugin.Anime.Dto;
using Jellyfin.Plugin.Anime.Kitsu;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Anime.Providers;

/// <summary>
///   Provider to resolve Anime Episodes from MyAnimeList
/// </summary>
public class MalEpisodeProvider(IHttpClientFactory httpClientFactory, ILogger<AbstractAnimeProvider<Episode, EpisodeInfo>> logger): AbstractAnimeProvider<Episode, EpisodeInfo>(httpClientFactory, logger)
{
  #region Fields

  private static readonly Regex DetectEpisodeNumber = new(@"\d+", RegexOptions.Compiled);

  #endregion

  #region Overrides

  public override async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
  {
    MetadataResult<Episode> result = (await base.GetMetadata(info, cancellationToken));

    if (!(result.Item?.ProviderIds?.ContainsKey(ProviderNames.AniListProvider) ?? false)
        || !long.TryParse(result.Item.ProviderIds[ProviderNames.AniListProvider], out long id))
    {
      return result;
    }

    Match? match = DetectEpisodeNumber.Matches(Path.GetFileNameWithoutExtension(info.Path)).LastOrDefault(entry => entry.Success);
    if (match != null && int.TryParse(match.Value, out int idx))
    {
      long? kitsuId;
      if (info.SeasonProviderIds.TryGetValue(ProviderNames.KitsuProvider, out string? kitsuIdString)
          && long.TryParse(kitsuIdString, out long kitsuIdValue))
      { 
        kitsuId = kitsuIdValue;
      }
      else
      {
        kitsuId = this.TryReadKitsuIdFromFile(info.Path);
      }

      if (!info.SeasonProviderIds.ContainsKey(ProviderNames.KitsuProvider) && kitsuId.HasValue)
      {
        info.SeasonProviderIds[ProviderNames.KitsuProvider] = kitsuId.ToString();
      }

      try
      {
        EpisodeData? episodeData = await KitsuProvider.Instance.GetEpisodeData
        (
          httpClientFactory.CreateClient($"Kitsu: {Path.GetFileNameWithoutExtension(info.Path)}"),
          kitsuId!.Value,
          idx,
          logger,
          cancellationToken
        );
        if (episodeData != null)
        {
          result.Item.Name = episodeData.Attributes.CanonicalTitle;
          result.Item.Overview = episodeData.Attributes.Description ?? string.Empty;
        }
      }
      catch(Exception e)
      {
        this.Logger?.LogError(e.Message, e); 
      }

      // season number can be set to 1 as animes don't really have seasons in that sense.
      // so for the mal implementation every season has its own id
      result.Item.ParentIndexNumber = info.ParentIndexNumber;
      
      result.Item.IndexNumber = idx;
    }
    result.Item.ParentIndexNumber = 1;

    return result;
  }

  #endregion
}