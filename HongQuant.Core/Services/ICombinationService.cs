using DynamicData;
using HongQuant.Core.Models;

namespace HongQuant.Core.Services;
public interface ICombinationService
{
    IObservableCache<CombinationScheme, Guid> Schemes { get; }
}
