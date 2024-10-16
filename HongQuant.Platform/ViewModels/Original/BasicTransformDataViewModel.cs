using HongQuant.Analysis.Original;

namespace HongQuant.Platform.ViewModels.Original;
public class BasicTransformDataViewModel(BasicTransformData item)
{
    /// <summary>
    /// 品种
    /// </summary>
    public string Product { get; set; } = item.Product;

    /// <summary>
    /// 信号对应时间
    /// </summary>
    public DateTime SignalDateTime { get; set; } = item.SignalDateTime;

    /// <summary>
    /// 交易日
    /// </summary>
    public DateOnly TradeDate { get; set; } = item.TradeDate;

    /// <summary>
    /// 权益
    /// </summary>
    public double Equity { get; set; } = item.Equity;

    /// <summary>
    /// 持仓
    /// </summary>
    public int Position { get; set; } = item.Position;

    public string PositionFormat { get; set; } = item.PositionFormat;

    /// <summary>
    /// 今日盈亏
    /// </summary>
    public double TodayProfitAndLoss { get; set; } = item.TodayProfitAndLoss;

    /// <summary>
    /// 今日收益率
    /// </summary>
    public double Yields { get; set; } = item.Yields;

    /// <summary>
    /// 累计收益率
    /// </summary>
    public double TotalYields { get; set; } = item.TotalYields;

    /// <summary>
    /// 实时回撤
    /// </summary>
    public double WithdrawDeep { get; set; } = item.WithdrawDeep;

    /// <summary>
    /// 最大回撤
    /// </summary>
    public double MaxWithdraw { get; set; } = item.MaxWithdraw;

    /// <summary>
    /// 回撤程度
    /// </summary>
    public double WithdrawLevel { get; set; } = item.WithdrawLevel;

    /// <summary>
    /// 上轨，最大的权益
    /// </summary>
    public double MaxEquity { get; set; } = item.MaxEquity;

    /// <summary>
    /// 下轨，最小的权益
    /// </summary>
    public double MinEquity { get; set; } = item.MinEquity;

    /// <summary>
    /// 中轨，上下的平均
    /// </summary>
    public double AvgEquity { get; set; } = item.AvgEquity;

    /// <summary>
    /// 是否有信号
    /// </summary>
    public bool HasSignal { get; set; } = item.HasSignal;

    /// <summary>
    /// 累计盈利
    /// </summary>
    public double TotalProfit { get; set; } = item.TotalProfit;

    /// <summary>
    /// 信号累计盈利
    /// </summary>
    public double TotalSignalProfit { get; set; } = item.TotalSignalProfit;
}
