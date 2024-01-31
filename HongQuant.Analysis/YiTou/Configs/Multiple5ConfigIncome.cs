namespace HongQuant.Analysis.YiTou.Configs;
public class Multiple5ConfigIncome : IConfigIncome
{
    /// <summary>
    /// 设置最多盈利
    /// </summary>
    public double MaxIncome { get; set; } = 100;

    /// <summary>
    /// 初始倍数
    /// </summary>
    public double Start { get; set; } = 1;

    /// <summary>
    /// 倍数增量
    /// </summary>
    public double Increment { get; set; } = 1;

    /// <summary>
    /// 倍数上限
    /// </summary>
    public double Max { get; set; } = 6;

    /// <summary>
    /// 倍数增加间隔
    /// </summary>
    public int Month { get; set; } = 3;
    /// <summary>
    /// 是否有低仓
    /// </summary>
    public bool Have { get; set; }

    /// <summary>
    /// 复利系数
    /// </summary>
    public double NetWorthIncrement { get; set; } = 0.2;

    public bool HaveNetWorth { get; set; }

    public bool UseTime { get; set; }

    public bool UseIncome { get; set; }

    public int MaxTime { get; set; } = 36;
}
