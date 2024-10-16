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
    /// 持仓格式化
    /// </summary>
    public string PositionFormat => $"{Position},{LongPosition},{ShortPosition}";

    /// <summary>
    /// 持仓
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 多头持仓
    /// </summary>
    public int LongPosition { get; set; }

    /// <summary>
    /// 空头持仓
    /// </summary>
    public int ShortPosition { get; set; }

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

    /// <summary>
    /// 上轨，最大的权益
    /// </summary>
    public double MaxEquity { get; set;  }

    /// <summary>
    /// 下轨，最小的权益
    /// </summary>
    public double MinEquity { get; set;  }

    /// <summary>
    /// 中轨，上下的平均
    /// </summary>
    public double AvgEquity { get; set;  }

    /// <summary>
    /// 是否有信号
    /// </summary>
    public bool HasSignal { get; set; }

    /// <summary>
    /// 累计盈利
    /// </summary>
    public double TotalProfit { get; set; }

    /// <summary>
    /// 信号累计盈利
    /// </summary>
    public double TotalSignalProfit { get; set; }
}
