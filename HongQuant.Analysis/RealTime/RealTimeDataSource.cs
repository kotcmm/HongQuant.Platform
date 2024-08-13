using DynamicData;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace HongQuant.Analysis.RealTime;
public class RealTimeDataSource
{
    public string Product { get; set; } = string.Empty;

    public string Strategy { get; set; } = string.Empty;

    private readonly Subject<AnalyzedData> _subject = new();

    public IObservable<IChangeSet<AnalyzedData, DateTime>> AnalyzedDatas => _subject.ToObservableChangeSet(x=>x.SignalDateTime);

    public void OnHistoryData(IEnumerable<string> datas)
    {
        var realTimeDatas = datas
            .Select(RealTimeData.BuildData)
            .OfType<RealTimeData>()
            .Where(x => x.Verify());

        foreach (var realTimeData in realTimeDatas)
        {
            OnNext(realTimeData);
        }
    }

    public void OnRealTimeData(string line)
    {
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

        _subject.OnNext(analyzedData);
    }
}
