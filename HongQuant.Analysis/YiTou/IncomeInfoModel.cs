namespace HongQuant.Analysis.YiTou;
public class IncomeInfoModel
{
    public IncomeInfoModel(DateTime date, double netWorth, double rateOfReturn)
    {
        Date = date;
        NetWorth = netWorth;
        RateOfReturn = rateOfReturn;
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
}
