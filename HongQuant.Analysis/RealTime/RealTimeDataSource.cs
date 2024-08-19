using DynamicData;
using System.Reactive.Linq;

namespace HongQuant.Analysis.RealTime;
public class RealTimeDataSource
{
    private bool _initialization = false;

    private AnalyzedData? _firstData;
    private AnalyzedData? _preData;
    private ConvertTradeDate? _tradeDate;

    public string Product { get; set; } = string.Empty;

    public string Strategy { get; set; } = string.Empty;

    private readonly SourceCache<AnalyzedData, DateTime> _analyzedDatas = new(x => x.SignalDateTime);
    public ISourceCache<AnalyzedData, DateTime> AnalyzedDatas => _analyzedDatas;

    public void OnHistoryData(IEnumerable<string> datas)
    {
        _initialization = false;

        Clean();

        var historyDatas = datas
            .Select(RealTimeData.BuildData)
            .OfType<RealTimeData>()
            .Where(x => x.Verify())
            .OrderBy(x=>x.SignalDateTime)
            .ToList();

        var dateTimes = historyDatas
            .Select(x => x.SignalDateTime)
            .OfType<DateTime>();

        _tradeDate = new ConvertTradeDate(dateTimes);

        foreach (var historyData in historyDatas)
        {
            OnNextHistory(historyData);
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

    private void OnNextHistory(RealTimeData realTimeData)
    {
        var analyzedData = BuildAnalyzedData(realTimeData);

        _firstData ??= analyzedData;

        TryCalculation(analyzedData, _firstData, _preData);

        TrySetPreData(analyzedData, analyzedData.SignalDateTime);
    }

    private void OnNexRealTimeData(RealTimeData realTimeData, RealTimeData preRealTimeData)
    {
        if (_firstData == null) return;

        var analyzedData = BuildAnalyzedData(realTimeData);

        TryCalculation(analyzedData, _firstData, _preData);

        TrySetPreData(analyzedData, preRealTimeData.SignalDateTime!.Value);
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
        analyzedData.AttachTradeDay(_tradeDate);
        analyzedData.AttachProfitAndLoss(firstData, preData);
        
        _analyzedDatas.AddOrUpdate(analyzedData);
    }

    private void TrySetPreData(AnalyzedData analyzedData, DateTime signalDateTime)
    {
        if (_preData == null || _preData.SignalDateTime < signalDateTime)
        {
            _preData = analyzedData;
        }
    }

    private void Clean()
    {
        _firstData = null;
        _preData = null;
    }
}
