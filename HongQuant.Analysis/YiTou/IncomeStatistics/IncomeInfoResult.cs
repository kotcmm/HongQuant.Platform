namespace HongQuant.Analysis.YiTou.IncomeStatistics;
public class IncomeInfoResult
{
    public IncomeInfoResult(DateTime date, double netWorth, double rateOfReturn,
            double total, double retracementRate)
    {
        Date = date;
        NetWorth = netWorth;
        RateOfReturn = rateOfReturn;
        Total = total;
        RetracementRate = retracementRate;
    }

    /// <summary>
    /// 日期
    /// </summary>
    public DateTime Date { get; }

    /// <summary>
    /// 期末净值
    /// </summary>
    public double NetWorth { get; }

    /// <summary>
    /// 收益率
    /// </summary>
    public double RateOfReturn { get; }

    /// <summary>
    /// 累计收益率
    /// </summary>
    public double Total { get; }

    /// <summary>
    /// 回撤率
    /// </summary>
    public double RetracementRate { get; set; }
}
