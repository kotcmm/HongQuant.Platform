namespace HongQuant.Analysis.RealTime;
public class CombRealTimeData : RealTimeData
{
    public static CombRealTimeData? From(RealTimeData? realTimeData)
    {
        if(realTimeData == null) return null;

        return new CombRealTimeData
        {
            SignalDateTime = realTimeData.SignalDateTime,
            Equity = realTimeData.Equity,
            Position = realTimeData.Position,
        };
    }

    /// <summary>
    /// 多头持仓
    /// </summary>
    public int LongPosition { get; set; }

    /// <summary>
    /// 空头持仓
    /// </summary>
    public int ShortPosition { get; set; }
}
