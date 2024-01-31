namespace HongQuant.Analysis.YiTou.IncomeStatistics;
public class MaxNetWorthInfo
{
    public MaxNetWorthInfo(DateTime date, double maxNetWorth)
    {
        Date = date;
        MaxNetWorth = maxNetWorth;
    }

    public DateTime Date { get; }

    public double MaxNetWorth { get; }
}
