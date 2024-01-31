namespace HongQuant.Analysis.YiTou.IncomeStatistics;
public class MultiplesInfo
{
    public MultiplesInfo(DateTime date, double multiples)
    {
        Date = date;
        Multiples = multiples;
    }

    public DateTime Date { get; }

    public double Multiples { get; }
}
