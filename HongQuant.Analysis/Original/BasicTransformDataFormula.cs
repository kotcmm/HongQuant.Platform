namespace HongQuant.Analysis.Original;
public static class BasicTransformDataFormula
{
    public static IEnumerable<BasicTransformData> TransformBasicDatas(this IEnumerable<BasicData> basicDatas, string product = "")
    {
        return basicDatas
            .TypeOfTransformData(product)
            .SkipAndInitStartEquity()
            .AttachTradeDay()
            .LastByTradeDay()
            .AttachTodayProfitAndLoss()
            .AttachYields()
            .AttachTotalYields()
            .AttachWithdrawDeep()
            .AttachMaxWithdrawAndLevel()
            .ToList();
    }

    public static IEnumerable<BasicTransformData> AttachMeragedBasicDatas(this IEnumerable<BasicTransformData> source)
    {
        return source
            .AttachYields()
            .AttachTotalYields()
            .AttachWithdrawDeep()
            .AttachMaxWithdrawAndLevel()
            .ToList();
    }

    /// <summary>
    /// 根据交易日合并数据
    /// </summary>
    /// <param name="basicDataFiles"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static IEnumerable<BasicTransformData> MeragedByTradeDate(this IEnumerable<IDataFile> basicDataFiles,int count = 0)
    {
        return basicDataFiles.SelectMany(x => x.Items)
            .GroupBy(x => x.TradeDate)
            .Select(x =>
            {
                var datas = x.ToList();

                return new BasicTransformData()
                {
                    Product = $"{count}({datas.Count})",
                    TradeDate = x.Key,
                    Equity = Math.Round(datas.Sum(x => x.Equity),2),
                    Position = datas.Sum(x => Math.Abs(x.Position)),
                    LongPosition = datas.Count(x => x.Position == 1),
                    ShortPosition = datas.Count(x => x.Position == -1),
                    StartEquity = datas.Sum(x => x.StartEquity),
                    TodayProfitAndLoss = Math.Round(datas.Sum(x => x.TodayProfitAndLoss),2),
                };
            })
            .OrderBy(x=>x.TradeDate);
    }


    private static IEnumerable<BasicTransformData> TypeOfTransformData(this IEnumerable<BasicData> source, string product = "")
    {
        return source.Select(data =>
        {
            return new BasicTransformData()
            {
                Product = product,
                SignalDateTime = data.SignalDateTime!.Value,
                Equity = data.Equity!.Value,
                Position = data.Position!.Value,
            };
        }).OrderBy(x=>x.SignalDateTime);
    }

    /// <summary>
    /// 过滤没有信号的前面数据，并赋值初始权益
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// 今日盈亏=今日最后一条权益-昨天最后一条权益
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

    /// <summary>
    /// 计算收益率
    /// 今日收益率=今日盈亏/本金
    /// </summary>
    /// <param name="source"></param>
    public static IEnumerable<BasicTransformData> AttachYields(this IEnumerable<BasicTransformData> source)
    {
        using var e = source.GetEnumerator();

        while (e.MoveNext())
        {
            var data = e.Current;

            data.Yields = data.TodayProfitAndLoss / data.StartEquity;

            yield return data;
        }
    }

    /// <summary>
    /// 计算累计收益率
    /// 累计收益率 = 当前收益率 + 上一条累计收益率
    /// 如果没有上一条收益率，就直接用当前收益率
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<BasicTransformData> AttachTotalYields(this IEnumerable<BasicTransformData> source)
    {
        return source.MyAggregate((f, p, c) =>
        {
            c.TotalYields = c.Yields + p.TotalYields;
        }, f =>
        {
            f.TotalYields = f.Yields;
            return f;
        });
    }

    /// <summary>
    /// 计算实时回撤
    /// 实时回撤 = 当前累计收益率 - 历史最大累计收益率 (包含当前)
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<BasicTransformData> AttachWithdrawDeep(this IEnumerable<BasicTransformData> source)
    {
        SortedSet<double> historyTotalYields = [];

        using var e = source.GetEnumerator();

        while (e.MoveNext())
        {
            var data = e.Current;

            historyTotalYields.Add(data.TotalYields);

            if (historyTotalYields.Count == 1)
            {
                data.WithdrawDeep = data.TotalYields >= 0 ? 0 : data.TotalYields;
            }
            else
            {
                data.WithdrawDeep = data.TotalYields - historyTotalYields.Max;
            }

            yield return data;
        }
    }

    /// <summary>
    /// 计算最大回撤和回撤程度
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<BasicTransformData> AttachMaxWithdrawAndLevel(this IEnumerable<BasicTransformData> source)
    {
        SortedSet<double> historyWithdrawDeeps = [];

        using var e = source.GetEnumerator();

        while (e.MoveNext())
        {
            var data = e.Current;

            historyWithdrawDeeps.Add(data.WithdrawDeep);

            data.MaxWithdraw = historyWithdrawDeeps.Min;
            data.WithdrawLevel = data.WithdrawDeep / (Math.Abs(data.MaxWithdraw) < 0.0001 ? 1 : data.MaxWithdraw);

            yield return data;
        }
    }
}
