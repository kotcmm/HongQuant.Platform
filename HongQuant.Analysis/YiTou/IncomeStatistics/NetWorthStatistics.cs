namespace HongQuant.Analysis.YiTou.IncomeStatistics;
public class NetWorthStatistics
{
    /// <summary>
    /// 上一个期末净值
    /// </summary>
    private double _preNetWorth = 1;

    /// <summary>
    /// 最大期末净值
    /// </summary>
    private double _maxNetWorth;

    /// <summary>
    /// 进行统计
    /// </summary>
    /// <param name="rateOfReturn"></param>
    public NetWorthInfo Statistics(double rateOfReturn)
    {
        double netWorth = rateOfReturn + _preNetWorth;

        _preNetWorth = netWorth;

        if (_maxNetWorth < netWorth)
        {
            _maxNetWorth = netWorth;
        }

        double retracementRate = (netWorth - _maxNetWorth) * 100f;

        return new NetWorthInfo(netWorth, retracementRate);
    }
}

public class NetWorthInfo
{
    public NetWorthInfo(double netWorth, double retracementRate)
    {
        NetWorth = netWorth;
        RetracementRate = retracementRate;
    }

    public double NetWorth { get; }

    public double RetracementRate { get; }
}
