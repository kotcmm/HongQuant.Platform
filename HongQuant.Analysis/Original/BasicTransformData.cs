namespace HongQuant.Analysis.Original;

/// <summary>
/// 原始数据转换后的数据
/// </summary>
public class BasicTransformData
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

    /// <summary>
    /// 今日收益率
    /// </summary>
    public double Yields { get; set; }

    /// <summary>
    /// 累计收益率
    /// </summary>
    public double TotalYields { get; set; }

    /// <summary>
    /// 实时回撤
    /// </summary>
    public double WithdrawDeep { get; set; }

    /// <summary>
    /// 最大回撤
    /// </summary>
    public double MaxWithdraw { get; set; }

    /// <summary>
    /// 回撤程度
    /// </summary>
    public double WithdrawLevel { get; set; }
}
