namespace HongQuant.Analysis.RealTime;
public class AnalyzedData
{
    /// <summary>
    /// 品种
    /// </summary>
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// 信号对应时间
    /// </summary>
    public DateTime SignalDateTime { get; set; }

    /// <summary>
    /// 持仓
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 权益
    /// </summary>
    public double Equity { get; set; }
}
