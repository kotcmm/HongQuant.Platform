namespace HongQuant.Analysis.Original;

/// <summary>
/// 原始数据转换后的数据
/// </summary>
public class BasicTransformData
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
    /// 初始权益
    /// </summary>
    public double StartEquity { get; set; }

    /// <summary>
    /// 权益
    /// </summary>
    public double Equity { get; set; }

    /// <summary>
    /// 今日盈亏
    /// </summary>
    public double TodayProfitAndLoss { get; set; }
}
