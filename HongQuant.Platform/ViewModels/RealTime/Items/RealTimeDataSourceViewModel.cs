using DynamicData;
using HongQuant.Analysis.RealTime;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class RealTimeDataSourceViewModel
{
    public RealTimeDataSourceViewModel(RealTimeDataSource realTimeData)
    {
        Product = realTimeData.Product;
        Strategy = realTimeData.Strategy;

        realTimeData.AnalyzedDatas
            .Connect()
            .Transform(x=>new RealTimeDataViewModel(x))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _items)
            .DisposeMany()
            .Subscribe();
    }

    public bool IsSelected {  get; set; }

    public string Product { get; set; }

    public string Strategy { get; set; }

    private readonly ReadOnlyObservableCollection<RealTimeDataViewModel> _items;
    public ReadOnlyObservableCollection<RealTimeDataViewModel> Items => _items;
}
