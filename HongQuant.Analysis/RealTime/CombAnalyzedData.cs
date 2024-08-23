namespace HongQuant.Analysis.RealTime;
public class CombAnalyzedData : AnalyzedData
{
    /// <summary>
    /// 多头持仓
    /// </summary>
    public int LongPosition { get; set; }

    /// <summary>
    /// 空头持仓
    /// </summary>
    public int ShortPosition { get; set; }
}
