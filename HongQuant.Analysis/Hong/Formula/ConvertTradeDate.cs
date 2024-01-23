namespace HongQuant.Analysis.Hong.Formula;
public class ConvertTradeDate
{
    private readonly Queue<IGrouping<DateOnly, DateTime>> _historyTradeDates;

    public ConvertTradeDate(IEnumerable<DateTime> dateTimes)
    {
        var datefilter = dateTimes.GroupBy(DateOnly.FromDateTime)
            .Where(x => x.Any(x => x.Hour > 8));

        _historyTradeDates = new Queue<IGrouping<DateOnly, DateTime>>(datefilter);
    }

    public DateOnly GetTradeDate(DateTime dateTime)
    {
        return dateTime.Hour switch
        {
            < 18 and > 8 => Day(dateTime),
            > 18 => Night(dateTime),
            < 8 => BeforeDawn(dateTime),
            _ => throw new NotImplementedException(),
        };
    }

    private const int Year = 2018;//2018年后就用默认的计算交易日

    private static DateOnly Day(DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }

    private DateOnly Night(DateTime dateTime)
    {
        return dateTime.Year > Year ? DefaultNight(dateTime) : 
            ByHistoryNight(dateTime);
    }

    private DateOnly ByHistoryNight(DateTime dateTime)
    {
        var tradeDate = DateOnly.FromDateTime(dateTime);

        var group = _historyTradeDates.Peek();

        while (group.Key <= tradeDate)
        {
            var dequeue = _historyTradeDates.Dequeue();

            group = dequeue.Key == tradeDate ? _historyTradeDates.Peek() : dequeue;
        }

        return group.Key;
    }

    private DateOnly BeforeDawn(DateTime dateTime)
    {
        return dateTime.Year > Year ? DefaultBeforeDawn(dateTime) :
            ByHistoryBeforeDawn(dateTime);
    }

    private DateOnly ByHistoryBeforeDawn(DateTime dateTime)
    {
        var tradeDate = DateOnly.FromDateTime(dateTime);

        var group = _historyTradeDates.Peek();

        while (group.Key < tradeDate)
        {
            var peek = _historyTradeDates.Peek();

            group = peek.Key == tradeDate ? peek : _historyTradeDates.Dequeue();
        }

        return group.Key;
    }

    private static DateOnly DefaultNight(DateTime dateTime)
    {
        if (dateTime.DayOfWeek == DayOfWeek.Friday)
        {
            return DateOnly.FromDateTime(dateTime.AddDays(3));
        }
        return DateOnly.FromDateTime(dateTime.AddDays(1));
    }

    private static DateOnly DefaultBeforeDawn(DateTime dateTime)
    {
        if (dateTime.DayOfWeek == DayOfWeek.Saturday)
        {
            return DateOnly.FromDateTime(dateTime.AddDays(2));
        }
        return DateOnly.FromDateTime(dateTime);
    }
}
