namespace HongQuant.Analysis.RealTime;
public class CombRealTimeDataSource : RealTimeDataSource<CombRealTimeData,CombAnalyzedData>
{
    private readonly Dictionary<string, CombRealTimeDataProx> _histories = [];

    public void OnProduct(IEnumerable<string> products)
    {
        foreach (var product in products)
        {
            _histories[product] = new CombRealTimeDataProx();
        }
    }

    public void OnHistoryData(string product, IEnumerable<string> datas)
    {
        if (_histories.TryGetValue(product, out var history)) 
        {
            history.SetData(datas);
        }

        if (_histories.All(x => x.Value.HasDatas()))
        {
            //var mergeDatas = GetMergeDatas();

            //OnHistoryData(mergeDatas);
        }
    }

    //private IEnumerable<CombRealTimeData> GetMergeDatas()
    //{
    //    return _histories.Values.SelectMany(x => x)
    //                    .Select(RealTimeData.BuildData)
    //                    .OfType<RealTimeData>()
    //                    .Where(x => x.Verify())
    //                    .GroupBy(x => x.SignalDateTime)
    //                    .Select(x => new CombRealTimeData
    //                    {
    //                        SignalDateTime = x.Key,
    //                        Equity = x.Sum(y => y.Equity),
    //                        Position = x.Count(y => y.Position != 0),
    //                        LongPosition = x.Count(y => y.Position == 1),
    //                        ShortPosition = x.Count(y => y.Position == -1)
    //                    })
    //                    .OrderBy(x => x.SignalDateTime);
    //}

    protected override (CombRealTimeData?, CombRealTimeData?) BuildRealTimeData(string line)
    {
        var realTimeData = CombRealTimeData.From(RealTimeData.BuildData(line));
        var preRealTimeData = CombRealTimeData.From(RealTimeData.PreBuildData(line));

        return (realTimeData, preRealTimeData);
    }

    protected override CombAnalyzedData BuildAnalyzedData(CombRealTimeData realTimeData)
    {
        return new CombAnalyzedData
        {
            Product = Product,
            SignalDateTime = realTimeData.SignalDateTime!.Value,
            Equity = realTimeData.Equity!.Value,
            Position = realTimeData.Position!.Value,
            LongPosition = realTimeData.LongPosition,
            ShortPosition = realTimeData.ShortPosition,
        };
    }
}
