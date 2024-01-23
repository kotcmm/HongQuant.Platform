namespace HongQuant.Analysis.Hong.Domain;

/// <summary>
/// 计算的配置数据
/// </summary>
public class SignalDataConfig
{
    public double Principal { get; set; }

    /// <summary>
    /// 日度等级
    /// </summary>
    public int DayMaxLevel { get; set; }

    /// <summary>
    /// 周度等级
    /// </summary>
    public int WeekMaxLevel { get; set; }

    /// <summary>
    /// 月度等级
    /// </summary>
    public int MonthMaxLevel { get; set; }

    /// <summary>
    /// 开仓条件
    /// </summary>
    public double Entrance { get; set; }

    /// <summary>
    /// 平仓条件
    /// </summary>
    public double Quit { get; set; }

    /// <summary>
    /// 止损条件
    /// </summary>
    public double Stop { get; set; }

    /// <summary>
    /// 优选边界
    /// </summary>
    public int MaxCombo { get; set; }

    public int SelectTop { get; set; }
}
