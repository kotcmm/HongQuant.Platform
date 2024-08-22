namespace HongQuant.Analysis.RealTime;
public class CombRealTimeDataSource : RealTimeDataSource
{
    private readonly Dictionary<string, IEnumerable<string>> _histories = [];

    public void OnProduct(IEnumerable<string> products)
    {
        foreach (var product in products)
        {
            _histories[product] = [];
        }
    }

    public void OnHistoryData(string product, IEnumerable<string> datas)
    {
        _histories[product] = datas;

        if (_histories.All(x => x.Value.Any()))
        {
            var mergeDatas = GetMergeDatas();

            OnHistoryData(mergeDatas);
        }
    }

    private IEnumerable<RealTimeData> GetMergeDatas()
    {
        return _histories
                        .Values.SelectMany(x => x)
                        .Select(RealTimeData.BuildData)
                        .OfType<RealTimeData>()
                        .Where(x => x.Verify())
                        .GroupBy(x => x.SignalDateTime)
                        .Select(x => new RealTimeData
                        {
                            SignalDateTime = x.Key,
                            Equity = x.Sum(y => y.Equity),
                            Position = x.Count(y => y.Position != 0),
                            LongPosition = x.Count(y => y.Position == 1),
                            ShortPosition = x.Count(y => y.Position == -1)
                        })
                        .OrderBy(x => x.SignalDateTime);
    }
}
