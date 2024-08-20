using HongQuant.Analysis.RealTime;
using LiveChartsCore.Defaults;
using ReactiveUI;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class ProfitAndLossInfo : ObservableValue, IDisposable
{
    private readonly IDisposable _disposed;

    public ProfitAndLossInfo(RealTimeDataSource dataSource,Func<AnalyzedData, double> func)
    {
        Name = dataSource.Product;

        _disposed = dataSource.AnalyzedDatas
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(changes => 
            {
                var change = changes.LastOrDefault();

                if(change.Current != null)
                {
                    Value = func?.Invoke(change.Current);
                    Position = change.Current.Position;
                    _analyzedData = change.Current;
                }
            }).Subscribe();

    }

    private AnalyzedData? _analyzedData;

    public void UpdateValue(CycleType cycleType)
    {
        if (_analyzedData == null) return;

        Value = cycleType switch
        {
            CycleType.HalfAnHour => _analyzedData.ProfitAndLoss,
            CycleType.Day => _analyzedData.DayProfitAndLoss,
            CycleType.Week => _analyzedData.WeekProfitAndLoss,
            CycleType.Month => _analyzedData.MonthProfitAndLoss,
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// 品种
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 持仓状态
    /// </summary>
    public int Position { get; set; }

    public void Dispose()
    {
        _disposed.Dispose();
    }
}
