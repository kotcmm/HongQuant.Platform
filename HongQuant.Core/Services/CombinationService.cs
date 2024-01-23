using DynamicData;
using HongQuant.Core.Models;

namespace HongQuant.Core.Services;
public class CombinationService : ICombinationService
{
    private readonly SourceCache<CombinationScheme, Guid> _schemeCache = new(x => x.Id);
    public IObservableCache<CombinationScheme, Guid> Schemes => _schemeCache;
}
