using DynamicData;
using System.Reactive.Linq;

namespace HongQuant.Analysis.RealTime;
public abstract class RealTimeDataSource<TRealTimeData, TAnalyzedData> 
    where TRealTimeData: RealTimeData
    where TAnalyzedData : AnalyzedData
{
    private bool _initialization = false;

    private TAnalyzedData? _firstData;
    private TAnalyzedData? _preData;
    private ConvertTradeDate? _tradeDate;

    public string Product { get; set; } = string.Empty;

    public string Strategy { get; set; } = string.Empty;

    private readonly SourceCache<TAnalyzedData, DateTime> _analyzedDatas = new(x => x.SignalDateTime);
    public ISourceCache<TAnalyzedData, DateTime> AnalyzedDatas => _analyzedDatas;

    public void OnHistoryData(IEnumerable<TRealTimeData> historyDatas)
    {
        _initialization = false;

        Clean();

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

        var (realTimeData, preRealTimeData) = BuildRealTimeData(line);

        if (realTimeData != null && realTimeData.Verify() &&
            preRealTimeData != null && preRealTimeData.Verify())
        {
            OnNexRealTimeData(realTimeData, preRealTimeData);
        }
    }

    protected abstract (TRealTimeData?, TRealTimeData?) BuildRealTimeData(string line);

    private void OnNextHistory(TRealTimeData realTimeData)
    {
        var analyzedData = BuildAnalyzedData(realTimeData);

        _firstData ??= analyzedData;

        TryCalculation(analyzedData, _firstData, _preData);

        TrySetPreData(analyzedData, analyzedData.SignalDateTime);
    }

    private void OnNexRealTimeData(TRealTimeData realTimeData, TRealTimeData preRealTimeData)
    {
        if (_firstData == null) return;

        var analyzedData = BuildAnalyzedData(realTimeData);

        TryCalculation(analyzedData, _firstData, _preData);

        TrySetPreData(analyzedData, preRealTimeData.SignalDateTime!.Value);
    }

    protected abstract TAnalyzedData BuildAnalyzedData(TRealTimeData realTimeData);

    private void TryCalculation(TAnalyzedData analyzedData, TAnalyzedData firstData, TAnalyzedData? preData)
    {
        analyzedData.AttachTradeDayAnWeekOfYear(_tradeDate);
        analyzedData.AttachProfitAndLoss(firstData, preData);
        
        _analyzedDatas.AddOrUpdate(analyzedData);
    }

    private void TrySetPreData(TAnalyzedData analyzedData, DateTime signalDateTime)
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
