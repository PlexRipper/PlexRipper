using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class PlexApiClientProgressMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.None)]
    public static partial ServerConnectionCheckStatusProgress ToServerConnectionCheckStatusProgress(this PlexApiClientProgress progress);

    #endregion
}