namespace HongQuant.Platform.ViewModels.Original;
public class BasicTransformDataViewModel
{
    /// <summary>
    /// 信号对应时间
    /// </summary>
    public DateTime SignalDateTime { get; set; }

    /// <summary>
    /// 交易日
    /// </summary>
    public DateOnly TradeDate { get; set; }

    /// <summary>
    /// 权益
    /// </summary>
    public double Equity { get; set; }

    /// <summary>
    /// 今日盈亏
    /// </summary>
    public double TodayProfitAndLoss { get; set; }
}
