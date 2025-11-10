using Jellyfin.Plugin.Anime.Common;
using Jellyfin.Plugin.Anime.Extensions;
using Jellyfin.Plugin.Anime.GraphQL.Dto.QueryAnime;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Anime.Providers;

public abstract class AbstractAnimeProvider<TItemType, TLookupInfoType>
  : AbstractBaseAnimeProvider<TItemType, TLookupInfoType>
  where TItemType : Video, IHasLookupInfo<TLookupInfoType>, new()
  where TLookupInfoType : ItemLookupInfo, new()
{
  #region Constructor

  protected AbstractAnimeProvider(IHttpClientFactory httpClientFactory, ILogger<AbstractBaseAnimeProvider<TItemType, TLookupInfoType>>? logger = null) : base(httpClientFactory, logger)
  {
  }

  #endregion

  #region Overrides

  /// <inheritdoc/>
  public override async Task<MetadataResult<TItemType>> GetMetadata(TLookupInfoType info, CancellationToken cancellationToken)
  {
    this.Logger?.LogInformation($"Resolving metadata for {info.Path}");
    
    Media? anime = await this.ResolveAnime(info.Path, cancellationToken);
    if (anime == null)
    {
      return new NoMetaDataFound<TItemType>();
    }
      
    MetadataResult<TItemType> metadataResult = new()
    {
      HasMetadata = true,
      Item = anime.ToVideo<TItemType>(),
      Provider = ProviderNames.AniListProvider,
      People = await anime.ToPersonInfos(cancellationToken: cancellationToken).ToListAsync(cancellationToken: cancellationToken),
      RemoteImages = [
        (
          anime.CoverImage.ExtraLarge ?? anime.CoverImage.Large ?? anime.CoverImage.Medium,
          ImageType.Primary
        )
      ]
    };
      
    return metadataResult;
  }

  #endregion
}