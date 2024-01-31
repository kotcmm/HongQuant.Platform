using HongQuant.Analysis.YiTou.Configs;

namespace HongQuant.Analysis.YiTou.IncomeStatistics;
public static class IncomeStatisticsFormula
{
    public static IEnumerable<BaseIncomeStatistics> ToIncomeStatistics(this IEnumerable<IncomeInfoModel> source, AlgorithmType algorithmType, IConfigIncome? config)
    {
        if (config != null)
        {
            using var e = source.GetEnumerator();

            var nextTime = DateTime.MinValue;

            var list = new List<BaseIncomeStatistics>();

            while (e.MoveNext())
            {
                var data = e.Current;

                if (data.Date >= nextTime)
                {
                    nextTime = Next(data.Date);

                    BaseIncomeStatistics statistics = algorithmType switch
                    {
                        AlgorithmType.Cycle => new CycleIncomeStatistics(data, (CycleConfigIncome)config),
                        AlgorithmType.Multiple5 => new Multiple5IncomeStatistics(data, (Multiple5ConfigIncome)config),
                        _ => throw new NotImplementedException(),
                    };

                    list.Add(statistics);

                    yield return statistics;
                }

                foreach (var item in list)
                {
                    item.Add(data);
                }
            }
        }
    }

    public static DateTime Next(DateTime now, int month = 1)
    {
        return now.AddMonths(month).AddDays(-now.Day + 1);
    }
}
