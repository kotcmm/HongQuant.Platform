namespace HongQuant.Analysis.RealTime;
public class SingleRealTimeDataSource : RealTimeDataSource<RealTimeData,AnalyzedData>
{
    public void OnHistoryData(IEnumerable<string> datas)
    {
        var historyDatas = datas
            .Select(RealTimeData.BuildData)
            .OfType<RealTimeData>()
            .Where(x => x.Verify())
            .OrderBy(x => x.SignalDateTime)
            .ToList();

        OnHistoryData(historyDatas);
    }

    protected override (RealTimeData?, RealTimeData?) BuildRealTimeData(string line)
    {
        var realTimeData = RealTimeData.BuildData(line);
        var preRealTimeData = RealTimeData.PreBuildData(line);

        return (realTimeData, preRealTimeData);
    }

    protected override AnalyzedData BuildAnalyzedData(RealTimeData realTimeData)
    {
        return new AnalyzedData
        {
            Product = Product,
            SignalDateTime = realTimeData.SignalDateTime!.Value,
            Equity = realTimeData.Equity!.Value,
            Position = realTimeData.Position!.Value
        };
    }
}
