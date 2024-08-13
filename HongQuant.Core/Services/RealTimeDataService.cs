using DynamicData;
using HongQuant.Analysis.RealTime;

namespace HongQuant.Core.Services;
public class RealTimeDataService : IRealTimeDataService
{
    private readonly SourceCache<RealTimeDataSource, string> _realTimeDataSources = new(x=>x.Product);
    public ISourceCache<RealTimeDataSource, string> RealTimeDataSources => _realTimeDataSources;
}
