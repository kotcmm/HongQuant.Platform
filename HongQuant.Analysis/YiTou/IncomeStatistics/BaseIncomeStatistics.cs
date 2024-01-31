namespace HongQuant.Analysis.YiTou.IncomeStatistics;
public abstract class BaseIncomeStatistics
{
    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public double Total { get; set; }

    public int DayCount { get; set; }

    public double MaxTotal { get; set; }

    public DateTime MaxTotalDate { get; set; }

    public double MaxRetracement { get; set; }

    public DateTime RetracementDate { get; set; }

    public NetWorthInfo NetWorthInfo { get; set; } = null!;

    public double LastMultiples { get; set; }


    private readonly List<IncomeInfoResult> _incomeInfoResults;

    private readonly NetWorthStatistics _netWorthStatistics;

    private readonly NetWorthStatistics _netWorthStatisticsResult;

    public BaseIncomeStatistics(IncomeInfoModel incomeInfoModel)
    {
        _incomeInfoResults = new List<IncomeInfoResult>();
        _netWorthStatistics = new NetWorthStatistics();
        _netWorthStatisticsResult = new NetWorthStatistics();

        Start = incomeInfoModel.Date;
        RetracementDate = incomeInfoModel.Date;
        MaxTotalDate = incomeInfoModel.Date;
    }

    public List<IncomeInfoResult> IncomeInfoResults => _incomeInfoResults;

    public void Add(IncomeInfoModel incomeInfoModel)
    {
        var netWorth = _netWorthStatistics.Statistics(incomeInfoModel.RateOfReturn);

        var multiples = GetMultiples(netWorth, incomeInfoModel);

        if (Continue(incomeInfoModel))
        {
            LastMultiples = multiples;

            Total += incomeInfoModel.RateOfReturn * 100 * multiples;

            End = incomeInfoModel.Date;

            if (MaxRetracement > Total)
            {
                MaxRetracement = Total;
                RetracementDate = incomeInfoModel.Date;
            }

            if (MaxTotal < Total)
            {
                MaxTotal = Total;
                MaxTotalDate = incomeInfoModel.Date;
            }

            var netWorthResult = _netWorthStatisticsResult.Statistics(incomeInfoModel.RateOfReturn * multiples);

            var incomeInfoModelClone = new IncomeInfoResult
            (
                incomeInfoModel.Date,
                netWorthResult.NetWorth,
                incomeInfoModel.RateOfReturn,
                Total,
                netWorthResult.RetracementRate
            );

            NetWorthInfo = netWorthResult;

            _incomeInfoResults.Add(incomeInfoModelClone);

            DayCount = _incomeInfoResults.Count;
        }
    }

    public abstract bool Continue(IncomeInfoModel incomeInfoModel);

    public virtual double GetMultiples(NetWorthInfo netWorthInfo, IncomeInfoModel incomeInfoModel)
    {
        return 1;
    }
}
