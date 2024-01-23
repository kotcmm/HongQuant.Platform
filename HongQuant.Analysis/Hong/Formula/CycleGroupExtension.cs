using HongQuant.Analysis.Hong.Domain;

namespace HongQuant.Analysis.Hong.Formula;
internal static class CycleGroupExtension
{
    /// <summary>
    /// 分组到天周期
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<CycleGroup> GroupDay(this IEnumerable<SignalData> source)
    {
        return source.GroupBy(d => d.TradeDate)
            .Select(d => new CycleGroup
            {
                KeyDateTime = d.Key,
                Children = [.. d]
            });
    }

    /// <summary>
    /// 分组到周
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<CycleGroup> GroupWeek(this IEnumerable<SignalData> source)
    {
        return source.GroupBy(d => d.TradeDate.AddDays(DayOfWeek.Monday - d.TradeDate.DayOfWeek))

            .Select(d => new CycleGroup
            {
                KeyDateTime = d.Key,
                Children = [.. d]
            });
    }

    /// <summary>
    /// 分组到月周期
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<CycleGroup> GroupMonth(this IEnumerable<SignalData> source)
    {
        return source.GroupBy(d => d.TradeDate.AddDays(1 - d.TradeDate.Day))

            .Select(d => new CycleGroup
            {
                KeyDateTime = d.Key,
                Children = [.. d]
            });
    }
}
