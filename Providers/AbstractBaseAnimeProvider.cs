using Jellyfin.Plugin.Anime.Common;
using Jellyfin.Plugin.Anime.GraphQL;
using Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;
using Jellyfin.Plugin.Anime.GraphQL.Dto.SearchAnime;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Anime.Providers;

public abstract class AbstractBaseAnimeProvider<TItemType, TLookupInfoType>
  : IRemoteMetadataProvider<TItemType, TLookupInfoType>
  where TItemType : BaseItem, IHasLookupInfo<TLookupInfoType>, new()
  where TLookupInfoType : ItemLookupInfo, new()
{
  #region Fields
  
  protected readonly HttpClient HttpClient;
  protected readonly ILogger<AbstractBaseAnimeProvider<TItemType, TLookupInfoType>>? Logger;

  #endregion

  #region Constructor

  protected AbstractBaseAnimeProvider(IHttpClientFactory httpClientFactory, ILogger<AbstractBaseAnimeProvider<TItemType, TLookupInfoType>>? logger = null)
  {
    this.HttpClient = httpClientFactory.CreateClient();
    this.Logger = logger;
  }

  #endregion

  #region RemoteMetadataProvider
  
  /// <inheritdoc cref="IRemoteMetadataProvider{TItemType,TLookupInfoType}.Name"/>
  public virtual string Name => ProviderNames.AniListProvider;
  
  /// <inheritdoc cref="IRemoteMetadataProvider{TItemType,TLookupInfoType}.GetSearchResults"/>
  public virtual Task<IEnumerable<RemoteSearchResult>> GetSearchResults(TLookupInfoType searchInfo, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc cref="IRemoteMetadataProvider{TItemType,TLookupInfoType}.GetMetadata"/>
  public abstract Task<MetadataResult<TItemType>> GetMetadata(TLookupInfoType info, CancellationToken cancellationToken);
  
  /// <inheritdoc cref="IRemoteMetadataProvider{TItemType,TLookupInfoType}.GetImageResponse"/>
  public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    => this.HttpClient.GetAsync(url, cancellationToken);

  #endregion
  
  #region Protected Methods

  /// <summary>
  ///   Resolves an Anime from a path of directory/filename
  /// </summary>
  /// <param name="path"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  protected async Task<Media?> ResolveAnime(string path, CancellationToken cancellationToken = default)
  {
    try
    {
      if (this.TryReadAniListIdFromFile(path) is not { } id)
      {
        Medium? medium = await AniListProvider
          .Instance!
          .SearchForAnimeAsync(this.ExtractSearchParamFromPath(Path.GetFileNameWithoutExtension(path)),
            cancellationToken)
          .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (medium?.Id == null)
        {
          return null;
        }

        id = medium.Id.Value;
      }
      
      return await (AniListProvider.Instance?.GetAnimeDataAsync(id, cancellationToken) ?? Task.FromResult<Media?>(null));
    }
    catch (Exception e)
    {
      this.Logger?.LogError(e, $"Error while resolving anime: {path}");
      return null;
    }
  } 

  /// <summary>
  ///   Try to load the AniList id from the filepath
  /// </summary>
  /// <param name="filePath"></param>
  /// <returns></returns>
  protected long? TryReadAniListIdFromFile(string filePath) =>
    Regexes.AniListIdRegex.Matches(filePath).LastOrDefault(entry => entry.Success) is not { } match
    || !long.TryParse(match.Value, out long id)
      ? null
      : id;

  /// <summary>
  ///   Try to load the AniList id from the filepath
  /// </summary>
  /// <param name="filePath"></param>
  /// <returns></returns>
  protected long? TryReadMalIdFromFile(string filePath) =>
    Regexes.MalIdRegex.Matches(filePath).LastOrDefault(entry => entry.Success) is not { } match
    || !long.TryParse(match.Value, out long id)
      ? null
      : id;
  
  /// <summary>
  ///   Parse the filename into a search parameter
  /// </summary>
  /// <param name="fileName"></param>
  /// <returns></returns>
  protected string ExtractSearchParamFromPath(string fileName)
  {
    string searchParam = Regexes.ClearBoxesRegex.Replace(fileName, string.Empty);
    searchParam = Regexes.SeparatorRegex.Replace(searchParam, " ");
    searchParam = Regexes.SeasonEpisodeRegex.Replace(searchParam, string.Empty);
    searchParam = Regexes.QualityRegex.Replace(searchParam, string.Empty);
    searchParam = Regexes.SpacesRegex.Replace(searchParam, " ");
    
    return searchParam;
  }

  #endregion
}