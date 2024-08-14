using DynamicData;
using System.Reactive.Linq;

namespace HongQuant.Analysis.RealTime;
public class RealTimeDataSource
{
    private bool _initialization = false;

    public string Product { get; set; } = string.Empty;

    public string Strategy { get; set; } = string.Empty;

    private readonly SourceCache<AnalyzedData, DateTime> _analyzedDatas = new(x => x.SignalDateTime);
    public ISourceCache<AnalyzedData, DateTime> AnalyzedDatas => _analyzedDatas;

    public void OnHistoryData(IEnumerable<string> datas)
    {
        _initialization = false;

        var realTimeDatas = datas
            .Select(RealTimeData.BuildData)
            .OfType<RealTimeData>()
            .Where(x => x.Verify());

        foreach (var realTimeData in realTimeDatas)
        {
            OnNext(realTimeData);
        }

        _initialization = true;
    }

    public void OnRealTimeData(string line)
    {
        if (!_initialization) return;

        var realTimeData = RealTimeData.BuildData(line);

        if (realTimeData != null && realTimeData.Verify())
        {
            OnNext(realTimeData);
        }
    }

    private void OnNext(RealTimeData realTimeData)
    {
        var analyzedData = new AnalyzedData
        {
            Product = Product,
            SignalDateTime = realTimeData.SignalDateTime!.Value,
            Equity = realTimeData.Equity!.Value,
            Position = realTimeData.Position!.Value,
        };

        _analyzedDatas.AddOrUpdate(analyzedData);
    }
}
