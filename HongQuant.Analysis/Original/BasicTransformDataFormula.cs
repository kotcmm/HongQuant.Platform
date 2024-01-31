using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HongQuant.Analysis.Original;
public static class BasicTransformDataFormula
{
    public static IEnumerable<BasicTransformData> TransformBasicDatas(this IEnumerable<BasicData> basicDatas)
    {
        return basicDatas
            .TypeOfTransformData()
            .SkipAndInitStartEquity()
            .AttachTradeDay()
            .LastByTradeDay()
            .AttachTodayProfitAndLoss()
            .ToList();
    }

    private static IEnumerable<BasicTransformData> TypeOfTransformData(this IEnumerable<BasicData> source)
    {
        return source.Select(data =>
        {
            return new BasicTransformData()
            {
                SignalDateTime = data.SignalDateTime!.Value,
                Equity = data.Equity!.Value,
            };
        });
    }

    private static IEnumerable<BasicTransformData> SkipAndInitStartEquity(this IEnumerable<BasicTransformData> source)
    {
        using var e = source.GetEnumerator();

        if (!e.MoveNext()) throw new InvalidOperationException("没有数据");

        var first = e.Current;

        bool endSkip = false;

        var preData = first;

        while (e.MoveNext())
        {
            var data = e.Current;

            data.StartEquity = first.Equity;

            if (endSkip)
            {
                yield return data;
            }
            else if (endSkip = first.Equity != data.Equity)
            {
                yield return preData;
            }
            else
            {
                preData = data;
            }
        }
    }

    /// <summary>
    /// 附加上交易日
    /// </summary>
    /// <param name="source"></param>
    public static IEnumerable<BasicTransformData> AttachTradeDay(this IEnumerable<BasicTransformData> source)
    {
        var tradeDate = new ConvertTradeDate(source.Select(x => x.SignalDateTime));

        using var e = source.GetEnumerator();

        while (e.MoveNext())
        {
            var data = e.Current;

            var dateTime = data.SignalDateTime;

            data.TradeDate = tradeDate.GetTradeDate(data.SignalDateTime);

            yield return data;
        }
    }

    /// <summary>
    /// 根据交易日保留最后一条数据
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<BasicTransformData> LastByTradeDay(this IEnumerable<BasicTransformData> source)
    {
        foreach (var item in source.GroupBy(x => x.TradeDate))
        {
            yield return item.Last();
        }
    }

    /// <summary>
    /// 计算今日盈亏
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<BasicTransformData> AttachTodayProfitAndLoss(this IEnumerable<BasicTransformData> source)
    {
        using var e = source.GetEnumerator();

        BasicTransformData? preData = null;

        while (e.MoveNext())
        {
            var data = e.Current;

            if (preData == null)
            {
                data.TodayProfitAndLoss = data.Equity - data.StartEquity;
            }
            else
            {
                data.TodayProfitAndLoss =Math.Round(data.Equity - preData.Equity,2);
            }

            preData = data;

            yield return data;
        }
    }
}
