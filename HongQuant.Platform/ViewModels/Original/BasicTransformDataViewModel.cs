using HongQuant.Analysis.Original;

namespace HongQuant.Platform.ViewModels.Original;
public class BasicTransformDataViewModel(BasicTransformData item)
{

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
}
