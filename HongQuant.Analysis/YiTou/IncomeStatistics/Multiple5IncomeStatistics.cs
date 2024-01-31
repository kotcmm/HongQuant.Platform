using HongQuant.Analysis.YiTou.Configs;

namespace HongQuant.Analysis.YiTou.IncomeStatistics;
internal class Multiple5IncomeStatistics : BaseIncomeStatistics, IMultiples, IMaxNetWorths
{
    private DateTime _nextTime = DateTime.MinValue;

    private readonly Multiple5ConfigIncome _config;

    private double _multiple;

    private double _maxNetWorth;

    private readonly DateTime _stopTime = DateTime.MinValue;

    public Multiple5IncomeStatistics(IncomeInfoModel incomeInfoModel,
        Multiple5ConfigIncome config) : base(incomeInfoModel)
    {
        _config = config;

        _multiple = config.Start;

        _nextTime = Next(incomeInfoModel.Date, _config.Month);

        _stopTime = Next(incomeInfoModel.Date, _config.MaxTime);

        Multiples = [];

        MaxNetWorthInfos = [];
    }


    private static DateTime Next(DateTime now, int month = 1)
    {
        return now.AddMonths(month).AddDays(-now.Day + 1);
    }

    public List<MultiplesInfo> Multiples { get; }

    public List<MaxNetWorthInfo> MaxNetWorthInfos { get; }

    public override bool Continue(IncomeInfoModel incomeInfoModel)
    {
        if (_config.UseTime && !_config.UseIncome)
        {
            return _stopTime > incomeInfoModel.Date;
        }

        if (_config.UseTime && _config.UseIncome)
        {
            return _config.MaxIncome > Total && _stopTime > incomeInfoModel.Date;
        }

        return _config.MaxIncome > Total;
    }

    public override double GetMultiples(NetWorthInfo netWorthInfo,
        IncomeInfoModel incomeInfoModel)
    {
        if (!Continue(incomeInfoModel))
        {
            return 0;
        }

        var multiple = _multiple;

        if (_nextTime <= incomeInfoModel.Date)
        {
            if (multiple < _config.Max)
            {
                _multiple = Math.Min(_config.Max, multiple + _config.Increment);
            }
            else if (_config.HaveNetWorth)
            {
                var surplus = multiple - _config.Max;

                //累计增量倍数*复利系数 + 1 + 最大倍数 * 基础系数
                //基础系数 = 复利系数 - 0.1
                var maxNetWorth = (surplus + 1) * _config.NetWorthIncrement + 1 + _config.Max * (_config.NetWorthIncrement - 0.1);

                if (maxNetWorth <= NetWorthInfo.NetWorth)
                {
                    _multiple = multiple + _config.Increment;
                }

                _maxNetWorth = maxNetWorth;
            }

            _nextTime = Next(incomeInfoModel.Date, _config.Month);
        }

        multiple = _config.Have ? Math.Max(1, multiple) : multiple;

        if (_config.HaveNetWorth)
        {
            MaxNetWorthInfos.Add(new MaxNetWorthInfo(incomeInfoModel.Date, _maxNetWorth));
        }

        Multiples.Add(new MultiplesInfo(incomeInfoModel.Date, multiple));

        return multiple;
    }
}
