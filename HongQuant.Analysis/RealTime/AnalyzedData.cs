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

    /// <summary>
    /// 今日盈亏
    /// </summary>
    public double TodayProfitAndLoss { get; set; }

    /// <summary>
    /// 计算今日盈亏
    /// 今日盈亏=今日最后一条权益-昨天最后一条权益
    /// </summary>
    /// <param name="firstData"></param>
    /// <param name="preData"></param>
    public void AttachTodayProfitAndLoss(AnalyzedData firstData, AnalyzedData? preData)
    {
        if (preData == null)
        {
            TodayProfitAndLoss = Equity - firstData.Equity;
        }
        else
        {
            TodayProfitAndLoss = Math.Round(Equity - preData.Equity, 2);
        }
    }
}
