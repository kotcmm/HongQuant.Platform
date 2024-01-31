using HongQuant.Analysis.YiTou.Configs;

namespace HongQuant.Analysis.YiTou.IncomeStatistics;
internal class CycleIncomeStatistics : BaseIncomeStatistics
{
    private readonly DateTime _stop;

    public CycleIncomeStatistics(IncomeInfoModel incomeInfoModel,
            CycleConfigIncome config) : base(incomeInfoModel)
    {
        _stop = Start.AddMonths(config.Month).AddDays(-Start.Day);
    }

    public override bool Continue(IncomeInfoModel incomeInfoModel)
    {
        return _stop >= incomeInfoModel.Date;
    }
}
