using HongQuant.Analysis.Hong.Domain;

namespace HongQuant.Analysis.Hong.Formula;
internal static class SignalDataFormula
{
    public static IEnumerable<SignalData> ToSignalDatas(this IEnumerable<OriginalData> originalDatas, SignalDataConfig config)
    {
        return originalDatas
            .TypeOfSignalData()
            .AttachYields()
            .AttachTotalYields()
            .AttachWithdrawDeep()
            .AttachMaxWithdrawAndLevel()
            .AttachPotentialRisk()
            .AttachTradeDay()
            .AttachDayDynamicLevel(config.DayMaxLevel)
            .AttachWeekDynamicLevel(config.WeekMaxLevel)
            .AttachMonthDynamicLevel(config.MonthMaxLevel)
            .AttachEntranceState(config.Entrance, config.Quit, config.Stop)
            .AttachAccurateTotalYields()
            .AttachContinuousLoss()
            .AttachProfitCycle()
            .AttachPositionCycle()
            .ToList();
    }

    /// <summary>
    /// OriginalData转换SignalData
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private static IEnumerable<SignalData> TypeOfSignalData(this IEnumerable<OriginalData> source)
    {
        using var e = source.GetEnumerator();

        var preSignalChange = SignalChange.None;

        while (e.MoveNext())
        {
            var data = e.Current;

            var signalChange = preSignalChange;

            if (data.SignalChange != SignalChange.Null)
            {
                preSignalChange = data.SignalChange;

                if (preSignalChange == SignalChange.Buy ||
                    preSignalChange == SignalChange.Sell)
                {
                    signalChange = preSignalChange;
                }
            }

            var direction = signalChange switch
            {
                SignalChange.None => DirectionState.None,
                SignalChange.Buy => DirectionState.Long,
                SignalChange.Sell => DirectionState.Short,
                _ => throw new NotImplementedException()
            };

            //TODO: 平仓要算下一个才改变状态
            yield return new SignalData()
            {
                Name = data.Name!,
                SignalDateTime = data.SignalDateTime!.Value,
                Equity = data.Equity!.Value,
                Direction = direction,
                SignalChange = data.SignalChange,
            };
        }
    }

    /// <summary>
    /// 自己定义的累计通用逻辑
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <param name="firstFunc"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static IEnumerable<T> MyAggregate<T>(this IEnumerable<T> source, Action<T, T, T> action, Func<T, T>? firstFunc = null)
    {
        using var e = source.GetEnumerator();

        if (!e.MoveNext()) throw new InvalidOperationException("没有数据");

        var first = firstFunc != null ? firstFunc(e.Current) : e.Current;

        yield return first;

        var preData = e.Current;

        while (e.MoveNext())
        {
            var data = e.Current;

            action(first, preData, data);

            preData = e.Current;

            yield return data;
        }
    }

    /// <summary>
    /// 计算收益率
    /// 收益率 = （当前权益-上一条权益） / 初始权益 
    /// 如果没有上一条权益，收益率为0
    /// </summary>
    /// <param name="source"></param>
    private static IEnumerable<SignalData> AttachYields(this IEnumerable<SignalData> source)
    {
        return source.MyAggregate((f, p, c) =>
        {
            c.Yields = (c.Equity - p.Equity) / f.Equity;
        });
    }

    /// <summary>
    /// 计算累计收益率
    /// 累计收益率 = 当前收益率 + 上一条累计收益率
    /// 如果没有上一条收益率，就直接用当前收益率
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<SignalData> AttachTotalYields(this IEnumerable<SignalData> source)
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
    public static IEnumerable<SignalData> AttachWithdrawDeep(this IEnumerable<SignalData> source)
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
    public static IEnumerable<SignalData> AttachMaxWithdrawAndLevel(this IEnumerable<SignalData> source)
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

    /// <summary>
    /// 计算潜在风险
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<SignalData> AttachPotentialRisk(this IEnumerable<SignalData> source)
    {
        using var e = source.GetEnumerator();

        while (e.MoveNext())
        {
            var data = e.Current;

            data.PotentialRisk = data.MaxWithdraw - data.WithdrawDeep;

            yield return data;
        }
    }

    /// <summary>
    /// 附加上交易日
    /// </summary>
    /// <param name="source"></param>
    public static IEnumerable<SignalData> AttachTradeDay(this IEnumerable<SignalData> source)
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
    /// 计算日度动态等级
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<SignalData> AttachDayDynamicLevel(this IEnumerable<SignalData> source, int takeCount)
    {
        var modelGroup = source.GroupDay();

        return DynamicLevel(modelGroup, takeCount, (m, l) =>
        {
            m.DayLevel = l;
        });
    }

    /// <summary>
    /// 计算周度动态等级
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<SignalData> AttachWeekDynamicLevel(this IEnumerable<SignalData> source, int takeCount)
    {
        var modelGroup = source.GroupWeek();

        return DynamicLevel(modelGroup, takeCount, (m, l) =>
        {
            m.WeekLevel = l;
        });
    }

    /// <summary>
    /// 计算月度动态等级
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<SignalData> AttachMonthDynamicLevel(this IEnumerable<SignalData> source, int takeCount)
    {
        var modelGroup = source.GroupMonth();

        return DynamicLevel(modelGroup, takeCount, (m, l) =>
        {
            m.MonthLevel = l;
        });
    }

    /// <summary>
    /// 动态等级计算
    /// 假设总等级为5
    ///
    /// 净利润     等级
    /// a
    /// b
    /// c
    /// d
    /// e         等级等于。e、e+d、e+d+c、e+d+c+b、e+d+c+b+a 中小于等于0的个数
    ///
    /// 其中e为映射到天的数据。
    /// 第一天         e=1
    /// 第二天         e=1+2
    /// 第三天         e=1+2+3
    /// 第四天         e=1+2+3+4
    /// ......
    /// </summary>
    /// <param name="modelMaps"></param>
    /// <param name="takeCount"></param>
    /// <param name="action"></param>
    private static IEnumerable<SignalData> DynamicLevel(IEnumerable<CycleGroup> modelMaps, int takeCount, Action<SignalData, int> action)
    {
        FixList<CycleGroup> fixList = new(takeCount);

        foreach (var modelMap in modelMaps)
        {
            if (!fixList.Add(modelMap))
            {
                foreach (var modelMapChild in modelMap.Children)
                {
                    yield return modelMapChild;
                }

                continue;
            }

            var sumList = fixList.SumWithoutLast(t => t.Children.Sum(x => x.Yields));

            double todaySum = 0;

            foreach (var modelMapChild in modelMap.Children)
            {
                todaySum += modelMapChild.Yields;

                var allSumList = new List<double>()
                    {
                        todaySum
                    };

                for (int i = sumList.Count - 1; i >= 0; i--)
                {
                    allSumList.Add(sumList.Skip(i).Sum() + todaySum);
                }

                var level = allSumList.Count(s => s <= 0);

                action(modelMapChild, level);

                yield return modelMapChild;
            }
        }
    }

    /// <summary>
    ///     计算精准状态
    /// </summary>
    /// <param name="source"></param>
    /// <param name="entrance"></param>
    /// <param name="quit"></param>
    /// <returns></returns>
    public static IEnumerable<SignalData> AttachEntranceState(this IEnumerable<SignalData> source, double entrance, double quit, double stop)
    {
        EntranceState runState = EntranceState.Out;

        using var e = source.GetEnumerator();

        while (e.MoveNext())
        {
            var data = e.Current;

            var entranceState = runState;

            if (runState == EntranceState.Out && data.WithdrawLevel >= entrance)
            {
                runState = EntranceState.Entrance;
                //进场
            }
            else if (runState == EntranceState.Entrance && data.WithdrawLevel >= stop)
            {
                runState = EntranceState.Stop;
                //止损
            }
            else if (runState != EntranceState.Out && data.WithdrawLevel <= quit)
            {
                runState = EntranceState.Out;
                //出场
            }

            data.EntranceState = entranceState;

            yield return data;
        }
    }

    /// <summary>
    /// 计算精准累计收益率
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<SignalData> AttachAccurateTotalYields(this IEnumerable<SignalData> source)
    {
        using var e = source.GetEnumerator();

        if (!e.MoveNext()) throw new InvalidOperationException("没有数据");

        var preData = e.Current;

        yield return preData;

        while (e.MoveNext())
        {
            var data = e.Current;

            if (data.EntranceState == EntranceState.Entrance)
            {
                data.AccurateTotalYields = preData.AccurateTotalYields + data.Yields;
            }
            else
            {
                data.AccurateTotalYields = preData.AccurateTotalYields;
            }

            preData = data;

            yield return data;
        }
    }

    /// <summary>
    ///     计算持仓盈亏和连续亏损次数
    ///     亏损次数是安装开仓状态计算的，
    /// </summary>
    /// <param name="source"></param>
    public static IEnumerable<SignalData> AttachContinuousLoss(this IEnumerable<SignalData> source)
    {
        int lossCount = 0;
        int maxLossCount = 0;
        double preEquity = 0;

        using var e = source.GetEnumerator();

        while (e.MoveNext())
        {
            var data = e.Current;

            if (data.Direction == DirectionState.None)
            {
                preEquity = data.Equity;
                data.LossCount = lossCount;
                data.MaxLossCount = maxLossCount;
                data.LossEquity = 0;
            }
            else
            {
                var lossEquity = data.Equity - preEquity;

                data.LossEquity = lossEquity;

                data.LossCount = lossEquity < 0 ? lossCount + 1 : 0;

                if (data.LossCount > maxLossCount)
                {
                    maxLossCount = data.LossCount;
                }

                data.MaxLossCount = maxLossCount;

                if (data.SignalChange == SignalChange.None)
                {
                    lossCount = lossEquity >= 0 ? 0 : data.LossCount;
                }
            }

            data.LostDegree = data.MaxLossCount == 0 ?
                                    0 :
                                    data.LossCount / (double)data.MaxLossCount;


            yield return data;
        }
    }

    /// <summary>
    /// 盈利周期
    /// </summary>
    /// <param name="source"></param>
    public static IEnumerable<SignalData> AttachProfitCycle(this IEnumerable<SignalData> source)
    {
        int cycle = 0;
        int maxCycle = 0;
        bool start = false;

        using var e = source.GetEnumerator();

        while (e.MoveNext())
        {
            var data = e.Current;

            if (data.SignalChange == SignalChange.None && data.LossEquity >= 0)
            {
                start = true;
                cycle = 0;
            }

            if (!start)
            {
                yield return data;
                continue;
            }

            if (data.Direction != DirectionState.None)
            {
                data.PreProfitCycle = data.LossEquity >= 0 ? 0 : cycle;
            }
            else
            {
                data.PreProfitCycle = cycle;
            }

            if (maxCycle < data.PreProfitCycle)
            {
                maxCycle = data.PreProfitCycle;
            }

            data.MaxProfitCycle = maxCycle;
            data.ProfitCycleDegree = data.MaxProfitCycle == 0 ?
                0 :
                data.PreProfitCycle / (double)data.MaxProfitCycle;
            cycle++;

            yield return data;
        }
    }

    /// <summary>
    /// 空仓或者持仓周期
    /// </summary>
    /// <param name="source"></param>
    public static IEnumerable<SignalData> AttachPositionCycle(this IEnumerable<SignalData> source)
    {
        int emptyCycle = 1;
        int maxEmptyCycle = 0;
        int positionCycle = 1;
        int maxPositionCycle = 1;

        var preDirection = DirectionState.None;
        bool hasPosition = false;

        using var e = source.GetEnumerator();

        while (e.MoveNext())
        {
            var data = e.Current;

            if (data.Direction != preDirection && (
                data.Direction == DirectionState.None ||
                preDirection == DirectionState.None))
            {
                preDirection = data.Direction;
                positionCycle = 1;
                emptyCycle = 1;
            }

            if (data.Direction != DirectionState.None)
            {
                maxPositionCycle = maxPositionCycle >= positionCycle ?
                    maxPositionCycle : positionCycle;
                data.PositionCycle = positionCycle++;
                data.PositionDegree = maxPositionCycle == 0 ?
                    0 :
                    data.PositionCycle / (double)maxPositionCycle;

                hasPosition = true;
            }
            else if (hasPosition)
            {
                maxEmptyCycle = maxEmptyCycle >= emptyCycle ?
                    maxEmptyCycle : emptyCycle;
                data.EmptyCycle = emptyCycle++;
                data.EmptyDegree = maxEmptyCycle == 0 ?
                    0 :
                    data.EmptyCycle / (double)maxEmptyCycle;
            }

            data.MaxPositionCycle = maxPositionCycle;
            data.MaxEmptyCycle = maxEmptyCycle;

            yield return data;
        }
    }
}
