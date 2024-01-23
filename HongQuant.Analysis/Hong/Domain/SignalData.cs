namespace HongQuant.Analysis.Hong.Domain;

/// <summary>
/// 计算结果的数据
/// </summary>
public class SignalData
{
    /// <summary>
    /// 合约名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

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
    /// 收益率
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

    /// <summary>
    /// 潜在风险
    /// </summary>
    public double PotentialRisk { get; set; }

    /// <summary>
    /// 日度等级
    /// </summary>
    public int DayLevel { get; set; } = -1;

    /// <summary>
    /// 周度等级
    /// </summary>
    public int WeekLevel { get; set; } = -1;

    /// <summary>
    /// 月度等级
    /// </summary>
    public int MonthLevel { get; set; } = -1;

    /// <summary>
    /// 精准状态
    /// </summary>
    public EntranceState EntranceState { get; set; }

    /// <summary>
    /// 精准累计收益率
    /// </summary>
    public double AccurateTotalYields { get; set; }

    /// <summary>
    /// 开平仓转换
    /// </summary>
    public SignalChange SignalChange { get; set; }

    /// <summary>
    /// 多空状态
    /// </summary>
    public DirectionState Direction { get; set; }

    /// <summary>
    /// 持仓盈亏
    /// </summary>
    public double LossEquity { get; set; }

    /// <summary>
    /// 亏损次数
    /// </summary>
    public int LossCount { get; set; }

    /// <summary>
    /// 最大连续亏损次数
    /// </summary>
    public int MaxLossCount { get; set; }

    /// <summary>
    /// 连亏程度
    /// </summary>
    public double LostDegree { get; set; }

    /// <summary>
    /// 距离上次盈利平仓周期
    /// </summary>
    public int PreProfitCycle { get; set; }

    /// <summary>
    /// 最大盈利周期
    /// </summary>
    public int MaxProfitCycle { get; set; }

    /// <summary>
    /// 距盈程度
    /// </summary>
    public double ProfitCycleDegree { get; set; }

    /// <summary>
    /// 空仓周期
    /// </summary>
    public int EmptyCycle { get; set; }

    /// <summary>
    /// 最大空仓周期
    /// </summary>
    public int MaxEmptyCycle { get; set; }

    /// <summary>
    /// 空仓程度
    /// </summary>
    public double EmptyDegree { get; set; }

    /// <summary>
    /// 持仓周期
    /// </summary>
    public int PositionCycle { get; set; }

    /// <summary>
    /// 最大持仓周期
    /// </summary>
    public int MaxPositionCycle { get; set; }

    /// <summary>
    /// 持仓程度
    /// </summary>
    public double PositionDegree { get; set; }
}

public enum EntranceState
{
    Entrance,
    Out,
    Stop
}


public enum DirectionState
{
    /// <summary>
    /// 转换空仓
    /// </summary>
    None,
    /// <summary>
    /// 转换多头
    /// </summary>
    Long,
    /// <summary>
    /// 转换空头
    /// </summary>
    Short
}