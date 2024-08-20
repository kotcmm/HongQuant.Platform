using HongQuant.Analysis.RealTime;
using ReactiveUI;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class AnalyzedDataViewModel
{
    private readonly IDisposable _disposed;

    public AnalyzedDataViewModel(RealTimeDataSource dataSource)
    {
        Name = dataSource.Product;

        ProfitAndLoss = new ProfitAndLossInfo(Name, 0);
        DayProfitAndLoss = new ProfitAndLossInfo(Name, 0);
        WeekProfitAndLoss = new ProfitAndLossInfo(Name, 0);
        MonthProfitAndLoss = new ProfitAndLossInfo(Name, 0);

        _disposed = dataSource.AnalyzedDatas
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(changes =>
            {
                var change = changes.LastOrDefault();

                if (change.Current != null)
                {
                    AnalyzedData = change.Current;
                    ProfitAndLoss.Value = change.Current.ProfitAndLoss;
                    DayProfitAndLoss.Value = change.Current.DayProfitAndLoss;
                    WeekProfitAndLoss.Value = change.Current.WeekProfitAndLoss;
                    MonthProfitAndLoss.Value = change.Current.MonthProfitAndLoss;
                }
            }).Subscribe();

    }

    public bool HasPosition()
    {
        return AnalyzedData?.Position != 0; 
    }

    public AnalyzedData? AnalyzedData { get; private set; }

    /// <summary>
    /// 净盈亏
    /// </summary>
    public ProfitAndLossInfo ProfitAndLoss { get; private set; }

    /// <summary>
    /// 日盈亏
    /// </summary>
    public ProfitAndLossInfo DayProfitAndLoss { get; private set; }

    /// <summary>
    /// 周盈亏
    /// </summary>
    public ProfitAndLossInfo WeekProfitAndLoss { get; private set; }

    /// <summary>
    /// 月盈亏
    /// </summary>
    public ProfitAndLossInfo MonthProfitAndLoss { get; private set; }

    /// <summary>
    /// 品种
    /// </summary>
    public string Name { get; set; }

    public void Dispose()
    {
        _disposed.Dispose();
    }
}
