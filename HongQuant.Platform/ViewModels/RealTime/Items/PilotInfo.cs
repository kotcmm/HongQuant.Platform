using HongQuant.Analysis.RealTime;
using LiveChartsCore.Defaults;
using MahApps.Metro.Controls;
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
                var change = changes.LastOrDefault();

                if(change.Current != null)
                {
                    Value = change.Current.ProfitAndLoss;
                    Position = change.Current.Position;
                }
            }).Subscribe();

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
