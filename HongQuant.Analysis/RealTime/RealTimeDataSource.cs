using DynamicData;
using System.Diagnostics.CodeAnalysis;
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

        Clean();

        var realTimeDatas = datas
            .Select(RealTimeData.BuildData)
            .OfType<RealTimeData>()
            .Where(x => x.Verify())
            .OrderBy(x=>x.SignalDateTime);

        foreach (var realTimeData in realTimeDatas)
        {
            OnNextHistory(realTimeData);
        }

        _initialization = true;
    }

    public void OnRealTimeData(string line)
    {
        if (!_initialization) return;

        var realTimeData = RealTimeData.BuildData(line);
        var preRealTimeData = RealTimeData.PreBuildData(line);

        if (realTimeData != null && realTimeData.Verify() &&
            preRealTimeData != null && preRealTimeData.Verify())
        {
            OnNexRealTimeData(realTimeData, preRealTimeData);
        }
    }

    private AnalyzedData? _firstData;
    private AnalyzedData? _preData;

    private void OnNextHistory(RealTimeData realTimeData)
    {
        var analyzedData = BuildAnalyzedData(realTimeData);

        _firstData ??= analyzedData;

        TryCalculation(analyzedData, _firstData, _preData);

        if (_preData == null || _preData.SignalDateTime < analyzedData.SignalDateTime)
        {
            _preData = analyzedData;
        }
    }

    private void OnNexRealTimeData(RealTimeData realTimeData, RealTimeData preRealTimeData)
    {
        if (_firstData == null) return;

        var analyzedData = BuildAnalyzedData(realTimeData);

        TryCalculation(analyzedData, _firstData, _preData);

        if (_preData == null || _preData.SignalDateTime < preRealTimeData.SignalDateTime)
        {
            _preData = analyzedData;
        }
    }

    private AnalyzedData BuildAnalyzedData(RealTimeData realTimeData)
    {
        return new AnalyzedData
        {
            Product = Product,
            SignalDateTime = realTimeData.SignalDateTime!.Value,
            Equity = realTimeData.Equity!.Value,
            Position = realTimeData.Position!.Value,
        };  
    }

    private void TryCalculation(AnalyzedData analyzedData, AnalyzedData firstData, AnalyzedData? preData)
    {
        analyzedData.AttachTodayProfitAndLoss(firstData, preData);

        _analyzedDatas.AddOrUpdate(analyzedData);
    }


    [MemberNotNull(nameof(_firstData))]
    private void TrySetFirst(AnalyzedData analyzedData)
    {
        _firstData ??= analyzedData;
    }

    private void Clean()
    {
        _firstData = null;
        _preData = null;
    }
}
