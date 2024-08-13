using DynamicData;
using HongQuant.Analysis.RealTime;

namespace HongQuant.Core.Services;
public interface IRealTimeDataService
{
    ISourceCache<RealTimeDataSource, string> RealTimeDataSources { get; }
}
