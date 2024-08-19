using HongQuant.Analysis.RealTime;
using LiveChartsCore.Defaults;
using ReactiveUI;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class PilotInfo : ObservableValue, IDisposable
{
    private readonly IDisposable _disposed;

    public PilotInfo(RealTimeDataSource dataSource)
    {
        Name = dataSource.Product;

        _disposed = dataSource.AnalyzedDatas
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Do(changes => 
            {
                foreach (var change in changes) {
                    Value = change.Current.TodayProfitAndLoss;
                };
            }).Subscribe();

    }

    /// <summary>
    /// 品种
    /// </summary>
    public string Name { get; set; }

    public void Dispose()
    {
        _disposed.Dispose();
    }
}
